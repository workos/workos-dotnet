// @oagen-ignore-file
namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>Options for decrypting a data key.</summary>
    public class DecryptDataKeyOptions : BaseOptions
    {
        [JsonProperty("keys")]
        public string Keys { get; set; } = default!;
    }
}
