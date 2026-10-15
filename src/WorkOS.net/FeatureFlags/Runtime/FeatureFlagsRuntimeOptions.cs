// @oagen-ignore-file
namespace WorkOS
{
    using System;

    /// <summary>Configuration for a locally evaluating feature flags runtime client.</summary>
    public sealed class FeatureFlagsRuntimeOptions
    {
        /// <summary>Interval between refreshes before jitter. Defaults to 30 seconds and is clamped to at least 5 seconds.</summary>
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>Deadline for an entire refresh, including queued time and transport retries. Defaults to 10 seconds; must be at least 1 millisecond.</summary>
        public TimeSpan RequestTimeout { get; set; } = TimeSpan.FromSeconds(10);

        /// <summary>Maximum exponential backoff before jitter. Defaults to 60 seconds. Retries never use a base delay shorter than the polling interval.</summary>
        public TimeSpan MaxRetryDelay { get; set; } = TimeSpan.FromSeconds(60);

        /// <summary>Optional validated cached configuration, immediately usable before polling starts.</summary>
        public FeatureFlagsConfiguration? BootstrapConfiguration { get; set; }

        /// <summary>
        /// Optional refresh status observer. The runtime calls Report one at a time in refresh order,
        /// independently of polling, and reports may arrive after disposal. Observer failures do not
        /// affect polling. Implement IProgress directly to handle reports in that order; a Progress
        /// instance posts callbacks to its captured synchronization context, or to the thread pool
        /// without one, where they can overlap or complete out of order.
        /// </summary>
        public IProgress<FeatureFlagsRuntimeStatus>? Progress { get; set; }

        /// <summary>
        /// Clock for polling delays, refresh deadlines, and refresh timestamps. Defaults to
        /// <see cref="System.TimeProvider.System"/>; supply a fake provider to control time in tests.
        /// </summary>
        public TimeProvider? TimeProvider { get; set; }
    }
}
