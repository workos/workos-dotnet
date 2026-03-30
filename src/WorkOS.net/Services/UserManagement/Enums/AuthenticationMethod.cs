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

        [EnumMember(Value = "Passkey")]
        Passkey,

        [EnumMember(Value = "AppleOAuth")]
        AppleOAuth,

        [EnumMember(Value = "BitbucketOAuth")]
        BitbucketOAuth,

        [EnumMember(Value = "DiscordOAuth")]
        DiscordOAuth,

        [EnumMember(Value = "GitHubOAuth")]
        GitHubOAuth,

        [EnumMember(Value = "GitLabOAuth")]
        GitLabOAuth,

        [EnumMember(Value = "GoogleOAuth")]
        GoogleOAuth,

        [EnumMember(Value = "IntuitOAuth")]
        IntuitOAuth,

        [EnumMember(Value = "LinkedInOAuth")]
        LinkedInOAuth,

        [EnumMember(Value = "MicrosoftOAuth")]
        MicrosoftOAuth,

        [EnumMember(Value = "SalesforceOAuth")]
        SalesforceOAuth,

        [EnumMember(Value = "SlackOAuth")]
        SlackOAuth,

        [EnumMember(Value = "VercelOAuth")]
        VercelOAuth,

        [EnumMember(Value = "VercelMarketplaceOAuth")]
        VercelMarketplaceOAuth,

        [EnumMember(Value = "XeroOAuth")]
        XeroOAuth,

        [EnumMember(Value = "MagicAuth")]
        MagicAuth,

        [EnumMember(Value = "Impersonation")]
        Impersonation,

        [EnumMember(Value = "CrossAppAuth")]
        CrossAppAuth,

        [EnumMember(Value = "ExternalAuth")]
        ExternalAuth,

        [EnumMember(Value = "MigratedSession")]
        MigratedSession,
    }
}
