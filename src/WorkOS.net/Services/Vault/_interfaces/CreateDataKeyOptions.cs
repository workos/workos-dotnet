// @oagen-ignore-file
namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>Options for creating a data key.</summary>
    public class CreateDataKeyOptions : BaseOptions
    {
        [JsonProperty("context")]
        public Dictionary<string, string>? Context { get; set; }
    }
}
