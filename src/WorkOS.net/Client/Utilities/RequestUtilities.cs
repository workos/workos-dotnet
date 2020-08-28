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

    public class RequestUtilities
    {
        public static string CreateQueryString(BaseOptions options)
        {
            var jsonOptions = ToJsonString(options);
            var dictionaryOptions = JsonConvert.DeserializeObject<IDictionary<string, string>>(jsonOptions);
            return string.Join(
                "&",
                dictionaryOptions.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
        }

        public static StringContent CreateHttpContent(BaseOptions options)
        {
            var jsonOptions = ToJsonString(options);
            var content = new StringContent(jsonOptions, Encoding.UTF8);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }

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

        public static T FromJson<T>(string value)
        {
            JsonSerializer jsonSerializer = JsonSerializer.Create();
            JsonTextReader reader = new JsonTextReader(new StringReader(value));
            return jsonSerializer.Deserialize<T>(reader);
        }

        public static Dictionary<string, string> ParseURLParameters(string url)
        {
            int startIndex = url.IndexOf('?') + 1;
            return url.Substring(startIndex).Split('&')
                .Select(x => x.Split(new[] { '=' }, 2))
                .ToDictionary(x => WebUtility.UrlDecode(x[0]), x => WebUtility.UrlDecode(x[1]));
        }
    }
}
