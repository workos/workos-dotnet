namespace WorkOS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Totp properties in Enroll Responses with Totp type.
    /// </summary>
    public class Totp
    {
        /// <summary>
        /// QR code value.
        /// </summary>
        [JsonProperty("qr_code")]
        public string QrCode { get; set; }

        /// <summary>
        /// Totp secret.
        /// </summary>
        [JsonProperty("secret")]
        public string Secret { get; set; }

        /// <summary>
        /// Uri with the secret to code value.
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }
    }
}
