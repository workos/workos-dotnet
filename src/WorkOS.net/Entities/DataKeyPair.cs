// @oagen-ignore-file
namespace WorkOS
{
    using System.Collections.Generic;
    using Newtonsoft.Json;

    /// <summary>A data key pair from the Vault key service.</summary>
    public class DataKeyPair
    {
        [JsonProperty("context")]
        public Dictionary<string, string>? Context { get; set; }

        [JsonProperty("data_key")]
        public DataKey? DataKey { get; set; }

        [JsonProperty("encrypted_keys")]
        public string EncryptedKeys { get; set; } = default!;
    }
}
