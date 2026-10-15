// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;

    /// <summary>
    /// Exact IDs to evaluate locally. Organization memberships are not fetched automatically;
    /// supply the organization for the current evaluation explicitly.
    /// </summary>
    public sealed class FeatureFlagContext
    {
        /// <summary>Initializes a new instance of the <see cref="FeatureFlagContext"/> class.</summary>
        /// <param name="userId">Optional user ID.</param>
        /// <param name="organizationId">Optional organization ID.</param>
        /// <param name="customTargets">Optional custom target type to ID mappings.</param>
        public FeatureFlagContext(
            string? userId = null,
            string? organizationId = null,
            IReadOnlyDictionary<string, string>? customTargets = null)
        {
            var targets = new Dictionary<string, string>(StringComparer.Ordinal);
            if (customTargets != null)
            {
                foreach (var target in customTargets)
                {
                    ArgumentException.ThrowIfNullOrWhiteSpace(target.Key);
                    ArgumentException.ThrowIfNullOrWhiteSpace(target.Value);
                    if (target.Key == "user" || target.Key == "organization")
                    {
                        throw new ArgumentException("Use userId or organizationId for built-in target types.", nameof(customTargets));
                    }

                    targets.Add(target.Key, target.Value);
                }
            }

            if (userId != null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(userId);
                targets.Add("user", userId);
            }

            if (organizationId != null)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(organizationId);
                targets.Add("organization", organizationId);
            }

            this.Targets = targets.ToFrozenDictionary(StringComparer.Ordinal);
        }

        internal FrozenDictionary<string, string> Targets { get; }
    }
}
