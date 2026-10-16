// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// An immutable, validated V2 configuration snapshot. Cache its JSON in caller-owned
    /// storage to bootstrap another runtime client without a successful network request.
    /// </summary>
    public sealed class FeatureFlagsConfiguration
    {
        private readonly string json;

        private FeatureFlagsConfiguration(string json, FrozenDictionary<string, Flag> flags)
        {
            this.json = json;
            this.Flags = flags;
        }

        /// <summary>The number of flags in this snapshot.</summary>
        public int Count => this.Flags.Count;

        internal FrozenDictionary<string, Flag> Flags { get; }

        /// <summary>
        /// Parses a V2 SDK configuration. Malformed or unsupported payload versions throw
        /// <see cref="JsonException"/>. Unknown rule kinds and operators are skipped safely, and a
        /// flag whose served value is not a boolean has no answer, so callers receive their own default.
        /// </summary>
        /// <param name="json">The complete SDK poll response.</param>
        /// <returns>An immutable configuration snapshot.</returns>
        public static FeatureFlagsConfiguration FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);
            using var reader = new JsonTextReader(new StringReader(json))
            {
                DateParseHandling = DateParseHandling.None,
            };
            var payload = JObject.Load(reader, new JsonLoadSettings
            {
                DuplicatePropertyNameHandling = DuplicatePropertyNameHandling.Error,
            });
            if (reader.Read())
            {
                throw InvalidPayload("Unexpected content after the configuration.");
            }

            if (payload["version"]?.Type != JTokenType.Integer || payload["version"]?.ToString() != "2")
            {
                throw InvalidPayload("Expected SDK payload version 2.");
            }

            var flagMap = RequireObject(payload["flags"], "flags");
            var flags = new Dictionary<string, Flag>(StringComparer.Ordinal);
            foreach (var entry in flagMap.Properties())
            {
                var flag = RequireObject(entry.Value, "flag");
                if (RequireString(flag["slug"], "slug") != entry.Name)
                {
                    throw InvalidPayload("Flag slug does not match its map key.");
                }

                var rules = RequireArray(flag["rules"], "rules").Select(ParseRule).ToArray();
                var enabled = RequireBoolean(flag["enabled"], "enabled");
                var defaultValue = ServedValue(flag["default_value"]);
                var offValue = ServedValue(flag["off_value"]);
                flags.Add(entry.Name, new Flag(
                    enabled,
                    defaultValue,
                    offValue,
                    rules.Select(rule => rule.Rule).ToArray(),
                    new FeatureFlagDefinition(entry.Name, enabled, defaultValue, offValue, rules.Select(rule => rule.Definition)),
                    flag));
            }

            return new FeatureFlagsConfiguration(payload.ToString(Formatting.None), flags.ToFrozenDictionary(StringComparer.Ordinal));
        }

        /// <summary>Returns the complete versioned configuration, including unknown fields.</summary>
        /// <returns>JSON suitable for caller-owned caching and bootstrap.</returns>
        public string ToJson() => this.json;

        /// <summary>Looks up one flag's configuration in this snapshot, for inspection.</summary>
        /// <param name="slug">Flag slug.</param>
        /// <param name="flag">The flag's configuration, when present.</param>
        /// <returns>Whether the snapshot contains the flag.</returns>
        public bool TryGetFlag(string slug, [NotNullWhen(true)] out FeatureFlagDefinition? flag)
        {
            ArgumentNullException.ThrowIfNull(slug);
            flag = this.Flags.TryGetValue(slug, out var value) ? value.Definition : null;
            return flag != null;
        }

        // Compares each flag's complete content per slug, because the server does not guarantee flag
        // order. Every slug changes on the first load.
        internal static FrozenSet<string> ChangedFlags(FeatureFlagsConfiguration? previous, FeatureFlagsConfiguration current)
        {
            if (previous == null)
            {
                return current.Flags.Keys.ToFrozenSet(StringComparer.Ordinal);
            }

            var changed = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in current.Flags)
            {
                if (!previous.Flags.TryGetValue(entry.Key, out var before) || !SameFlagContent(before.Content, entry.Value.Content))
                {
                    changed.Add(entry.Key);
                }
            }

            foreach (var slug in previous.Flags.Keys)
            {
                if (!current.Flags.ContainsKey(slug))
                {
                    changed.Add(slug);
                }
            }

            return changed.ToFrozenSet(StringComparer.Ordinal);
        }

        // Compares like JToken.DeepEquals, so object property order never matters, except that the IDs
        // of a one_of condition in a conditions rule form a set. Every other array, including those under
        // rule kinds and operators this SDK does not evaluate, stays order-sensitive: a spurious change
        // only costs a redundant save, but a missed one leaves a cache stale.
        private static bool SameFlagContent(JObject previous, JObject current) =>
            SameProperties(previous, current, (name, before, after) =>
                name == "rules" && before is JArray beforeRules && after is JArray afterRules
                    ? SameElements(beforeRules, afterRules, SameRuleContent)
                    : JToken.DeepEquals(before, after));

        private static bool SameRuleContent(JToken previous, JToken current)
        {
            if (previous is not JObject before || current is not JObject after ||
                OptionalString(before["kind"]) != "conditions" || OptionalString(after["kind"]) != "conditions")
            {
                return JToken.DeepEquals(previous, current);
            }

            return SameProperties(before, after, (name, beforeValue, afterValue) =>
                name == "conditions" && beforeValue is JArray beforeConditions && afterValue is JArray afterConditions
                    ? SameElements(beforeConditions, afterConditions, SameConditionContent)
                    : JToken.DeepEquals(beforeValue, afterValue));
        }

        private static bool SameConditionContent(JToken previous, JToken current)
        {
            if (previous is not JObject before || current is not JObject after ||
                OptionalString(before["operator"]) != "one_of" || OptionalString(after["operator"]) != "one_of")
            {
                return JToken.DeepEquals(previous, current);
            }

            return SameProperties(before, after, (name, beforeValue, afterValue) =>
                name == "values" ? SameTargetIds(beforeValue, afterValue) : JToken.DeepEquals(beforeValue, afterValue));
        }

        private static bool SameTargetIds(JToken previous, JToken current)
        {
            if (previous is not JArray before || current is not JArray after ||
                before.Any(id => id.Type != JTokenType.String) || after.Any(id => id.Type != JTokenType.String))
            {
                return JToken.DeepEquals(previous, current);
            }

            return new HashSet<string>(before.Select(id => id.Value<string>()!), StringComparer.Ordinal)
                .SetEquals(after.Select(id => id.Value<string>()!));
        }

        private static bool SameProperties(JObject previous, JObject current, Func<string, JToken, JToken, bool> same)
        {
            if (previous.Count != current.Count)
            {
                return false;
            }

            foreach (var property in previous.Properties())
            {
                if (!current.TryGetValue(property.Name, out var value) || !same(property.Name, property.Value, value))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool SameElements(JArray previous, JArray current, Func<JToken, JToken, bool> same) =>
            previous.Count == current.Count && previous.Zip(current).All(pair => same(pair.First, pair.Second));

        private static (Rule Rule, FeatureFlagRuleDefinition Definition) ParseRule(JToken token)
        {
            var rule = RequireObject(token, "rule");
            var id = RequireString(rule["id"], "rule.id");
            var kind = RequireString(rule["kind"], "rule.kind");
            var value = ServedValue(rule["value"]);
            if (kind != "conditions")
            {
                // A future rule kind may have entirely different fields. Record what is recognizable
                // for inspection, but never evaluate it.
                return (Rule.NoMatch, new FeatureFlagRuleDefinition(id, kind, value, InspectConditions(rule["conditions"] as JArray, 0)));
            }

            var tokens = RequireArray(rule["conditions"], "rule.conditions");
            var conditions = new List<Condition>();
            var definitions = new List<FeatureFlagConditionDefinition>();
            for (var index = 0; index < tokens.Count; index++)
            {
                var condition = RequireObject(tokens[index], "condition");
                var conditionOperator = RequireString(condition["operator"], "condition.operator");
                if (conditionOperator != "one_of")
                {
                    // Future operators may have entirely different operands. Do not interpret them,
                    // or anything after them; only record them for inspection.
                    definitions.AddRange(InspectConditions(tokens, index));
                    return (Rule.NoMatch, new FeatureFlagRuleDefinition(id, kind, value, definitions));
                }

                var targetType = RequireString(condition["target_type"], "condition.target_type");
                var values = RequireArray(condition["values"], "condition.values")
                    .Select(target => RequireString(target, "condition.values[]"))
                    .ToArray();
                conditions.Add(new Condition(targetType, values.ToFrozenSet(StringComparer.Ordinal)));
                definitions.Add(new FeatureFlagConditionDefinition(targetType, conditionOperator, values));
            }

            return (new Rule(value, conditions.ToArray()), new FeatureFlagRuleDefinition(id, kind, value, definitions));
        }

        // Reads conditions this SDK does not evaluate without validating them, so an unfamiliar shape
        // can never fail a refresh.
        private static IEnumerable<FeatureFlagConditionDefinition> InspectConditions(JArray? tokens, int start)
        {
            if (tokens == null)
            {
                yield break;
            }

            for (var index = start; index < tokens.Count; index++)
            {
                if (tokens[index] is JObject condition)
                {
                    var values = condition["values"] is JArray array
                        ? array.Where(target => target.Type == JTokenType.String).Select(target => target.Value<string>()!)
                        : Array.Empty<string>();
                    yield return new FeatureFlagConditionDefinition(
                        OptionalString(condition["target_type"]),
                        OptionalString(condition["operator"]),
                        values);
                }
            }
        }

        private static string? OptionalString(JToken? token) =>
            token?.Type == JTokenType.String ? token.Value<string>() : null;

        private static JObject RequireObject(JToken? token, string field) =>
            token is JObject value ? value : throw InvalidPayload($"Expected an object for {field}.");

        private static JArray RequireArray(JToken? token, string field) =>
            token is JArray value ? value : throw InvalidPayload($"Expected an array for {field}.");

        private static string RequireString(JToken? token, string field)
        {
            if (token?.Type != JTokenType.String || string.IsNullOrWhiteSpace(token.Value<string>()))
            {
                throw InvalidPayload($"Expected a nonempty string for {field}.");
            }

            return token.Value<string>()!;
        }

        private static bool RequireBoolean(JToken? token, string field) =>
            token?.Type == JTokenType.Boolean ? token.Value<bool>() : throw InvalidPayload($"Expected a boolean for {field}.");

        // Mirrors the server's definition: a served value that is not a boolean, such as a future
        // multivariate variant, makes the flag have no answer rather than failing the refresh or
        // silently turning the flag off. The caller's own default applies instead.
        private static bool? ServedValue(JToken? token) =>
            token?.Type == JTokenType.Boolean ? token.Value<bool>() : null;

        private static JsonSerializationException InvalidPayload(string message) => new JsonSerializationException(message);

        internal sealed class Flag
        {
            private readonly bool enabled;
            private readonly bool? defaultValue;
            private readonly bool? offValue;
            private readonly Rule[] rules;

            internal Flag(bool enabled, bool? defaultValue, bool? offValue, Rule[] rules, FeatureFlagDefinition definition, JObject content)
            {
                this.enabled = enabled;
                this.defaultValue = defaultValue;
                this.offValue = offValue;
                this.rules = rules;
                this.Definition = definition;
                this.Content = content;
            }

            internal FeatureFlagDefinition Definition { get; }

            // The flag exactly as received, including fields this SDK does not interpret. Never mutated.
            internal JObject Content { get; }

            // Null means the flag has no boolean answer for this context and the caller's default applies.
            internal bool? Evaluate(FeatureFlagContext? context)
            {
                if (!this.enabled)
                {
                    return this.offValue;
                }

                foreach (var rule in this.rules)
                {
                    if (rule.Matches(context))
                    {
                        return rule.Value;
                    }
                }

                return this.defaultValue;
            }
        }

        internal sealed class Rule
        {
            internal static readonly Rule NoMatch = new Rule(false, Array.Empty<Condition>());

            private readonly Condition[] conditions;

            internal Rule(bool? value, Condition[] conditions)
            {
                this.Value = value;
                this.conditions = conditions;
            }

            internal bool? Value { get; }

            internal bool Matches(FeatureFlagContext? context) =>
                this.conditions.Length > 0 && this.conditions.All(condition => condition.Matches(context));
        }

        internal sealed class Condition
        {
            private readonly string targetType;
            private readonly FrozenSet<string> values;

            internal Condition(string targetType, FrozenSet<string> values)
            {
                this.targetType = targetType;
                this.values = values;
            }

            internal bool Matches(FeatureFlagContext? context) =>
                context != null && context.Targets.TryGetValue(this.targetType, out var id) && this.values.Contains(id);
        }
    }
}
