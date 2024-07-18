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
        Active,

        [EnumMember(Value = "deleting")]
        Deleting,

        [EnumMember(Value = "unlinked")]
        Inactive,

        [EnumMember(Value = "invalid_credentials")]
        InvalidCredentials,

        [EnumMember(Value = "validating")]
        Validating,
    }
}
