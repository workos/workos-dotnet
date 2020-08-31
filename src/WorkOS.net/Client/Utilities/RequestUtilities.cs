namespace WorkOS
{
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
            var dictionaryOptions = JsonConvert.DeserializeObject<IDictionary<string, string>>(jsonOptions);
            return string.Join(
                "&",
                dictionaryOptions.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
        }

        /// <summary>
        /// Creates an encoded HTTP entity body and content headers.
        /// </summary>
        /// <param name="options">Parameters to encode.</param>
        /// <returns>Encoded HTTP content for a request.</returns>
        public static StringContent CreateHttpContent(BaseOptions options)
        {
            var jsonOptions = ToJsonString(options);
            var content = new StringContent(jsonOptions, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
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
                    NullValueHandling = NullValueHandling.Ignore
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
    }
}
