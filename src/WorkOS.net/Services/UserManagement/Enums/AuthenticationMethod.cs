namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the authentication method used to authenticate a user.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuthenticationMethod
    {
        [EnumMember(Value = "SSO")]
        SSO,

        [EnumMember(Value = "Password")]
        Password,

        [EnumMember(Value = "AppleOAuth")]
        AppleOAuth,

        [EnumMember(Value = "GitHubOAuth")]
        GitHubOAuth,

        [EnumMember(Value = "GoogleOAuth")]
        GoogleOAuth,

        [EnumMember(Value = "MicrosoftOAuth")]
        MicrosoftOAuth,

        [EnumMember(Value = "MagicAuth")]
        MagicAuth,

        [EnumMember(Value = "Impersonation")]
        Impersonation,
    }
}
