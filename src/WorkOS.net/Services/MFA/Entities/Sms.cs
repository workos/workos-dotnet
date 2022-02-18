namespace WorkOS
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Totp properties in Enroll Responses with Totp type.
    /// </summary>
    public class Sms
    {
        /// <summary>
        /// Phone number.
        /// </summary>
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; set; }
    }
}
