namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the verification state of an Organization Domain.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrganizationDomainState
    {
        [EnumMember(Value = "failed")]
        Failed,

        [EnumMember(Value = "legacy_verified")]
        LegacyVerified,

        [EnumMember(Value = "pending")]
        Pending,

        [EnumMember(Value = "verified")]
        Verified,
    }
}
