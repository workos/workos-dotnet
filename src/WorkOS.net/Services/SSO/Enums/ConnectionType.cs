namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Constants that enumerate available <see cref="Connection"/> types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectionType
    {
        [EnumMember(Value = "ADFSSAML")]
        ADFSSAML,

        [EnumMember(Value = "Auth0SAML")]
        Auth0SAML,

        [EnumMember(Value = "AzureSAML")]
        AzureSAML,

        [EnumMember(Value = "CyberArkSAML")]
        CyberArkSAML,

        [EnumMember(Value = "GenericOIDC")]
        GenericOIDC,

        [EnumMember(Value = "GenericSAML")]
        GenericSAML,

        [EnumMember(Value = "GoogleOAuth")]
        GoogleOAuth,

        [EnumMember(Value = "GoogleSAML")]
        GoogleSAML,

        [EnumMember(Value = "JumpCloudSAML")]
        JumpCloudSAML,

        [EnumMember(Value = "MagicLink")]
        MagicLink,

        [EnumMember(Value = "OktaSAML")]
        OktaSAML,

        [EnumMember(Value = "OneLoginSAML")]
        OneLoginSAML,

        [EnumMember(Value = "PingFederateSAML")]
        PingFederateSAML,

        [EnumMember(Value = "PingOneSAML")]
        PingOneSAML,

        [EnumMember(Value = "SalesforceSAML")]
        SalesforceSAML,

        [EnumMember(Value = "VMwareSAML")]
        VMwareSAML,
    }
}
