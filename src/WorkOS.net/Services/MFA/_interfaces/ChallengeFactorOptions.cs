 namespace WorkOS
{
    using Newtonsoft.Json;
    public class IChallengeFactorOptions : BaseOptions
    {
        /// <summary>
        /// Authentication Factor ID
        /// </summary>
        [JsonProperty("authentication_factor_id")]
        string type {get; set;}
    }

    sealed class ChallengeSmsFactorOptions : IEnrollFactorOptions
    {
        /// <summary>
        /// Phone number for SMS type.
        /// </summary>
        [JsonProperty("sms_template")]
        string SmsTemplate { get;}

        public ChallengeSmsFactorOptions(string smsTemplate)
        {
            SmsTemplate = smsTemplate;
        }
    }
}