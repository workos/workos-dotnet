// @oagen-ignore-file
namespace WorkOS
{
    using System;

    /// <summary>An immutable view of readiness and the most recent refresh result.</summary>
    public sealed class FeatureFlagsRuntimeStatus
    {
        internal FeatureFlagsRuntimeStatus(
            FeatureFlagsConfiguration? configuration,
            DateTimeOffset? lastSuccessfulRefresh,
            Exception? lastError,
            long refreshCount = 0,
            long failedRefreshCount = 0,
            DateTimeOffset? lastRefreshAttempt = null,
            bool isPollingStopped = false)
        {
            this.Configuration = configuration;
            this.LastSuccessfulRefresh = lastSuccessfulRefresh;
            this.LastError = lastError;
            this.RefreshCount = refreshCount;
            this.FailedRefreshCount = failedRefreshCount;
            this.LastRefreshAttempt = lastRefreshAttempt;
            this.IsPollingStopped = isPollingStopped;
        }

        /// <summary>Whether a validated bootstrap or remote configuration is available.</summary>
        public bool IsReady => this.Configuration != null;

        /// <summary>Last valid snapshot. Failed refreshes never replace it.</summary>
        public FeatureFlagsConfiguration? Configuration { get; }

        /// <summary>Time of the last successful network refresh; null for bootstrap-only readiness.</summary>
        public DateTimeOffset? LastSuccessfulRefresh { get; }

        /// <summary>Most recent refresh failure, cleared by the next successful refresh.</summary>
        public Exception? LastError { get; }

        /// <summary>
        /// Refreshes that sent a request and finished, successfully or not, whether polled or manual.
        /// Refreshes cancelled by their caller or by disposal, and queued refreshes that time out
        /// before sending, are not counted.
        /// </summary>
        public long RefreshCount { get; }

        /// <summary>Counted refreshes that failed, including timeouts and invalid configurations.</summary>
        public long FailedRefreshCount { get; }

        /// <summary>Start time of the most recent counted refresh; null until one finishes.</summary>
        public DateTimeOffset? LastRefreshAttempt { get; }

        /// <summary>
        /// Whether background polling stopped permanently after an HTTP 401 or 403. The last good
        /// snapshot stays usable but will not be refreshed again; correct the credentials or access
        /// and create a new runtime. Disposal does not set this.
        /// </summary>
        public bool IsPollingStopped { get; }

        internal FeatureFlagsRuntimeStatus WithPollingStopped() => new FeatureFlagsRuntimeStatus(
            this.Configuration,
            this.LastSuccessfulRefresh,
            this.LastError,
            this.RefreshCount,
            this.FailedRefreshCount,
            this.LastRefreshAttempt,
            isPollingStopped: true);
    }
}
