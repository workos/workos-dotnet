namespace WorkOS
{
    using System;
    using System.Collections.Generic;
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

        /// <summary>
        /// The access token to use for authentication instead of an API key.
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Dictionary of custom WorkOS headers.
        /// </summary>
        public IDictionary<string, string> WorkOSHeaders { get; set; }

        /// <summary>
        /// Optional flag to indicate if the request is JSON encoded.
        /// </summary>
        public bool IsJsonContentType { get; set; } = true;
    }
}
