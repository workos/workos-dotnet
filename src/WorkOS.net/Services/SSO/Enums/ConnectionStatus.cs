namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the linked status of a <see cref="Connection"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectionStatus
    {
        [EnumMember(Value = "linked")]
        Linked,

        [EnumMember(Value = "unlinked")]
        Unlinked,
    }
}
