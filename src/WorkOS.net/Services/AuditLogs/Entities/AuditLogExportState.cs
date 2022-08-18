namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the state of an <see cref="AuditLogExport"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum AuditLogExportState
    {
        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "ready")]
        Ready,

        [EnumMember(Value = "error")]
        Error,
    }
}
