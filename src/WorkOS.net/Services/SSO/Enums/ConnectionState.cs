namespace WorkOS
{
    using System;
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the linked state of a <see cref="Connection"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectionState
    {
        [EnumMember(Value = "active")]
        Active,

        [EnumMember(Value = "draft")]
        Draft,

        [EnumMember(Value = "inactive")]
        Inactive,

        [EnumMember(Value = "validating")]
        Validating,
    }
}
