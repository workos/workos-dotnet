// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>
    /// Helper utilities when issuing HTTP requests.
    /// </summary>
    public class RequestUtilities
    {
        /// <summary>
        /// Encodes options into a query string.
        /// </summary>
        /// <param name="options">Options to encode.</param>
        /// <returns>The encoded query string.</returns>
        public static string CreateQueryString(BaseOptions options)
        {
            var jsonOptions = ToJsonString(options);
            var dictionaryOptions = JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonOptions)!;
            var flattenedQuery = FlattenQueryParameters(dictionaryOptions);
            return string.Join(
                "&",
                flattenedQuery.Select(kvp => $"{UrlEncodeAndClean(kvp.Key)}={UrlEncodeAndClean(kvp.Value)}"));
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

            var jsonOptions = ToJsonString(options);

            if (request.IsJsonContentType)
            {
                var content = new StringContent(jsonOptions, Encoding.UTF8);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return content;
            }

            // Form-encode via the same object-based flattener the query-string
            // path uses. An earlier implementation deserialized directly into
            // IDictionary<string, string>, which threw on any bool/numeric field.
            var dictionaryOptions = JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonOptions)!;
            var flattened = FlattenQueryParameters(dictionaryOptions);
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
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                });
        }

        /// <summary>
        /// Deserializes a string into an object.
        /// </summary>
        /// <typeparam name="T">Type of object to serialize to.</typeparam>
        /// <param name="value">String to deserialize.</param>
        /// <returns>A deserialized object.</returns>
        public static T FromJson<T>(string value)
        {
            JsonSerializer jsonSerializer = JsonSerializer.Create();
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

        private static List<KeyValuePair<string, string>> FlattenQueryParameters(
            IDictionary<string, object> options)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            foreach (var kv in options)
            {
                var key = kv.Key;
                var value = kv.Value;
                switch (value)
                {
                    case null:
                        break;
                    case string s:
                        result.Add(new KeyValuePair<string, string>(key, s));
                        break;
                    case IEnumerable e:
                        foreach (object elem in e)
                        {
                            result.Add(new KeyValuePair<string, string>($"{key}[]", elem.ToString() ?? string.Empty));
                        }

                        break;
                    case DateTime dt:
                        result.Add(new KeyValuePair<string, string>(key, dt.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)));
                        break;
                    case DateTimeOffset dto:
                        result.Add(new KeyValuePair<string, string>(key, dto.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture)));
                        break;
                    case Enum en:
                        result.Add(new KeyValuePair<string, string>(key, en.ToString()));
                        break;
                    case bool b:
                        result.Add(new KeyValuePair<string, string>(key, b ? "true" : "false"));
                        break;
                    case long l:
                        result.Add(new KeyValuePair<string, string>(key, l.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case int i:
                        result.Add(new KeyValuePair<string, string>(key, i.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case short sh:
                        result.Add(new KeyValuePair<string, string>(key, sh.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case double d:
                        result.Add(new KeyValuePair<string, string>(key, d.ToString("R", CultureInfo.InvariantCulture)));
                        break;
                    case float f:
                        result.Add(new KeyValuePair<string, string>(key, f.ToString("R", CultureInfo.InvariantCulture)));
                        break;
                    case decimal m:
                        result.Add(new KeyValuePair<string, string>(key, m.ToString(CultureInfo.InvariantCulture)));
                        break;
                    case IConvertible c:
                        result.Add(new KeyValuePair<string, string>(key, c.ToString(CultureInfo.InvariantCulture)));
                        break;
                    default:
                        result.Add(new KeyValuePair<string, string>(key, value.ToString() ?? string.Empty));
                        break;
                }
            }

            return result;
        }
    }
}
