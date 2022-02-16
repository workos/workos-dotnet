namespace WorkOS
{
    using Newtonsoft.Json;

    public sealed class EnrollTotpFactorOptions : EnrollFactorOptions
    {
        public EnrollTotpFactorOptions(string issuer, string user)
        {
            this.Issuer = issuer;
            this.User = user;
        }

        /// <summary>
        /// Totp Issuer.
        /// </summary>
        [JsonProperty("totp_issuer")]
        public string Issuer { get; }

        /// <summary>
        /// Totp user.
        /// </summary>
        [JsonProperty("totp_user")]
        public string User { get; }
    }
}