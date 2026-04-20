// @oagen-ignore-file
namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using STJS = System.Text.Json.Serialization;

    [JsonConverter(typeof(WorkOSNewtonsoftStringEnumConverter))]
    [STJS.JsonConverter(typeof(WorkOSStringEnumConverterFactory))]
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
