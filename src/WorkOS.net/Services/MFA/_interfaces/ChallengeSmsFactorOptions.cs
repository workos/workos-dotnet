namespace WorkOS
{
    using Newtonsoft.Json;
    sealed class ChallengeSmsFactorOptions : ChallengeFactorOptions
    {
        /// <summary>
        /// Phone number for SMS type.
        /// </summary>
        [JsonProperty("sms_template")]
        string SmsTemplate { get; }

        public ChallengeSmsFactorOptions(string smsTemplate)
        {
            SmsTemplate = smsTemplate;
        }
    }
}