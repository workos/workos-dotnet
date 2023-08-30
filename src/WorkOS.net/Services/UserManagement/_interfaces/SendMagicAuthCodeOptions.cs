namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to send Magic Auth code.
    /// </summary>
    public class SendMagicAuthCodeOptions : BaseOptions
    {
        /// <summary>
        /// The email address of the user.
        /// </summary>
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
