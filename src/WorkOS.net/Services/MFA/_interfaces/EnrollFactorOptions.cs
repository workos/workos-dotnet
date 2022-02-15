 namespace WorkOS
{
    using Newtonsoft.Json;
    public class IEnrollFactorOptions : BaseOptions
    {
        /// <summary>
        /// Describes which type to use.
        /// </summary>
        [JsonProperty("type")]
        string type {get; set;}
    }

    sealed class EnrollSmsFactorOptions : IEnrollFactorOptions
    {
        /// <summary>
        /// Phone number for SMS type.
        /// </summary>
        [JsonProperty("phone_number")]
        string PhoneNumber { get;}

        public EnrollSmsFactorOptions(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }
    }


    sealed class EnrollTotpFactorOptions : IEnrollFactorOptions
    {
        /// <summary>
        /// Totp Issuer
        /// </summary>
        [JsonProperty("totp_issuer")]
        string Issuer { get; }

        /// <summary>
        /// Totp user
        /// </summary>
        [JsonProperty("totp_user")]
        string User { get; }

        public EnrollTotpFactorOptions(string issuer, string user)
        {
            Issuer = issuer;
            User = user;
        }
    }
}