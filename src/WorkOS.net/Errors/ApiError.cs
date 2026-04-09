// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Net;

    public class ApiError : Exception
    {
        public ApiError(string message, HttpStatusCode statusCode)
            : base(message)
        {
            this.StatusCode = statusCode;
        }

        public HttpStatusCode StatusCode { get; }
    }

    public class AuthenticationError : ApiError
    {
        public AuthenticationError(string message)
            : base(message, HttpStatusCode.Unauthorized)
        {
        }
    }

    public class NotFoundError : ApiError
    {
        public NotFoundError(string message)
            : base(message, HttpStatusCode.NotFound)
        {
        }
    }

    public class UnprocessableEntityError : ApiError
    {
        public UnprocessableEntityError(string message)
            : base(message, (HttpStatusCode)422)
        {
        }
    }

    public class RateLimitExceededError : ApiError
    {
        public RateLimitExceededError(string message)
            : base(message, (HttpStatusCode)429)
        {
        }
    }

    public class ServerError : ApiError
    {
        public ServerError(string message)
            : base(message, HttpStatusCode.InternalServerError)
        {
        }
    }
}
