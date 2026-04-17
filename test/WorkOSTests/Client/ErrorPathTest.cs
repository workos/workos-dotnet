// @oagen-ignore-file
namespace WorkOSTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Verifies that non-2xx HTTP responses are mapped to the correct typed
    /// exception class (sdk-runtime-contract §3, §6).
    /// </summary>
    public class ErrorPathTest
    {
        [Fact]
        public async Task Returns401_ThrowsAuthenticationException()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<AuthenticationException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = "/test",
                }));

            Assert.Equal(HttpStatusCode.Unauthorized, ex.StatusCode);
            Assert.Contains("Unauthorized", ex.Message);
        }

        [Fact]
        public async Task Returns404_ThrowsNotFoundException()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.NotFound, "{\"message\":\"Not Found\"}");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<NotFoundException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = "/test",
                }));

            Assert.Equal(HttpStatusCode.NotFound, ex.StatusCode);
            Assert.Contains("Not Found", ex.Message);
        }

        [Fact]
        public async Task Returns422_ThrowsUnprocessableEntityException()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(
                (HttpStatusCode)422,
                "{\"message\":\"Validation failed\"}");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Post,
                    Path = "/test",
                }));

            Assert.Equal((HttpStatusCode)422, ex.StatusCode);
            Assert.Contains("Validation failed", ex.Message);
        }

        [Fact]
        public async Task Returns429_ThrowsRateLimitExceededException()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(
                (HttpStatusCode)429,
                "{\"message\":\"Too Many Requests\"}");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<RateLimitExceededException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = "/test",
                }));

            Assert.Equal((HttpStatusCode)429, ex.StatusCode);
            Assert.Contains("Too Many Requests", ex.Message);
        }

        [Fact]
        public async Task Returns500_ThrowsServerException()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(
                HttpStatusCode.InternalServerError,
                "{\"message\":\"Internal Server Error\"}");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<ServerException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = "/test",
                }));

            Assert.Equal(HttpStatusCode.InternalServerError, ex.StatusCode);
            Assert.Contains("Internal Server Error", ex.Message);
        }

        [Fact]
        public async Task Returns400_ThrowsApiException()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(
                HttpStatusCode.BadRequest,
                "{\"message\":\"Bad Request\"}");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<ApiException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Post,
                    Path = "/test",
                }));

            Assert.Equal(HttpStatusCode.BadRequest, ex.StatusCode);
            Assert.Contains("Bad Request", ex.Message);
        }

        [Fact]
        public async Task Returns400_IsNotRetried()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(
                HttpStatusCode.BadRequest,
                "{\"message\":\"Bad Request\"}");
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 2,
            });

            await Assert.ThrowsAsync<ApiException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Post,
                    Path = "/test",
                }));

            // 400 is a client error — should NOT be retried, so only 1 request.
            Assert.Single(httpMock.CapturedRequests);
        }

        [Fact]
        public async Task StructuredError_ParsesCodeAndMessage()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(
                (HttpStatusCode)422,
                "{\"message\":\"Validation failed\",\"code\":\"invalid_field\"}");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Post,
                    Path = "/test",
                }));

            Assert.Equal("invalid_field", ex.Code);
            Assert.Equal("Validation failed", ex.ErrorMessage);
            Assert.Empty(ex.Errors);
        }

        [Fact]
        public async Task StructuredError_ParsesErrorsArray()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(
                (HttpStatusCode)422,
                "{\"message\":\"Validation failed\",\"code\":\"invalid_field\",\"errors\":[{\"instancePath\":\"/name\"}]}");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<UnprocessableEntityException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Post,
                    Path = "/test",
                }));

            Assert.Single(ex.Errors);
            Assert.Equal("/name", ex.Errors[0]["instancePath"]);
        }

        [Fact]
        public async Task StructuredError_NonJsonBody_DoesNotThrow()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(
                HttpStatusCode.InternalServerError,
                "plain text error");
            var client = CreateClient(httpMock);

            var ex = await Assert.ThrowsAsync<ServerException>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = "/test",
                }));

            Assert.Null(ex.Code);
            Assert.Null(ex.ErrorMessage);
            Assert.Empty(ex.Errors);
            Assert.Equal("plain text error", ex.RawBody);
        }

        private static WorkOSClient CreateClient(HttpMock httpMock)
        {
            return new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 0,
            });
        }
    }
}
