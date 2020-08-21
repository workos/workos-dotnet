namespace WorkOS
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Newtonsoft.Json;

    public class HttpUtils
    {
        public static string CreateQueryString(BaseOptions options)
        {
            var jsonOptions = JsonConvert.SerializeObject(
                options,
                Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
            var dictionaryOptions = JsonConvert.DeserializeObject<IDictionary<string, string>>(jsonOptions);
            return string.Join(
                "&",
                dictionaryOptions.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));
        }
    }
}
