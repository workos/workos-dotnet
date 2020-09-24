namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Enumerations for the different intents when creating a Portal link.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Intent
    {
        [EnumMember(Value = "sso")]
        SSO,
    }
}
