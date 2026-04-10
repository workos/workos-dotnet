// @oagen-ignore-file
namespace WorkOSTests
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    public class WorkOSClientTest
    {
        [Fact]
        public void TestEmptyAPIKey()
        {
            Assert.Throws<ArgumentException>(
                () => new WorkOSClient(new WorkOSOptions { }));
        }

        [Fact]
        public void TestDefaultBaseUrl()
        {
            var client = new WorkOSClient(new WorkOSOptions { ApiKey = "sk_test" });
            Assert.Equal("https://api.workos.com", client.ApiBaseURL);
        }

        [Fact]
        public void TestCustomBaseUrl()
        {
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                ApiBaseURL = "https://custom.example.com",
            });
            Assert.Equal("https://custom.example.com", client.ApiBaseURL);
        }

        [Fact]
        public async Task TestAuthorizationHeader()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.OK, "{}");

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test_auth_header",
                HttpClient = httpMock.HttpClient,
            });

            await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = "/test",
            });

            Assert.Single(httpMock.CapturedRequests);
            var request = httpMock.CapturedRequests[0];
            Assert.Equal("Bearer", request.Headers.Authorization.Scheme);
            Assert.Equal("sk_test_auth_header", request.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task TestUserAgentHeader()
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
            Assert.Contains("workos-dotnet/", request.Headers.UserAgent.ToString());
        }

        [Fact]
        public async Task TestRequestOptionsApiKeyOverride()
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.OK, "{}");

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_default",
                HttpClient = httpMock.HttpClient,
            });

            await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = "/test",
                RequestOptions = new RequestOptions { ApiKey = "sk_override" },
            });

            Assert.Single(httpMock.CapturedRequests);
            var request = httpMock.CapturedRequests[0];
            Assert.Equal("sk_override", request.Headers.Authorization.Parameter);
        }

        [Fact]
        public async Task TestRequestOptionsIdempotencyKey()
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
                RequestOptions = new RequestOptions { IdempotencyKey = "idem_123" },
            });

            Assert.Single(httpMock.CapturedRequests);
            var request = httpMock.CapturedRequests[0];
            Assert.True(request.Headers.Contains("Idempotency-Key"));
            Assert.Equal("idem_123", request.Headers.GetValues("Idempotency-Key").First());
        }
    }
}
