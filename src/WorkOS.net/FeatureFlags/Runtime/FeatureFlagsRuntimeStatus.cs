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
            DateTimeOffset? lastRefreshAttempt = null)
        {
            this.Configuration = configuration;
            this.LastSuccessfulRefresh = lastSuccessfulRefresh;
            this.LastError = lastError;
            this.RefreshCount = refreshCount;
            this.FailedRefreshCount = failedRefreshCount;
            this.LastRefreshAttempt = lastRefreshAttempt;
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
    }
}
