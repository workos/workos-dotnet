namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the state of a <see cref="User"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuthenticationMethods
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
