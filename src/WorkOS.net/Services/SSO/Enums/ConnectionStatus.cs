namespace WorkOS
{
    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the linked status of a <see cref="Connection"/>.
    /// </summary>
    [ObsoleteAttribute("The status property is obsolete. Please use state instead.", false)]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectionStatus
    {
        [EnumMember(Value = "linked")]
        Linked,

        [EnumMember(Value = "unlinked")]
        Unlinked,
    }
}
