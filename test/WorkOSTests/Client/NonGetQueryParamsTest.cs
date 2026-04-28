// @oagen-ignore-file
namespace WorkOSTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Verifies that DELETE (and other non-GET request-with-query methods)
    /// correctly serialize their options as query-string parameters instead
    /// of dropping them into a request body.
    /// </summary>
    public class NonGetQueryParamsTest
    {
        [Fact]
        public async Task DeleteWithBoolQueryParam_IncludesCascadeDeleteOnUri()
        {
            var httpMock = new HttpMock();
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                ClientId = "client_test",
                HttpClient = httpMock.HttpClient,
            });

            httpMock.MockResponse(
                HttpMethod.Delete,
                "/authorization/resources/res_1",
                HttpStatusCode.NoContent,
                "{}");

            await client.Authorization.DeleteResource(
                "res_1",
                new AuthorizationDeleteResourceOptions { CascadeDelete = true });

            var last = Assert.Single(httpMock.CapturedRequests);
            Assert.Equal(HttpMethod.Delete, last.Method);
            Assert.Contains("cascade_delete=true", last.RequestUri!.Query);
            Assert.Null(last.Content);
        }

        [Fact]
        public async Task DeleteWithBoolFalse_SerializesAsFalse()
        {
            var httpMock = new HttpMock();
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                ClientId = "client_test",
                HttpClient = httpMock.HttpClient,
            });

            httpMock.MockResponse(
                HttpMethod.Delete,
                "/authorization/resources/res_1",
                HttpStatusCode.NoContent,
                "{}");

            await client.Authorization.DeleteResource(
                "res_1",
                new AuthorizationDeleteResourceOptions { CascadeDelete = false });

            var last = Assert.Single(httpMock.CapturedRequests);
            Assert.Contains("cascade_delete=false", last.RequestUri!.Query);
        }
    }
}
