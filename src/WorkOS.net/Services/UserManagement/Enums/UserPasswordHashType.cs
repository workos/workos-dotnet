namespace WorkOS
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An enum describing the password hash types for a User's password.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserPasswordHashType
    {
        [EnumMember(Value = "bcrypt")]
        Bcrypt,

        [EnumMember(Value = "scrypt")]
        Scrypt,

        [EnumMember(Value = "firebase-scrypt")]
        FirebaseScrypt,

        [EnumMember(Value = "ssha")]
        Ssha,

        [EnumMember(Value = "pbkdf2")]
        Pbkdf2,

        [EnumMember(Value = "argon2")]
        Argon2,
    }
}
