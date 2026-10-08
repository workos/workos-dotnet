// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Frozen;
    using System.Collections.Generic;
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
        /// <see cref="JsonException"/>. Unknown rule kinds and operators are skipped safely.
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
                flags.Add(entry.Name, new Flag(
                    RequireBoolean(flag["enabled"], "enabled"),
                    RequireBoolean(flag["default_value"], "default_value"),
                    RequireBoolean(flag["off_value"], "off_value"),
                    rules));
            }

            return new FeatureFlagsConfiguration(payload.ToString(Formatting.None), flags.ToFrozenDictionary(StringComparer.Ordinal));
        }

        /// <summary>Returns the complete versioned configuration, including unknown fields.</summary>
        /// <returns>JSON suitable for caller-owned caching and bootstrap.</returns>
        public string ToJson() => this.json;

        private static Rule ParseRule(JToken token)
        {
            var rule = RequireObject(token, "rule");
            RequireString(rule["id"], "rule.id");
            if (RequireString(rule["kind"], "rule.kind") != "conditions")
            {
                return Rule.NoMatch;
            }

            var value = RequireBoolean(rule["value"], "rule.value");
            var conditions = new List<Condition>();
            foreach (var tokenCondition in RequireArray(rule["conditions"], "rule.conditions"))
            {
                var condition = RequireObject(tokenCondition, "condition");
                if (RequireString(condition["operator"], "condition.operator") != "one_of")
                {
                    // Future operators may have entirely different operands. Do not interpret them.
                    return Rule.NoMatch;
                }

                conditions.Add(new Condition(
                    RequireString(condition["target_type"], "condition.target_type"),
                    RequireArray(condition["values"], "condition.values")
                        .Select(id => RequireString(id, "condition.values[]"))
                        .ToFrozenSet(StringComparer.Ordinal)));
            }

            return new Rule(value, conditions.ToArray());
        }

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

        private static JsonSerializationException InvalidPayload(string message) => new JsonSerializationException(message);

        internal sealed class Flag
        {
            private readonly bool enabled;
            private readonly bool defaultValue;
            private readonly bool offValue;
            private readonly Rule[] rules;

            internal Flag(bool enabled, bool defaultValue, bool offValue, Rule[] rules)
            {
                this.enabled = enabled;
                this.defaultValue = defaultValue;
                this.offValue = offValue;
                this.rules = rules;
            }

            internal bool Evaluate(FeatureFlagContext? context)
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

            internal Rule(bool value, Condition[] conditions)
            {
                this.Value = value;
                this.conditions = conditions;
            }

            internal bool Value { get; }

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
