// @oagen-ignore-file
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
        public HttpMethod Method { get; set; } = default!;

        /// <summary>
        /// The parameters for the HTTP request.
        /// </summary>
        public BaseOptions? Options { get; set; }

        /// <summary>
        /// The path of the WorkOS API request.
        /// </summary>
        public string Path { get; set; } = default!;

        /// <summary>
        /// The access token to use for authentication instead of an API key.
        /// </summary>
        public string? AccessToken { get; set; }

        /// <summary>
        /// Dictionary of custom WorkOS headers.
        /// </summary>
        public IDictionary<string, string>? WorkOSHeaders { get; set; }

        /// <summary>
        /// Optional flag to indicate if the request is JSON encoded.
        /// </summary>
        public bool IsJsonContentType { get; set; } = true;

        /// <summary>
        /// Per-request configuration overrides (API key, idempotency key, etc.).
        /// </summary>
        public RequestOptions? RequestOptions { get; set; }

        /// <summary>
        /// Extra query parameters injected by parameter-group dispatch
        /// (e.g. discriminated parent-resource unions). These are appended
        /// to the URL query string alongside the options-derived params.
        /// </summary>
        internal Dictionary<string, string>? ExtraQueryParams { get; set; }

        /// <summary>
        /// Extra body parameters injected by parameter-group dispatch
        /// (e.g. password variants for user creation). These are merged
        /// into the JSON request body alongside the options-derived fields.
        /// </summary>
        internal Dictionary<string, string>? ExtraBodyParams { get; set; }

        /// <summary>
        /// Append an extra query parameter to the request.
        /// </summary>
        internal void AddQueryParam(string key, string value)
        {
            this.ExtraQueryParams ??= new Dictionary<string, string>();
            this.ExtraQueryParams[key] = value;
        }

        /// <summary>
        /// Append an extra body parameter to the request. The value is
        /// merged into the serialized JSON body.
        /// </summary>
        internal void AddBodyParam(string key, string value)
        {
            this.ExtraBodyParams ??= new Dictionary<string, string>();
            this.ExtraBodyParams[key] = value;
        }
    }
}
