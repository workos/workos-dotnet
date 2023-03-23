namespace WorkOS
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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
            var dictionaryOptions = JsonConvert.DeserializeObject<IDictionary<string, object>>(jsonOptions);
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

            var dictionaryOptions = JsonConvert.DeserializeObject<IDictionary<string, string>>(jsonOptions);
            return new FormUrlEncodedContent(dictionaryOptions.ToList());
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
            return jsonSerializer.Deserialize<T>(reader);
        }

        /// <summary>
        /// Parses query parameters from a URL into a dictionary.
        /// </summary>
        /// <param name="url">URL to parse.</param>
        /// <returns>A dictionary of parameters.</returns>
        public static Dictionary<string, string> ParseURLParameters(string url)
        {
            int startIndex = url.IndexOf('?') + 1;
            return url.Substring(startIndex).Split('&')
                .Select(x => x.Split(new[] { '=' }, 2))
                .ToDictionary(x => WebUtility.UrlDecode(x[0]), x => WebUtility.UrlDecode(x[1]));
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
                            result.Add(new KeyValuePair<string, string>($"{key}[]", elem.ToString()));
                        }

                        break;

                    case DateTime dt:
                        var isoDt = dt.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        result.Add(new KeyValuePair<string, string>(key, isoDt));
                        break;

                    case Enum e:
                        result.Add(new KeyValuePair<string, string>(key, e.ToString()));
                        break;

                    case long l:
                        result.Add(new KeyValuePair<string, string>(key, l.ToString()));
                        break;

                    case int i:
                        result.Add(new KeyValuePair<string, string>(key, i.ToString()));
                        break;

                    default:
                        break;
                }
            }

            return result;
        }
    }
}
