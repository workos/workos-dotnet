// @oagen-ignore-file
namespace WorkOS
{
    using System;

    /// <summary>Configuration for a locally evaluating feature flags runtime client.</summary>
    public sealed class FeatureFlagsRuntimeOptions
    {
        /// <summary>Interval between successful refreshes. Defaults to 60 seconds.</summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>Deadline for an entire refresh, including transport retries. Defaults to 10 seconds.</summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>Maximum delay after failed polls. Defaults to 5 minutes and must be at least the polling interval.</summary>
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromMinutes(5);

        /// <summary>Optional validated cached configuration, immediately usable before polling starts.</summary>
        public FeatureFlagsConfiguration? BootstrapConfiguration { get; set; }

        /// <summary>
        /// Optional refresh status observer, dispatched independently of polling. Reports may arrive
        /// after disposal or concurrently. Observer failures do not affect polling. A Progress instance
        /// can marshal callbacks to your synchronization context.
        /// </summary>
        public IProgress<FeatureFlagsRuntimeStatus>? Progress { get; set; }
    }
}
