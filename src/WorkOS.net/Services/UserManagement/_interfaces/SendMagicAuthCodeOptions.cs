namespace WorkOS
{
    using Newtonsoft.Json;

    /// <summary>
    /// The parameters to create a User.
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
