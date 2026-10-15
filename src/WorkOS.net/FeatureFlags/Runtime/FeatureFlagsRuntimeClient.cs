// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Polls bulk configuration and evaluates boolean flags locally from immutable snapshots.
    /// Own one instance per environment/API key, and dispose it asynchronously before its WorkOSClient.
    /// </summary>
    public sealed class FeatureFlagsRuntimeClient : IAsyncDisposable
    {
        private const int MinimumPollingIntervalMilliseconds = 5_000;
        private const int MinimumDelayMilliseconds = 1_000;
        private const int InitialRetryMilliseconds = 1_000;
        private const double JitterFactor = 0.1;
        private const uint MaximumTimerMilliseconds = uint.MaxValue - 1;
        private readonly WorkOSClient client;
        private readonly TimeSpan pollingInterval;
        private readonly TimeSpan requestTimeout;
        private readonly TimeSpan maxRetryDelay;
        private readonly TimeProvider timeProvider;
        private readonly IProgress<FeatureFlagsRuntimeStatus>? progress;
        private readonly object lifecycleLock = new object();
        private readonly object reportLock = new object();
        private readonly HashSet<Task> pendingRefreshes = new HashSet<Task>();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly SemaphoreSlim refreshGate = new SemaphoreSlim(1, 1);
        private readonly TaskCompletionSource ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        private FeatureFlagsRuntimeStatus status;
        private Task reportChain = Task.CompletedTask;
        private Task? pollingTask;
        private Task? disposalTask;
        private bool disposed;

        /// <summary>Initializes a new instance of the <see cref="FeatureFlagsRuntimeClient"/> class without starting network requests.</summary>
        /// <param name="client">Caller-owned SDK transport.</param>
        /// <param name="options">Polling, bootstrap, and status options.</param>
        public FeatureFlagsRuntimeClient(WorkOSClient client, FeatureFlagsRuntimeOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(client);
            options ??= new FeatureFlagsRuntimeOptions();
            var minimumPollingInterval = TimeSpan.FromMilliseconds(MinimumPollingIntervalMilliseconds);
            var pollingInterval = options.PollingInterval < minimumPollingInterval ? minimumPollingInterval : options.PollingInterval;
            ValidateDuration(pollingInterval, nameof(options.PollingInterval));
            ValidateDuration(options.RequestTimeout, nameof(options.RequestTimeout));
            ValidateDuration(options.MaxRetryDelay, nameof(options.MaxRetryDelay));

            this.client = client;
            this.pollingInterval = pollingInterval;
            this.requestTimeout = options.RequestTimeout;
            this.maxRetryDelay = options.MaxRetryDelay;
            this.timeProvider = options.TimeProvider ?? TimeProvider.System;
            this.progress = options.Progress;
            this.status = new FeatureFlagsRuntimeStatus(options.BootstrapConfiguration, null, null);
            if (this.status.IsReady)
            {
                this.ready.TrySetResult();
            }
        }

        /// <summary>Current readiness, snapshot, refresh timestamp, and last error.</summary>
        public FeatureFlagsRuntimeStatus Status => Volatile.Read(ref this.status);

        /// <summary>Whether a validated configuration is available for local evaluation.</summary>
        public bool IsReady => this.Status.IsReady;

        /// <summary>Current immutable snapshot, suitable for persisting with ToJson. Remains readable after disposal.</summary>
        public FeatureFlagsConfiguration? Configuration => this.Status.Configuration;

        private FeatureFlagsConfiguration? EvaluableConfiguration => Volatile.Read(ref this.disposed) ? null : this.Configuration;

        /// <summary>
        /// Starts background polling once and waits for a valid configuration. Bootstrap makes
        /// this wait immediate. Cancellation cancels only this caller's wait; dispose the runtime
        /// to stop polling. Failed polls are retried with bounded exponential backoff and jitter.
        /// Authentication/permission failures stop background polling; fix credentials/access and
        /// create a new runtime. A bootstrap or last good snapshot remains usable.
        /// </summary>
        /// <param name="cancellationToken">Cancellation for the readiness wait.</param>
        /// <returns>A task completed when configuration is available.</returns>
        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            lock (this.lifecycleLock)
            {
                this.ThrowIfDisposed();
                this.pollingTask ??= Task.Run(this.RunPollingAsync);
                return this.IsReady ? Task.CompletedTask : this.ready.Task.WaitAsync(cancellationToken);
            }
        }

        /// <summary>Waits for a validated bootstrap or successful refresh without starting polling.</summary>
        /// <param name="cancellationToken">Cancellation for this wait.</param>
        /// <returns>A task completed when configuration is available.</returns>
        public Task WaitUntilReadyAsync(CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();
            return this.IsReady ? Task.CompletedTask : this.ready.Task.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Performs a complete refresh through the SDK transport. Concurrent refreshes are
        /// serialized, and the timeout includes waiting for an earlier refresh. Errors propagate
        /// to the caller and retain the last good snapshot.
        /// </summary>
        /// <param name="cancellationToken">Cancellation for this refresh.</param>
        /// <returns>A task completed after a new validated snapshot is installed.</returns>
        public Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            CancellationTokenSource timeout;
            CancellationTokenSource cancellation;
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (this.lifecycleLock)
            {
                this.ThrowIfDisposed();
                timeout = new CancellationTokenSource(this.requestTimeout, this.timeProvider);
                cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.lifetime.Token, timeout.Token);

                // Register before waiting for the gate so disposal can join queued refreshes.
                this.pendingRefreshes.Add(completion.Task);
            }

            _ = this.RefreshAndCompleteAsync(cancellationToken, timeout, cancellation, completion);
            return completion.Task;
        }

        /// <summary>
        /// Evaluates locally with no network request. Missing flags, an unready or disposed client,
        /// and flags without a boolean answer return defaultValue. For enabled flags, the first
        /// matching rule wins; otherwise the flag default applies. Disabled flags serve their off value.
        /// </summary>
        /// <param name="slug">Flag slug.</param>
        /// <param name="context">Optional exact target IDs.</param>
        /// <param name="defaultValue">Caller fallback for a missing flag, answer, or configuration.</param>
        /// <returns>The evaluated boolean value.</returns>
        public bool IsEnabled(string slug, FeatureFlagContext? context = null, bool defaultValue = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slug);
            var configuration = this.EvaluableConfiguration;
            return configuration != null && configuration.Flags.TryGetValue(slug, out var flag)
                ? flag.Evaluate(context) ?? defaultValue
                : defaultValue;
        }

        /// <summary>
        /// Evaluates all flags from one snapshot. Flags without a boolean answer are omitted. An
        /// unready or disposed client returns an empty map.
        /// </summary>
        /// <param name="context">Optional exact target IDs.</param>
        /// <returns>A read-only slug to boolean map, including false values.</returns>
        public IReadOnlyDictionary<string, bool> GetAllFlags(FeatureFlagContext? context = null)
        {
            var configuration = this.EvaluableConfiguration;
            if (configuration == null)
            {
                return FrozenDictionary<string, bool>.Empty;
            }

            var values = new Dictionary<string, bool>(configuration.Count, StringComparer.Ordinal);
            foreach (var entry in configuration.Flags)
            {
                if (entry.Value.Evaluate(context) is bool value)
                {
                    values[entry.Key] = value;
                }
            }

            return values;
        }

        /// <summary>
        /// Returns one flag's configuration from the current snapshot, for inspection and debugging.
        /// Use <see cref="IsEnabled"/> for decisions. Like <see cref="Configuration"/>, it remains
        /// readable after disposal.
        /// </summary>
        /// <param name="slug">Flag slug.</param>
        /// <returns>The flag's configuration, or null when the client is unready or the flag is missing.</returns>
        public FeatureFlagDefinition? GetFlag(string slug)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slug);
            return this.Configuration is { } configuration && configuration.TryGetFlag(slug, out var flag) ? flag : null;
        }

        /// <summary>Cancels and joins polling and all active or queued refreshes before releasing resources.</summary>
        /// <returns>A task completed when network activity has stopped.</returns>
        public ValueTask DisposeAsync()
        {
            var refreshes = Array.Empty<Task>();
            TaskCompletionSource? completion = null;
            Task disposal;
            lock (this.lifecycleLock)
            {
                if (this.disposalTask == null)
                {
                    Volatile.Write(ref this.disposed, true);
                    refreshes = new Task[this.pendingRefreshes.Count];
                    this.pendingRefreshes.CopyTo(refreshes);
                    completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                    this.disposalTask = completion.Task;
                }

                disposal = this.disposalTask;
            }

            if (completion != null)
            {
                // Cancellation runs registered callbacks inline; never own the lifecycle lock while it does.
                _ = this.DisposeCoreAsync(refreshes, completion);
            }

            return new ValueTask(disposal);
        }

        private static void ValidateDuration(TimeSpan value, string name)
        {
            if (value < TimeSpan.FromMilliseconds(1) || value.TotalMilliseconds > MaximumTimerMilliseconds)
            {
                throw new ArgumentOutOfRangeException(name, "Duration must be at least one millisecond and supported by .NET timers.");
            }
        }

        private async Task RefreshAndCompleteAsync(
            CancellationToken cancellationToken,
            CancellationTokenSource timeout,
            CancellationTokenSource cancellation,
            TaskCompletionSource completion)
        {
            try
            {
                using (timeout)
                using (cancellation)
                {
                    await this.refreshGate.WaitAsync(cancellation.Token).ConfigureAwait(false);
                    var attempted = this.timeProvider.GetUtcNow();
                    try
                    {
                        // Cancellation callbacks may not yet have reached this linked token.
                        this.lifetime.Token.ThrowIfCancellationRequested();
                        var request = new WorkOSRequest { Method = HttpMethod.Get, Path = "/sdk/feature-flags" };
                        request.AddQueryParam("payload_version", "2");
                        using var response = await this.client.MakeRawAPIRequest(request, cancellation.Token).ConfigureAwait(false);
                        var json = await response.Content.ReadAsStringAsync(cancellation.Token).ConfigureAwait(false);
                        var configuration = FeatureFlagsConfiguration.FromJson(json);
                        cancellation.Token.ThrowIfCancellationRequested();
                        var refreshed = this.timeProvider.GetUtcNow();
                        this.Publish(previous => new FeatureFlagsRuntimeStatus(
                            configuration,
                            refreshed,
                            null,
                            previous.RefreshCount + 1,
                            previous.FailedRefreshCount,
                            attempted,
                            previous.IsPollingStopped));
                        this.ready.TrySetResult();
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested || this.lifetime.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        var error = exception is OperationCanceledException
                            ? new TimeoutException("Feature flag configuration refresh exceeded its request timeout.", exception)
                            : exception;
                        this.Publish(previous => new FeatureFlagsRuntimeStatus(
                            previous.Configuration,
                            previous.LastSuccessfulRefresh,
                            error,
                            previous.RefreshCount + 1,
                            previous.FailedRefreshCount + 1,
                            attempted,
                            previous.IsPollingStopped));
                        if (error != exception)
                        {
                            throw error;
                        }

                        throw;
                    }
                    finally
                    {
                        this.refreshGate.Release();
                    }
                }

                completion.TrySetResult();
            }
            catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested && !this.lifetime.IsCancellationRequested)
            {
                // Queue timeouts belong to this caller, not the in-flight network refresh's status.
                completion.TrySetException(new TimeoutException("Feature flag configuration refresh exceeded its request timeout.", exception));
            }
            catch (OperationCanceledException exception)
            {
                completion.TrySetCanceled(exception.CancellationToken);
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
            finally
            {
                lock (this.lifecycleLock)
                {
                    this.pendingRefreshes.Remove(completion.Task);
                }
            }
        }

        private async Task RunPollingAsync()
        {
            var failures = 0;
            while (!this.lifetime.IsCancellationRequested)
            {
                try
                {
                    await this.RefreshAsync(this.lifetime.Token).ConfigureAwait(false);
                    failures = 0;
                }
                catch (Exception) when (this.lifetime.IsCancellationRequested)
                {
                    return;
                }
                catch (ApiException exception) when (exception.StatusCode == HttpStatusCode.Unauthorized || exception.StatusCode == HttpStatusCode.Forbidden)
                {
                    // Publish before failing readiness so a caller woken by it already sees the stop.
                    this.Publish(previous => previous.WithPollingStopped());
                    this.ready.TrySetException(exception);
                    return;
                }
                catch (Exception)
                {
                    failures = Math.Min(failures + 1, 20);
                }

                var baseDelay = this.pollingInterval.TotalMilliseconds;
                if (failures > 0)
                {
                    var backoff = Math.Min(this.maxRetryDelay.TotalMilliseconds, InitialRetryMilliseconds * Math.Pow(2, failures - 1));
                    baseDelay = Math.Max(baseDelay, backoff);
                }

                var jitter = 1 + (((Random.Shared.NextDouble() * 2) - 1) * JitterFactor);
                var milliseconds = Math.Min(MaximumTimerMilliseconds, Math.Max(MinimumDelayMilliseconds, baseDelay * jitter));
                var delay = TimeSpan.FromMilliseconds(milliseconds);

                try
                {
                    await Task.Delay(delay, this.timeProvider, this.lifetime.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (this.lifetime.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        private async Task DisposeCoreAsync(Task[] refreshes, TaskCompletionSource completion)
        {
            try
            {
                this.lifetime.Cancel();
                this.ready.TrySetCanceled(this.lifetime.Token);
                var polling = Volatile.Read(ref this.pollingTask);
                if (polling != null)
                {
                    await polling.ConfigureAwait(false);
                }

                try
                {
                    await Task.WhenAll(refreshes).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    // Refresh errors belong to their callers; all refreshes have now finished.
                }

                this.refreshGate.Dispose();
                this.lifetime.Dispose();
                completion.TrySetResult();
            }
            catch (Exception exception)
            {
                completion.TrySetException(exception);
            }
        }

        // Status transitions are derived from the current status, and a manual refresh can finish
        // while polling records a stop, so each transition and its report are published atomically.
        private void Publish(Func<FeatureFlagsRuntimeStatus, FeatureFlagsRuntimeStatus> transition)
        {
            lock (this.reportLock)
            {
                var value = transition(Volatile.Read(ref this.status));
                Volatile.Write(ref this.status, value);
                var progress = this.progress;
                if (progress == null)
                {
                    return;
                }

                // Report is called one at a time in refresh order, never on the polling task or while
                // owning the refresh gate, so a synchronous observer may dispose the runtime safely.
                // Progress<T> observers post onward to their own context; only the Report calls are ordered.
                this.reportChain = this.reportChain.ContinueWith(
                    _ =>
                    {
                        try
                        {
                            progress.Report(value);
                        }
                        catch (Exception)
                        {
                            // Observer failures must not interrupt polling or invalidate a snapshot.
                        }
                    },
                    CancellationToken.None,
                    TaskContinuationOptions.DenyChildAttach,
                    TaskScheduler.Default);
            }
        }

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(Volatile.Read(ref this.disposed), this);
    }
}
