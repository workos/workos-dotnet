namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PaginationOrder
    {
        [EnumMember(Value = "asc")]
        Asc,

        [EnumMember(Value = "desc")]
        Desc
    }

}