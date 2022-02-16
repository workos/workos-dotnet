namespace WorkOS
{
    using Newtonsoft.Json;

    public sealed class EnrollTotpFactorOptions : EnrollFactorOptions
    {
        public EnrollTotpFactorOptions(string issuer, string user)
        : base("totp")
        {
            this.Issuer = issuer;
            this.User = user;
        }

        /// <summary>
        /// TOTP Issuer.
        /// </summary>
        [JsonProperty("totp_issuer")]
        public string Issuer { get; }

        /// <summary>
        /// TOTP user.
        /// </summary>
        [JsonProperty("totp_user")]
        public string User { get; }
    }
}
