namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the allowed authentication method.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuthenticationMethod
    {
        [EnumMember(Value = "GoogleOauth")]
        AuthenticationMethodRequired,

        [EnumMember(Value = "MicrosoftOauth")]
        MicrosoftOauth,

        [EnumMember(Value = "MagicAuth")]
        MagicAuth,

        [EnumMember(Value = "Password")]
        Password,
    }
}
