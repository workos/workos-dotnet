// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// Helper utilities when issuing HTTP requests.
    /// </summary>
    public class RequestUtilities
    {
        /// <summary>
        /// Shared Newtonsoft settings with snake_case naming and OneOf converter.
        /// Convention-based: generated types no longer carry per-property
        /// <c>[JsonProperty("wire_name")]</c> attributes.
        /// </summary>
        internal static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new InternalPropertyContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy(),
            },
            Converters = { new OneOfJsonConverter() },
        };

        private static readonly SnakeCaseNamingStrategy WireNamingStrategy = new SnakeCaseNamingStrategy();

        /// <summary>
        /// Encodes options into a query string.
        /// </summary>
        /// <param name="options">Options to encode.</param>
        /// <returns>The encoded query string.</returns>
        public static string CreateQueryString(BaseOptions options)
        {
            var flattened = FlattenOptions(options);
            return string.Join(
                "&",
                flattened.Select(kvp => $"{UrlEncodeAndClean(kvp.Key)}={UrlEncodeAndClean(kvp.Value)}"));
        }

        /// <summary>
        /// Creates an encoded HTTP entity body and content headers.
        /// </summary>
        /// <param name="request">Request to encode.</param>
        /// <returns>Encoded HTTP content for a request.</returns>
        public static HttpContent CreateHttpContent(WorkOSRequest request)
        {
            var options = request.Options;
            if (options == null)
            {
                options = new BaseOptions { };
            }

            if (request.IsJsonContentType)
            {
                var jsonOptions = ToJsonString(options);
                var content = new StringContent(jsonOptions, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return content;
            }

            // Form-encode via the same object-graph walker the query-string
            // path uses. Prior implementation round-tripped through JSON,
            // which erased int/short/decimal/date type info and mangled
            // nested objects into ToString() soup.
            var flattened = FlattenOptions(options);
            return new FormUrlEncodedContent(flattened);
        }

        /// <summary>
        /// Serializes an object.
        /// </summary>
        /// <param name="value">Object to serialize.</param>
        /// <returns>A serialized JSON object.</returns>
        public static string ToJsonString(object value)
        {
            return JsonConvert.SerializeObject(
                value,
                Formatting.None,
                JsonSettings);
        }

        /// <summary>
        /// Deserializes a string into an object.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize to.</typeparam>
        /// <param name="value">String to deserialize.</param>
        /// <returns>A deserialized object.</returns>
        public static T FromJson<T>(string value)
        {
            JsonSerializer jsonSerializer = JsonSerializer.Create(JsonSettings);
            JsonTextReader reader = new JsonTextReader(new StringReader(value));
            return jsonSerializer.Deserialize<T>(reader)!;
        }

        private static string UrlEncodeAndClean(string value)
        {
            return WebUtility.UrlEncode(value)
                .Replace("%40", "@")
                .Replace("%3A", ":")
                .Replace("%5B", "[")
                .Replace("%5D", "]");
        }

        /// <summary>
        /// Walk the options object graph directly (no JSON round-trip) and
        /// emit wire-name=value pairs. Respects Newtonsoft's
        /// [JsonProperty] for name/ignore/default-value handling and
        /// [JsonIgnore] for full exclusion. Nested objects flatten with
        /// bracket notation (`parent[child]=v`); lists/arrays repeat the
        /// key with `[]` per element; dictionaries keyed by string flatten
        /// as `parent[key]=v`.
        /// </summary>
        private static List<KeyValuePair<string, string>> FlattenOptions(object options)
        {
            var result = new List<KeyValuePair<string, string>>();
            FlattenObject(options, string.Empty, result);
            return result;
        }

        private static void FlattenObject(object? value, string prefix, List<KeyValuePair<string, string>> result)
        {
            if (value == null)
            {
                return;
            }

            var type = value.GetType();
            if (IsScalar(type))
            {
                AddScalar(prefix, value, result);
                return;
            }

            // IDictionary<string, _>: flatten as prefix[key]=value
            if (value is IDictionary dict)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    var key = entry.Key?.ToString() ?? string.Empty;
                    var childKey = string.IsNullOrEmpty(prefix) ? key : $"{prefix}[{key}]";
                    FlattenObject(entry.Value, childKey, result);
                }

                return;
            }

            // Any other IEnumerable (but not string, already handled in scalars)
            if (value is IEnumerable enumerable)
            {
                foreach (var elem in enumerable)
                {
                    var childKey = string.IsNullOrEmpty(prefix) ? "[]" : $"{prefix}[]";
                    if (elem != null && !IsScalar(elem.GetType()))
                    {
                        // Nested object inside a list — rare, but flatten anyway.
                        FlattenObject(elem, childKey, result);
                    }
                    else
                    {
                        AddScalar(childKey, elem, result);
                    }
                }

                return;
            }

            // Plain object: walk public properties.
            foreach (var prop in GetSerializableProperties(type))
            {
                if (!prop.CanRead)
                {
                    continue;
                }

                var wireName = ResolveWireName(prop);
                if (wireName == null)
                {
                    continue; // JsonIgnore
                }

                object? propValue;
                try
                {
                    propValue = prop.GetValue(value);
                }
                catch (TargetInvocationException)
                {
                    continue;
                }

                if (ShouldSkip(prop, propValue))
                {
                    continue;
                }

                var childKey = string.IsNullOrEmpty(prefix) ? wireName : $"{prefix}[{wireName}]";
                FlattenObject(propValue, childKey, result);
            }
        }

        private static IEnumerable<PropertyInfo> GetSerializableProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Return the wire name for a property, or null if the property is
        /// ignored entirely. Respects Newtonsoft [JsonProperty]/[JsonIgnore]
        /// and falls back to snake_case conversion of the property name
        /// (matching the global naming convention).
        /// </summary>
        private static string? ResolveWireName(PropertyInfo prop)
        {
            if (prop.GetCustomAttribute<JsonIgnoreAttribute>() != null)
            {
                return null;
            }

            var jp = prop.GetCustomAttribute<JsonPropertyAttribute>();
            if (jp != null && !string.IsNullOrEmpty(jp.PropertyName))
            {
                return jp.PropertyName;
            }

            return WireNamingStrategy.GetPropertyName(prop.Name, false);
        }

        /// <summary>
        /// Skip values that would be omitted by NullValueHandling.Ignore
        /// (null) or DefaultValueHandling.Ignore (== type default).
        /// </summary>
        private static bool ShouldSkip(PropertyInfo prop, object? value)
        {
            if (value == null)
            {
                return true; // NullValueHandling.Ignore matches the JSON path
            }

            var jp = prop.GetCustomAttribute<JsonPropertyAttribute>();
            if (jp != null && jp.DefaultValueHandling == DefaultValueHandling.Ignore)
            {
                var defaultValue = GetTypeDefault(prop.PropertyType);
                if (object.Equals(value, defaultValue))
                {
                    return true;
                }
            }

            return false;
        }

        private static object? GetTypeDefault(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        private static bool IsScalar(Type type)
        {
            if (type.IsPrimitive)
            {
                return true;
            }

            if (type == typeof(string))
            {
                return true;
            }

            if (type == typeof(decimal))
            {
                return true;
            }

            if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            {
                return true;
            }

            if (type == typeof(TimeSpan))
            {
                return true;
            }

            if (type == typeof(Guid))
            {
                return true;
            }

            if (type.IsEnum)
            {
                return true;
            }

            var underlying = Nullable.GetUnderlyingType(type);
            if (underlying != null)
            {
                return IsScalar(underlying);
            }

            return false;
        }

        private static void AddScalar(string key, object? value, List<KeyValuePair<string, string>> result)
        {
            if (value == null)
            {
                return;
            }

            string rendered;
            switch (value)
            {
                case string s:
                    rendered = s;
                    break;
                case bool b:
                    rendered = b ? "true" : "false";
                    break;
                case DateTime dt:
                    rendered = dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                    break;
                case DateTimeOffset dto:
                    rendered = dto.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
                    break;
                case Enum en:
                    rendered = en.ToString();
                    break;
                case double d:
                    rendered = d.ToString("R", CultureInfo.InvariantCulture);
                    break;
                case float f:
                    rendered = f.ToString("R", CultureInfo.InvariantCulture);
                    break;
                case IFormattable fmt:
                    rendered = fmt.ToString(null, CultureInfo.InvariantCulture);
                    break;
                default:
                    rendered = value.ToString() ?? string.Empty;
                    break;
            }

            result.Add(new KeyValuePair<string, string>(key, rendered));
        }

        /// <summary>
        /// Contract resolver that includes internal properties so that
        /// service-injected fields (grant_type, client_id, client_secret)
        /// on wrapper options classes are serialized into the JSON body.
        /// </summary>
        private class InternalPropertyContractResolver : DefaultContractResolver
        {
            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                var members = base.GetSerializableMembers(objectType);

                // Add internal (assembly-level) properties that base skips.
                foreach (var prop in objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (prop.GetMethod?.IsAssembly == true && !members.Any(m => m.Name == prop.Name))
                    {
                        members.Add(prop);
                    }
                }

                return members;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var prop = base.CreateProperty(member, memberSerialization);

                // Ensure internal properties are readable/writable for serialization.
                if (member is PropertyInfo pi && pi.GetMethod?.IsAssembly == true)
                {
                    prop.Readable = pi.CanRead;
                    prop.Writable = pi.CanWrite;
                }

                return prop;
            }
        }
    }
}
