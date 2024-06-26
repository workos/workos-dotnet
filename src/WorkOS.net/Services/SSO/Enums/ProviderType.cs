namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Constants that enumerate available OAuth provider types to initiate OAuth authentication.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProviderType
    {
        [EnumMember(Value = "AppleOAuth")]
        AppleOAuth,

        [EnumMember(Value = "GitHubOAuth")]
        GitHubOAuth,

        [EnumMember(Value = "GoogleOAuth")]
        GoogleOAuth,

        [EnumMember(Value = "MicrosoftOAuth")]
        MicrosoftOAuth,
    }
}
