namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectionType
    {
        [EnumMember(Value = "ADFSSAML")]
        ADFSSAML,

        [EnumMember(Value = "AzureSAML")]
        AzureSAML,

        [EnumMember(Value = "GenericSAML")]
        GenericSAML,

        [EnumMember(Value = "GoogleOAuth")]
        GoogleOAuth,

        [EnumMember(Value = "OktaSAML")]
        OktaSAML,

        [EnumMember(Value = "OneLoginSAML")]
        OneLoginSAML,

        [EnumMember(Value = "VMwareSAML")]
        VMwareSAML,
    }
}
