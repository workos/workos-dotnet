namespace WorkOS
{
    using System.Net.Http;

    /// <summary>
    /// Describes a request made to the WorkOS API.
    /// </summary>
    public class WorkOSRequest
    {
        /// <summary>
        /// The HTTP method.
        /// </summary>
        public HttpMethod Method { get; set; }

        /// <summary>
        /// The parameters for the HTTP request.
        /// </summary>
        public BaseOptions Options { get; set; }

        /// <summary>
        /// The path of the WorkOS API request.
        /// </summary>
        public string Path { get; set; }
    }
}
