// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Base exception type for all non-2xx HTTP responses from the WorkOS API.
    /// The SDK maps known status codes to the specific subclasses below; any other
    /// status code (e.g. 400, 403, 409) surfaces as an <see cref="ApiException"/> directly.
    /// The <see cref="Exception.Message"/> contains the raw response body.
    /// Structured fields (<see cref="Code"/>, <see cref="Errors"/>) are best-effort
    /// parsed from the JSON response body when available.
    /// </summary>
    public class ApiException : Exception
    {
        public ApiException(string message, HttpStatusCode statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
            this.RawBody = message;
            this.ParseBody(message);
        }

        /// <summary>The HTTP status code returned by the API.</summary>
        public HttpStatusCode StatusCode { get; }

        /// <summary>The raw response body string.</summary>
        public string RawBody { get; }

        /// <summary>
        /// Machine-readable error code from the response body (e.g.
        /// <c>"invalid_audit_log_event"</c>, <c>"mfa_challenge_failed"</c>).
        /// <c>null</c> when the response body does not contain a <c>code</c> field.
        /// </summary>
        public string? Code { get; private set; }

        /// <summary>
        /// Human-readable error message extracted from the response body's
        /// <c>message</c> field. <c>null</c> when the response body is not JSON
        /// or does not contain a <c>message</c> field.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Field-level validation errors from the response body. Each entry
        /// contains an <c>instancePath</c> key indicating the JSON path to the
        /// invalid field. Empty when no <c>errors</c> array is present.
        /// </summary>
        public IReadOnlyList<Dictionary<string, object>> Errors { get; private set; } = Array.Empty<Dictionary<string, object>>();

        private void ParseBody(string body)
        {
            if (string.IsNullOrEmpty(body))
            {
                return;
            }

            try
            {
                var obj = JObject.Parse(body);
                this.Code = obj.Value<string>("code");
                this.ErrorMessage = obj.Value<string>("message");

                var errorsToken = obj["errors"];
                if (errorsToken is JArray errorsArray)
                {
                    var list = new List<Dictionary<string, object>>();
                    foreach (var item in errorsArray)
                    {
                        var dict = item.ToObject<Dictionary<string, object>>();
                        if (dict != null)
                        {
                            list.Add(dict);
                        }
                    }

                    this.Errors = list;
                }
            }
            catch (JsonException)
            {
                // Response body is not JSON — structured fields remain at defaults.
            }
        }
    }

    /// <summary>Thrown when the API rejects the provided credentials (HTTP 401).</summary>
    public class AuthenticationException : ApiException
    {
        public AuthenticationException(string message)
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }

    /// <summary>Thrown when the requested resource does not exist (HTTP 404).</summary>
    public class NotFoundException : ApiException
    {
        public NotFoundException(string message)
            : base(message, HttpStatusCode.NotFound)
        {
        }
    }

    /// <summary>
    /// Thrown when the request body is syntactically valid but semantically
    /// unacceptable (HTTP 422) — e.g. a missing required field or a value that
    /// fails server-side validation.
    /// </summary>
    public class UnprocessableEntityException : ApiException
    {
        public UnprocessableEntityException(string message)
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
    public class RateLimitExceededException : ApiException
    {
        public RateLimitExceededException(string message)
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
    public class ServerException : ApiException
    {
        public ServerException(string message)
            : base(message, HttpStatusCode.InternalServerError)
        {
        }
    }
}
