// @oagen-ignore-file
namespace WorkOS
{
    using System;

    /// <summary>Configuration for a locally evaluating feature flags runtime client.</summary>
    public sealed class FeatureFlagsRuntimeOptions
    {
        /// <summary>Interval between refreshes before jitter. Defaults to 30 seconds and is clamped to at least 5 seconds.</summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>Deadline for an entire refresh, including transport retries. Defaults to 10 seconds; must be at least 1 millisecond.</summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>Maximum exponential backoff before jitter. Defaults to 60 seconds. Retries never use a base delay shorter than the polling interval.</summary>
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(60);

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
