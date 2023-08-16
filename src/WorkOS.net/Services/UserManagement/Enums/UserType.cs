namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the state of a <see cref="User"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserType
    {
        [EnumMember(Value = "managed")]
        Managed,
        [EnumMember(Value = "unmanaged")]
        Unmanaged,
    }
}
