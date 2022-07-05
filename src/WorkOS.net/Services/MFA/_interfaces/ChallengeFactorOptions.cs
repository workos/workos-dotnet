 namespace WorkOS
{
    using Newtonsoft.Json;

    public class ChallengeFactorOptions : BaseOptions
    {
        /// <summary>
        /// Authentication Factor ID.
        /// </summary>
        [JsonIgnore]
        public string FactorId { get; set; }
    }
}
