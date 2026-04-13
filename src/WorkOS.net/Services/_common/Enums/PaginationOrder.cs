// @oagen-ignore-file
namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    [JsonConverter(typeof(WorkOSNewtonsoftStringEnumConverter))]
    public enum PaginationOrder
    {
        [EnumMember(Value = "normal")]
        Normal,

        [EnumMember(Value = "asc")]
        Asc,

        [EnumMember(Value = "desc")]
        Desc,
    }
}
