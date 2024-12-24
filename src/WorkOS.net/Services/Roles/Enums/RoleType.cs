namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the type of a Role.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RoleType
    {
        [EnumMember(Value = "EnvironmentRole")]
        Environment,

        [EnumMember(Value = "OrganizationRole")]
        Organization,
    }
}
