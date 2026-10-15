// @oagen-ignore-file
namespace WorkOS
{
    using System;

    /// <summary>An immutable view of readiness and the most recent refresh result.</summary>
    public sealed class FeatureFlagsRuntimeStatus
    {
        internal FeatureFlagsRuntimeStatus(FeatureFlagsConfiguration? configuration, DateTimeOffset? lastSuccessfulRefresh, Exception? lastError)
        {
            this.Configuration = configuration;
            this.LastSuccessfulRefresh = lastSuccessfulRefresh;
            this.LastError = lastError;
        }

        /// <summary>Whether a validated bootstrap or remote configuration is available.</summary>
        public bool IsReady => this.Configuration != null;

        /// <summary>Last valid snapshot. Failed refreshes never replace it.</summary>
        public FeatureFlagsConfiguration? Configuration { get; }

        /// <summary>Time of the last successful network refresh; null for bootstrap-only readiness.</summary>
        public DateTimeOffset? LastSuccessfulRefresh { get; }

        /// <summary>Most recent refresh failure, cleared by the next successful refresh.</summary>
        public Exception? LastError { get; }
    }
}
