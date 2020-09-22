namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the linked state of a <see cref="Directory"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DirectoryState
    {
        [EnumMember(Value = "linked")]
        Linked,

        [EnumMember(Value = "unlinked")]
        Unlinked,
    }
}
