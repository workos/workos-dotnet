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

        [Theory]
        [InlineData(".")]
        [InlineData("..")]
        [InlineData("%2E")]
        [InlineData("%2e")]
        [InlineData("%2E%2E")]
        [InlineData("%2e%2e")]
        [InlineData("%2E%2e")]
        [InlineData(".%2E")]
        [InlineData("%2e.")]
        public async Task TestBuildRequestUriRejectsDotSegments(string segment)
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.OK, "{}");
            using var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
            });
            foreach (var path in new[] { $"/{segment}", $"/{segment}/test", $"/test/{segment}", $"/test/{segment}/", $"/test/{segment}/child" })
            {
                var request = new WorkOSRequest
                {
                    Method = HttpMethod.Get,
                    Path = path,
                };
                var exception = Assert.Throws<ArgumentException>(() => client.BuildRequestUri(request));
                Assert.Equal("Path", exception.ParamName);
                await Assert.ThrowsAsync<ArgumentException>(() => client.MakeRawAPIRequest(request));
            }

            Assert.Empty(httpMock.CapturedRequests);
        }

        [Theory]
        [InlineData("/organizations/org_123/feature-flags")]
        [InlineData("/test/a.b")]
        [InlineData("/test/.hidden")]
        [InlineData("/test/name..")]
        [InlineData("/test/...")]
        [InlineData("/test/%252E%252E")]
        [InlineData("/test/a%2Fb")]
        [InlineData("/test/..%2Fchild")]
        [InlineData("/test/%2F..")]
        [InlineData("/test/a%5Cb")]
        [InlineData("/test/a%20b%3Fc%23d")]
        [InlineData("/test//child/")]
        [InlineData("/test/")]
        [InlineData("/")]
        public void TestBuildRequestUriPreservesNonDotSegments(string path)
        {
            using var client = new WorkOSClient(new WorkOSOptions { ApiKey = "sk_test" });
            var uri = client.BuildRequestUri(new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = path,
                Options = new ListOptions { After = ".." },
            });

            Assert.Equal(path, uri.AbsolutePath);
            Assert.Contains("after=..", uri.Query);
        }

        [Theory]
        [InlineData(".")]
        [InlineData("..")]
        public async Task TestDotSegmentIdsNeverSendRequests(string id)
        {
            var httpMock = new HttpMock();
            httpMock.MockResponseForAnyRequest(HttpStatusCode.OK, "{}");
            using var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = httpMock.HttpClient,
            });

            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.Groups.DeleteOrganizationMembershipAsync("org_123", "group_123", id));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.Groups.DeleteOrganizationGroupAsync("org_123", id));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.Authorization.RemoveOrganizationRolePermissionAsync("org_123", "role", id));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.Pipes.DeleteUserConnectedAccountAsync("user_123", id));
            await Assert.ThrowsAsync<ArgumentException>(() =>
                client.FeatureFlags.ListOrganizationFeatureFlagsAsync(id));

            Assert.Empty(httpMock.CapturedRequests);
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
