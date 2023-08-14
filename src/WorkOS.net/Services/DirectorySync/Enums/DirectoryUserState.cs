﻿namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the state of a <see cref="DirectoryUser"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DirectoryUserState
    {
        [EnumMember(Value = "active")]
        Active,
        [EnumMember(Value = "inactive")]
        Inactive,
        [EnumMember(Value = "suspended")]
        Suspended,
    }
}
