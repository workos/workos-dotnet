namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Constants that enumerate available <see cref="Connection"/> types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Type
    {
        [EnumMember(Value = "ADFSSAML")]
        ADFSSAML,

        [EnumMember(Value = "AdpOidc")]
        AdpOidc,

        [EnumMember(Value = "Auth0SAML")]
        Auth0SAML,

        [EnumMember(Value = "AzureSAML")]
        AzureSAML,

        [EnumMember(Value = "CasSAML")]
        CasSAML,

        [EnumMember(Value = "CloudflareSAML")]
        CloudflareSAML,

        [EnumMember(Value = "ClasslinkSAML")]
        ClassLinkSAML,

        [EnumMember(Value = "CyberArkSAML")]
        CyberArkSAML,

        [EnumMember(Value = "DuoSAML")]
        DuoSAML,

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

        [EnumMember(Value = "KeycloakSAML")]
        KeycloakSAML,

        [EnumMember(Value = "LastPassSAML")]
        LastPassSAML,

        [EnumMember(Value = "MagicLink")]
        MagicLink,

        [EnumMember(Value = "MicrosoftOAuth")]
        MicrosoftOAuth,

        [EnumMember(Value = "MiniOrangeSAML")]
        MiniOrangeSAML,

        [EnumMember(Value = "NetIqSAML")]
        NetIqSAML,

        [EnumMember(Value = "OktaSAML")]
        OktaSAML,

        [EnumMember(Value = "OneLoginSAML")]
        OneLoginSAML,

        [EnumMember(Value = "OracleSAML")]
        OracleSAML,

        [EnumMember(Value = "PingFederateSAML")]
        PingFederateSAML,

        [EnumMember(Value = "PingOneSAML")]
        PingOneSAML,

        [EnumMember(Value = "RipplingSAML")]
        RipplingSAML,

        [EnumMember(Value = "SalesforceSAML")]
        SalesforceSAML,

        [EnumMember(Value = "ShibbolethGenericSAML")]
        ShibbolethGenericSAML,

        [EnumMember(Value = "ShibbolethSAML")]
        ShibbolethSAML,

        [EnumMember(Value = "SimpleSamlPhpSAML")]
        SimpleSamlPhpSAML,

        [EnumMember(Value = "VMwareSAML")]
        VMwareSAML,
    }
}
