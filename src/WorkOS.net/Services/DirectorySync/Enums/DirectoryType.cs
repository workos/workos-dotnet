namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the different types of a <see cref="Directory"/>.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum DirectoryType
    {
        [EnumMember(Value = "azure scim v2.0")]
        AzureSCIMV2_0,

        [EnumMember(Value = "generic scim v1.1")]
        GenericSCIMV1_1,

        [EnumMember(Value = "generic scim v2.0")]
        GenericSCIMV2_0,

        [EnumMember(Value = "okta scim v1.1")]
        OktaSCIMV1_1,

        [EnumMember(Value = "okta scim v2.0")]
        OktaSCIMV2_0,

        [EnumMember(Value = "bamboohr")]
        BambooHR,

        [EnumMember(Value = "gsuite directory")]
        GSuiteDirectory,

        [EnumMember(Value = "workday")]
        Workday,

        [EnumMember(Value = "hibob")]
        HiBob,

        [EnumMember(Value = "jump cloud scim v2.0")]
        JumpCloud,

        [EnumMember(Value = "cyberark scim v2.0")]
        CyberArk,

        [EnumMember(Value = "breathehr")]
        BreatheHR,

        [EnumMember(Value = "bamboohr")]
        BambooHr,

        [EnumMember(Value = "fourth hr")]
        FourthHr,

        [EnumMember(Value = "rippling scim v2.0")]
        RipplingSCIMV2_0,

        [EnumMember(Value = "pingfederate scim v2.0")]
        PingFederateSCIMV2_0,

        [EnumMember(Value = "people hr")]
        PeopleHR,

        [EnumMember(Value = "onelogin scim v2.0")]
        OneLoginSCIMV2_0,
    }
}
