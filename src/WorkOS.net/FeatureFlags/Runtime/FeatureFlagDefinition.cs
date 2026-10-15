// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// One flag's configuration as the runtime received it, for inspection and debugging. It does
    /// not evaluate; use the runtime client's <c>IsEnabled</c> for decisions.
    /// </summary>
    public sealed class FeatureFlagDefinition
    {
        internal FeatureFlagDefinition(
            string slug,
            bool enabled,
            bool? defaultValue,
            bool? offValue,
            IEnumerable<FeatureFlagRuleDefinition> rules)
        {
            this.Slug = slug;
            this.Enabled = enabled;
            this.DefaultValue = defaultValue;
            this.OffValue = offValue;
            this.Rules = Array.AsReadOnly(rules.ToArray());
        }

        /// <summary>The flag slug.</summary>
        public string Slug { get; }

        /// <summary>Whether the flag is enabled in this environment.</summary>
        public bool Enabled { get; }

        /// <summary>
        /// Served when the flag is enabled and no rule matches. Null when the served value is not a
        /// boolean, in which case the flag has no answer and callers receive their own default.
        /// </summary>
        public bool? DefaultValue { get; }

        /// <summary>Served when the flag is disabled. Null when the served value is not a boolean.</summary>
        public bool? OffValue { get; }

        /// <summary>Rules in evaluation order; the first matching rule wins.</summary>
        public IReadOnlyList<FeatureFlagRuleDefinition> Rules { get; }

        // Rule order is significant; see the rule and condition comparisons for the rest.
        internal bool HasSameConfiguration(FeatureFlagDefinition other) =>
            this.Enabled == other.Enabled &&
            this.DefaultValue == other.DefaultValue &&
            this.OffValue == other.OffValue &&
            this.Rules.Count == other.Rules.Count &&
            this.Rules.Zip(other.Rules).All(pair => pair.First.HasSameConfiguration(pair.Second));
    }

    /// <summary>One rule of a <see cref="FeatureFlagDefinition"/>, in the form the runtime received it.</summary>
    public sealed class FeatureFlagRuleDefinition
    {
        internal FeatureFlagRuleDefinition(
            string id,
            string kind,
            bool? value,
            IEnumerable<FeatureFlagConditionDefinition> conditions)
        {
            this.Id = id;
            this.Kind = kind;
            this.Value = value;
            this.Conditions = Array.AsReadOnly(conditions.ToArray());
        }

        /// <summary>The rule ID.</summary>
        public string Id { get; }

        /// <summary>
        /// The rule kind. This SDK evaluates only <c>conditions</c> rules; a rule of any other kind,
        /// such as one added by a newer server, never matches.
        /// </summary>
        public string Kind { get; }

        /// <summary>Served when the rule matches. Null when the served value is not a boolean.</summary>
        public bool? Value { get; }

        /// <summary>
        /// Conditions that must all match. For a kind or operator this SDK does not evaluate, these are
        /// read on a best-effort basis and may be incomplete.
        /// </summary>
        public IReadOnlyList<FeatureFlagConditionDefinition> Conditions { get; }

        internal bool HasSameConfiguration(FeatureFlagRuleDefinition other) =>
            this.Id == other.Id &&
            this.Kind == other.Kind &&
            this.Value == other.Value &&
            this.Conditions.Count == other.Conditions.Count &&
            this.Conditions.Zip(other.Conditions).All(pair => pair.First.HasSameConfiguration(pair.Second));
    }

    /// <summary>One condition of a <see cref="FeatureFlagRuleDefinition"/>.</summary>
    public sealed class FeatureFlagConditionDefinition
    {
        internal FeatureFlagConditionDefinition(string? targetType, string? @operator, IEnumerable<string> values)
        {
            this.TargetType = targetType;
            this.Operator = @operator;
            this.Values = Array.AsReadOnly(values.ToArray());
        }

        /// <summary>
        /// The target type the condition matches, such as <c>organization</c>, <c>user</c> or a custom
        /// type. Null only when a condition this SDK does not evaluate omits it.
        /// </summary>
        public string? TargetType { get; }

        /// <summary>
        /// The comparison operator. This SDK evaluates only <c>one_of</c>; a rule containing any other
        /// operator never matches. Null only when a condition this SDK does not evaluate omits it.
        /// </summary>
        public string? Operator { get; }

        /// <summary>Exact, case-sensitive IDs, in the order received.</summary>
        public IReadOnlyList<string> Values { get; }

        // Values are a membership set: the server's ID order carries no meaning.
        internal bool HasSameConfiguration(FeatureFlagConditionDefinition other) =>
            this.TargetType == other.TargetType &&
            this.Operator == other.Operator &&
            new HashSet<string>(this.Values, StringComparer.Ordinal).SetEquals(other.Values);
    }
}
