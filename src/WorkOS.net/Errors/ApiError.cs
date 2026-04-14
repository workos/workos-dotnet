// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Net;

    /// <summary>
    /// Base exception type for all non-2xx HTTP responses from the WorkOS API.
    /// The SDK maps known status codes to the specific subclasses below; any other
    /// status code (e.g. 400, 403, 409) surfaces as an <see cref="ApiError"/> directly.
    /// The <see cref="Exception.Message"/> contains the raw response body.
    /// </summary>
    public class ApiError : Exception
    {
        public ApiError(string message, HttpStatusCode statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        /// <summary>The HTTP status code returned by the API.</summary>
        public HttpStatusCode StatusCode { get; }
    }

    /// <summary>Thrown when the API rejects the provided credentials (HTTP 401).</summary>
    public class AuthenticationError : ApiError
    {
        public AuthenticationError(string message)
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }

    /// <summary>Thrown when the requested resource does not exist (HTTP 404).</summary>
    public class NotFoundError : ApiError
    {
        public NotFoundError(string message)
            : base(message, HttpStatusCode.NotFound)
        {
        }
    }

    /// <summary>
    /// Thrown when the request body is syntactically valid but semantically
    /// unacceptable (HTTP 422) — e.g. a missing required field or a value that
    /// fails server-side validation.
    /// </summary>
    public class UnprocessableEntityError : ApiError
    {
        public UnprocessableEntityError(string message)
            : base(message, (HttpStatusCode)422)
        {
        }
    }

    /// <summary>
    /// Thrown when the caller has exceeded the API rate limit (HTTP 429).
    /// The SDK automatically retries 429 responses up to
    /// <see cref="WorkOSOptions.MaxRetries"/> times with exponential backoff,
    /// honoring the <c>Retry-After</c> header when present. This exception is
    /// thrown only after all retries have been exhausted.
    /// </summary>
    public class RateLimitExceededError : ApiError
    {
        public RateLimitExceededError(string message)
            : base(message, (HttpStatusCode)429)
        {
        }
    }

    /// <summary>
    /// Thrown for HTTP 5xx responses from the WorkOS API. The SDK automatically
    /// retries 5xx responses up to <see cref="WorkOSOptions.MaxRetries"/> times
    /// with exponential backoff. This exception is thrown only after all retries
    /// have been exhausted.
    /// </summary>
    public class ServerError : ApiError
    {
        public ServerError(string message)
            : base(message, HttpStatusCode.InternalServerError)
        {
        }
    }
}
