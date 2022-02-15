namespace WorkOS
{
    using Newtonsoft.Json;
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