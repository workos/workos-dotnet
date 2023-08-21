namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the type of reason for unauthorized access to an organization.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SessionUnauthorizedOrganizationReason
    {
        [EnumMember(Value = "authentication_method_required")]
        AuthenticationMethodRequired,
    }
}
