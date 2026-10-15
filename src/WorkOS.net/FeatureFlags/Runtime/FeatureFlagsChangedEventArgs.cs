// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;

    /// <summary>Describes a refresh that installed a different configuration.</summary>
    public sealed class FeatureFlagsChangedEventArgs : EventArgs
    {
        internal FeatureFlagsChangedEventArgs(
            FeatureFlagsConfiguration? previous,
            FeatureFlagsConfiguration current,
            IReadOnlySet<string> changedFlags)
        {
            this.Previous = previous;
            this.Current = current;
            this.ChangedFlags = changedFlags;
        }

        /// <summary>
        /// The snapshot this refresh replaced: null on the first load without a bootstrap, and the
        /// bootstrap configuration on the first refresh after one.
        /// </summary>
        public FeatureFlagsConfiguration? Previous { get; }

        /// <summary>The newly installed snapshot. Persist <c>Current.ToJson()</c> to bootstrap later.</summary>
        public FeatureFlagsConfiguration Current { get; }

        /// <summary>
        /// Slugs that were added, removed or whose configuration changed. On a first load without a
        /// bootstrap, every slug in <see cref="Current"/>.
        /// </summary>
        public IReadOnlySet<string> ChangedFlags { get; }
    }
}
