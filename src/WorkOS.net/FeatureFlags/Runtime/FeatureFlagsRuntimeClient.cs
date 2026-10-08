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
        private readonly WorkOSClient client;
        private readonly TimeSpan pollingInterval;
        private readonly TimeSpan requestTimeout;
        private readonly TimeSpan maxRetryDelay;
        private readonly IProgress<FeatureFlagsRuntimeStatus>? progress;
        private readonly object lifecycleLock = new object();
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
            ValidateDuration(options.PollingInterval, nameof(options.PollingInterval));
            ValidateDuration(options.RequestTimeout, nameof(options.RequestTimeout));
            ValidateDuration(options.MaxRetryDelay, nameof(options.MaxRetryDelay));
            if (options.MaxRetryDelay < options.PollingInterval)
            {
                throw new ArgumentOutOfRangeException(nameof(options.MaxRetryDelay), "Maximum retry delay must be at least the polling interval.");
            }

            this.client = client;
            this.pollingInterval = options.PollingInterval;
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
        public async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            CancellationTokenSource cancellation;
            lock (this.lifecycleLock)
            {
                this.ThrowIfDisposed();
                cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.lifetime.Token);
            }

            using (cancellation)
            {
                await this.refreshGate.WaitAsync(cancellation.Token).ConfigureAwait(false);
                try
                {
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

        /// <summary>Cancels and joins polling and any active refresh before releasing resources.</summary>
        /// <returns>A task completed when network activity has stopped.</returns>
        public ValueTask DisposeAsync()
        {
            lock (this.lifecycleLock)
            {
                if (this.disposalTask == null)
                {
                    Volatile.Write(ref this.disposed, true);
                    this.disposalTask = this.DisposeCoreAsync();
                }

                return new ValueTask(this.disposalTask);
            }
        }

        private static void ValidateDuration(TimeSpan value, string name)
        {
            if (value <= TimeSpan.Zero || value.TotalMilliseconds > uint.MaxValue - 1)
            {
                throw new ArgumentOutOfRangeException(name, "Duration must be positive and supported by .NET timers.");
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

                var delay = this.pollingInterval;
                if (failures > 0)
                {
                    var cap = Math.Min(this.maxRetryDelay.TotalMilliseconds, this.pollingInterval.TotalMilliseconds * Math.Pow(2, failures));
                    delay = TimeSpan.FromMilliseconds(cap * (0.5 + (Random.Shared.NextDouble() * 0.5)));
                }

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

        private async Task DisposeCoreAsync()
        {
            this.lifetime.Cancel();
            this.ready.TrySetCanceled(this.lifetime.Token);
            if (this.pollingTask != null)
            {
                await this.pollingTask.ConfigureAwait(false);
            }

            // Join manual refreshes too, including when background polling was never started.
            await this.refreshGate.WaitAsync().ConfigureAwait(false);
            this.refreshGate.Release();
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
