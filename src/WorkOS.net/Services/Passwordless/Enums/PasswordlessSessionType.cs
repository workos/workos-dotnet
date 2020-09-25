namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Enumerations for the different WorkOS Passwordless Session types.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PasswordlessSessionType
    {
        [EnumMember(Value = "MagicLink")]
        MagicLink,
    }
}
