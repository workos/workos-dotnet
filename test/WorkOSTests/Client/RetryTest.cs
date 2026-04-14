// @oagen-ignore-file
#nullable enable
namespace WorkOSTests
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Verifies retry behavior: exponential backoff, Retry-After handling,
    /// per-request override, and auto-idempotency (sdk-runtime-contract §2, §6).
    /// </summary>
    public class RetryTest
    {
        [Fact]
        public async Task Retries429ThenSucceeds()
        {
            var httpMock = new HttpMock();
            httpMock.MockSequentialResponseMessagesForAnyRequest(new[]
            {
                ((HttpStatusCode)429, "{\"message\":\"rate limited\"}", (IDictionary<string, string>?)null),
                (HttpStatusCode.OK, "{}", (IDictionary<string, string>?)null),
            });

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 2,
            });

            var response = await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = "/test",
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Retries500ThenSucceeds()
        {
            var httpMock = new HttpMock();
            httpMock.MockSequentialResponseMessagesForAnyRequest(new[]
            {
                (HttpStatusCode.InternalServerError, "{\"message\":\"oops\"}", (IDictionary<string, string>?)null),
                (HttpStatusCode.OK, "{}", (IDictionary<string, string>?)null),
            });

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 2,
            });

            var response = await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = "/test",
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task DoesNotRetry400()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.BadRequest, "{\"message\":\"bad\"}");

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 2,
            });

            await Assert.ThrowsAsync<ApiError>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = "/test",
                }));

            Assert.Single(httpMock.CapturedRequests);
        }

        [Fact]
        public async Task RetriesUpToMaxThenThrows()
        {
            var httpMock = new HttpMock();
            httpMock.MockSequentialResponseMessagesForAnyRequest(new[]
            {
                (HttpStatusCode.InternalServerError, "{\"message\":\"fail 1\"}", (IDictionary<string, string>?)null),
                (HttpStatusCode.InternalServerError, "{\"message\":\"fail 2\"}", (IDictionary<string, string>?)null),
                (HttpStatusCode.InternalServerError, "{\"message\":\"fail 3\"}", (IDictionary<string, string>?)null),
            });

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 2,
            });

            var ex = await Assert.ThrowsAsync<ServerError>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = "/test",
                }));

            Assert.Contains("fail 3", ex.Message);
        }

        [Fact]
        public async Task HonorsRetryAfterHeader()
        {
            var httpMock = new HttpMock();
            var retryAfterHeaders = new Dictionary<string, string> { ["Retry-After"] = "1" };
            httpMock.MockSequentialResponseMessagesForAnyRequest(new (HttpStatusCode, string, IDictionary<string, string>?)[]
            {
                ((HttpStatusCode)429, "{\"message\":\"rate limited\"}", retryAfterHeaders),
                (HttpStatusCode.OK, "{}", null),
            });

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 1,
            });

            var sw = Stopwatch.StartNew();
            var response = await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = "/test",
            });
            sw.Stop();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.True(sw.ElapsedMilliseconds >= 900, $"Expected >= 900ms delay for Retry-After: 1, got {sw.ElapsedMilliseconds}ms");
        }

        [Fact]
        public async Task PerRequestMaxRetriesOverride()
        {
            var httpMock = new HttpMock();
            httpMock.MockSequentialResponseMessagesForAnyRequest(new[]
            {
                (HttpStatusCode.InternalServerError, "{\"message\":\"fail\"}", (IDictionary<string, string>?)null),
                (HttpStatusCode.OK, "{}", (IDictionary<string, string>?)null),
            });

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 0,
            });

            var response = await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = "/test",
                RequestOptions = new RequestOptions { MaxRetries = 1 },
            });

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task PostRequestsGetAutoIdempotencyKey()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.OK, "{}");

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
            });

            await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = "/test",
            });

            Assert.Single(httpMock.CapturedRequests);
            var request = httpMock.CapturedRequests[0];
            Assert.True(request.Headers.Contains("Idempotency-Key"));
            var key = request.Headers.GetValues("Idempotency-Key").First();
            Assert.False(string.IsNullOrWhiteSpace(key));
        }

        [Fact]
        public async Task GetRequestsDoNotGetAutoIdempotencyKey()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.OK, "{}");

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
            });

            await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = "/test",
            });

            Assert.Single(httpMock.CapturedRequests);
            var request = httpMock.CapturedRequests[0];
            Assert.False(request.Headers.Contains("Idempotency-Key"));
        }

        [Fact]
        public async Task ExplicitIdempotencyKeyIsPreserved()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.OK, "{}");

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
            });

            await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = "/test",
                RequestOptions = new RequestOptions { IdempotencyKey = "my_custom_key" },
            });

            Assert.Single(httpMock.CapturedRequests);
            var request = httpMock.CapturedRequests[0];
            Assert.Equal("my_custom_key", request.Headers.GetValues("Idempotency-Key").First());
        }

        [Fact]
        public async Task ZeroRetriesDisablesRetry()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.InternalServerError, "{\"message\":\"fail\"}");

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
                MaxRetries = 0,
            });

            await Assert.ThrowsAsync<ServerError>(() =>
                client.MakeRawAPIRequest(new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = "/test",
                }));

            Assert.Single(httpMock.CapturedRequests);
        }
    }
}
