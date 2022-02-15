namespace WorkOS
{
    using Newtonsoft.Json;
    sealed class EnrollSmsFactorOptions : IEnrollFactorOptions
    {
        /// <summary>
        /// Phone number for SMS type.
        /// </summary>
        [JsonProperty("phone_number")]
        string PhoneNumber { get; }

        public EnrollSmsFactorOptions(string phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }
    }
}