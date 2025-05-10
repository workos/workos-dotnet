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

        [EnumMember(Value = "generic scim v2.0")]
        GenericSCIMV2_0,

        [EnumMember(Value = "okta scim v2.0")]
        OktaSCIMV2_0,

        [EnumMember(Value = "bamboohr")]
        BambooHR,

        [EnumMember(Value = "gsuite directory")]
        GSuiteDirectory,

        [EnumMember(Value = "workday")]
        Workday,

        [EnumMember(Value = "hibob")]
        Hibob,

        [EnumMember(Value = "jump cloud scim v2.0")]
        JumpCloud,

        [EnumMember(Value = "cyberark scim v2.0")]
        CyberArk,

        [EnumMember(Value = "breathehr")]
        BreatheHr,

        [EnumMember(Value = "fourth hr")]
        FourthHr,

        [EnumMember(Value = "rippling scim v2.0")]
        RipplingScimV2_0,

        [EnumMember(Value = "pingfederate scim v2.0")]
        PingFederateScimV2_0,

        [EnumMember(Value = "people hr")]
        PeopleHr,

        [EnumMember(Value = "onelogin scim v2.0")]
        OneLoginScimV2_0,

        [EnumMember(Value = "personio")]
        Personio,

        [EnumMember(Value = "cezanne hr")]
        CezanneHr,

        [EnumMember(Value = "sftp")]
        SFTP,

        [EnumMember(Value = "sftp workday")]
        SFTPWorkday,
    }
}
