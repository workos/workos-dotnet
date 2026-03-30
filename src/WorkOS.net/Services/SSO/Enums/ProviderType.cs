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
    }
}
