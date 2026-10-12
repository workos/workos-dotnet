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
    /// Own one instance per environment/API key, and dispose it before its WorkOSClient.
    /// </summary>
    public sealed class FeatureFlagsRuntimeClient : IDisposable, IAsyncDisposable
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
        private readonly IProgress<FeatureFlagsRuntimeStatus>? progress;
        private readonly object lifecycleLock = new object();
        private readonly HashSet<Task> pendingRefreshes = new HashSet<Task>();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private readonly SemaphoreSlim refreshGate = new SemaphoreSlim(1, 1);
        private readonly TaskCompletionSource ready = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        private FeatureFlagsRuntimeStatus status;
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

        /// <summary>Current immutable snapshot, suitable for persisting with ToJson.</summary>
        public FeatureFlagsConfiguration? Configuration => this.Status.Configuration;

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
        /// serialized. Errors propagate to the caller and retain the last good snapshot.
        /// </summary>
        /// <param name="cancellationToken">Cancellation for this refresh.</param>
        /// <returns>A task completed after a new validated snapshot is installed.</returns>
        public Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            CancellationTokenSource cancellation;
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            lock (this.lifecycleLock)
            {
                this.ThrowIfDisposed();
                cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.lifetime.Token);

                // Register before waiting for the gate so disposal can join queued refreshes.
                this.pendingRefreshes.Add(completion.Task);
            }

            _ = this.RefreshAndCompleteAsync(cancellationToken, cancellation, completion);
            return completion.Task;
        }

        /// <summary>
        /// Evaluates locally with no network request. Missing flags or an unready client return
        /// defaultValue. For enabled flags, the first matching rule wins; otherwise the flag default
        /// applies. Disabled flags serve their off value.
        /// </summary>
        /// <param name="slug">Flag slug.</param>
        /// <param name="context">Optional exact target IDs.</param>
        /// <param name="defaultValue">Caller fallback for a missing flag/configuration.</param>
        /// <returns>The evaluated boolean value.</returns>
        public bool IsEnabled(string slug, FeatureFlagContext? context = null, bool defaultValue = false)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(slug);
            this.ThrowIfDisposed();
            var configuration = this.Configuration;
            return configuration != null && configuration.Flags.TryGetValue(slug, out var flag)
                ? flag.Evaluate(context)
                : defaultValue;
        }

        /// <summary>Evaluates all flags from one snapshot. An unready client returns an empty map.</summary>
        /// <param name="context">Optional exact target IDs.</param>
        /// <returns>An immutable slug to boolean map, including false values.</returns>
        public IReadOnlyDictionary<string, bool> GetAllFlags(FeatureFlagContext? context = null)
        {
            this.ThrowIfDisposed();
            var configuration = this.Configuration;
            return configuration == null
                ? FrozenDictionary<string, bool>.Empty
                : configuration.Flags.ToFrozenDictionary(entry => entry.Key, entry => entry.Value.Evaluate(context), StringComparer.Ordinal);
        }

        /// <summary>Cancels and joins polling. Does not dispose the caller-owned WorkOSClient.</summary>
        public void Dispose()
        {
            this.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        /// <summary>Cancels and joins polling and all active or queued refreshes before releasing resources.</summary>
        /// <returns>A task completed when network activity has stopped.</returns>
        public ValueTask DisposeAsync()
        {
            lock (this.lifecycleLock)
            {
                if (this.disposalTask == null)
                {
                    Volatile.Write(ref this.disposed, true);
                    var refreshes = new Task[this.pendingRefreshes.Count];
                    this.pendingRefreshes.CopyTo(refreshes);
                    this.disposalTask = this.DisposeCoreAsync(refreshes);
                }

                return new ValueTask(this.disposalTask);
            }
        }

        private static void ValidateDuration(TimeSpan value, string name)
        {
            if (value < TimeSpan.FromMilliseconds(1) || value.TotalMilliseconds > MaximumTimerMilliseconds)
            {
                throw new ArgumentOutOfRangeException(name, "Duration must be at least one millisecond and supported by .NET timers.");
            }
        }

        private async Task RefreshAndCompleteAsync(CancellationToken cancellationToken, CancellationTokenSource cancellation, TaskCompletionSource completion)
        {
            try
            {
                using (cancellation)
                {
                    await this.refreshGate.WaitAsync(cancellation.Token).ConfigureAwait(false);
                    try
                    {
                        // Cancellation callbacks may not yet have reached this linked token.
                        this.lifetime.Token.ThrowIfCancellationRequested();
                        cancellation.CancelAfter(this.requestTimeout);
                        var request = new WorkOSRequest { Method = HttpMethod.Get, Path = "/sdk/feature-flags" };
                        request.AddQueryParam("payload_version", "2");
                        using var response = await this.client.MakeRawAPIRequest(request, cancellation.Token).ConfigureAwait(false);
                        var json = await response.Content.ReadAsStringAsync(cancellation.Token).ConfigureAwait(false);
                        var configuration = FeatureFlagsConfiguration.FromJson(json);
                        cancellation.Token.ThrowIfCancellationRequested();
                        var successful = new FeatureFlagsRuntimeStatus(configuration, DateTimeOffset.UtcNow, null);
                        Volatile.Write(ref this.status, successful);
                        this.ready.TrySetResult();
                        this.ReportStatus(successful);
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
                        var previous = this.Status;
                        var failed = new FeatureFlagsRuntimeStatus(previous.Configuration, previous.LastSuccessfulRefresh, error);
                        Volatile.Write(ref this.status, failed);
                        this.ReportStatus(failed);
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
                    await Task.Delay(delay, this.lifetime.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (this.lifetime.IsCancellationRequested)
                {
                    return;
                }
            }
        }

        private async Task DisposeCoreAsync(Task[] refreshes)
        {
            this.lifetime.Cancel();
            this.ready.TrySetCanceled(this.lifetime.Token);
            if (this.pollingTask != null)
            {
                await this.pollingTask.ConfigureAwait(false);
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
        }

        private void ReportStatus(FeatureFlagsRuntimeStatus value)
        {
            if (this.progress != null)
            {
                // Never run application code on the polling task or while owning the refresh gate.
                // In particular, a synchronous observer may dispose the runtime safely.
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        this.progress.Report(value);
                    }
                    catch (Exception)
                    {
                        // Observer failures must not interrupt polling or invalidate a snapshot.
                    }
                });
            }
        }

        private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(Volatile.Read(ref this.disposed), this);
    }
}
