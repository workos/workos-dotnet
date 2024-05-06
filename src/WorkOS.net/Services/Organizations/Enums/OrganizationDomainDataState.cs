namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the state of an Organization Domain.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrganizationDomainDataState
    {
        [EnumMember(Value = "verified")]
        Verified,

        [EnumMember(Value = "pending")]
        Pending,
    }
}
