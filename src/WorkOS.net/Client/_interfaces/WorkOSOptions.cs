// @oagen-ignore-file
namespace WorkOS
{
    using System.Net.Http;

    /// <summary>
    /// Describes options for a Client connecting to the WorkOS API.
    /// </summary>
    public class WorkOSOptions
    {
        /// <summary>
        /// Base URL for the WorkOS API.
        /// </summary>
        public string? ApiBaseURL { get; set; }

        /// <summary>
        /// The API key to authenticate requests to the WorkOS API.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// The WorkOS Client ID (e.g., <c>client_01H...</c>) used for SSO and User Management
        /// endpoints that require an OAuth client identifier.
        /// Required for SSO authorization URLs, User Management authenticate-with-* flows,
        /// and other endpoints that accept a <c>client_id</c> parameter.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// The HTTP client to make API requests.
        /// </summary>
        public HttpClient? HttpClient { get; set; }
    }
}
