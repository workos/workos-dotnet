 namespace WorkOS
{
    using Newtonsoft.Json;
    public class IVerifyFactorOptions : BaseOptions
    {
        /// <summary>
        /// Describes which type to use.
        /// </summary>
        [JsonProperty("authentication_challenge_id")]
        string id {get; set;}

        /// <summary>
        /// Describes which type to use.
        /// </summary>
        [JsonProperty("code")]
        string code {get; set;}
    }
}