namespace WorkOS
{
    using Newtonsoft.Json;

    public sealed class ChallengeSmsFactorOptions : ChallengeFactorOptions
    {
        public ChallengeSmsFactorOptions(string smsTemplate)
        {
            this.SmsTemplate = smsTemplate;
        }

        /// <summary>
        /// Phone number for SMS type.
        /// </summary>
        [JsonProperty("sms_template")]
        public string SmsTemplate { get; }
    }
}
