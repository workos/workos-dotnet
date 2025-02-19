namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the verification strategy of an Organization Domain.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrganizationDomainVerificationStrategy
    {
        [EnumMember(Value = "dns")]
        Dns,

        [EnumMember(Value = "manual")]
        Manual,
    }
}
