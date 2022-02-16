namespace WorkOS
{
    using Newtonsoft.Json;

    public sealed class EnrollSmsFactorOptions : EnrollFactorOptions
    {
        public EnrollSmsFactorOptions(string phoneNumber)
        {
            this.PhoneNumber = phoneNumber;
        }

        /// <summary>
        /// Phone number for SMS type.
        /// </summary>
        [JsonProperty("phone_number")]
        public string PhoneNumber { get; }
    }
}