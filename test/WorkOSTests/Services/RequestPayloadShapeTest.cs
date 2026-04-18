// @oagen-ignore-file
namespace WorkOSTests
{
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Demonstrates the request-shape assertion pattern: populate non-default
    /// options, invoke a write or list method, then assert the actual JSON
    /// body / query string sent on the wire — not just the path.
    ///
    /// Generated service tests historically only verified the HTTP method and
    /// path, which left serializer drift, missing fields, and incorrect
    /// snake_case mapping completely uncovered. New AssertRequestBodyJsonContainsAsync
    /// and AssertQueryParam helpers on HttpMock support this pattern.
    /// </summary>
    public class RequestPayloadShapeTest
    {
        [Fact]
        public async Task CreateOrganization_SendsExpectedBodyShape()
        {
            var (mock, client) = NewClient();
            var fixture = File.ReadAllText("testdata/organization.json");
            mock.MockResponse(HttpMethod.Post, "/organizations", HttpStatusCode.OK, fixture);

            await client.Organizations.Create(new OrganizationsCreateOptions
            {
                Name = "Acme Inc.",
                ExternalId = "ext-42",
                Metadata = new System.Collections.Generic.Dictionary<string, string> { { "tier", "diamond" } },
            });

            await mock.AssertRequestBodyJsonContainsAsync(
                "{\"name\":\"Acme Inc.\",\"external_id\":\"ext-42\",\"metadata\":{\"tier\":\"diamond\"}}");
        }

        [Fact]
        public async Task ListOrganizations_SendsPagingQueryParams()
        {
            var (mock, client) = NewClient();
            mock.MockResponse(HttpMethod.Get, "/organizations", HttpStatusCode.OK, "{\"data\":[],\"list_metadata\":{\"before\":null,\"after\":null}}");

            await client.Organizations.List(new OrganizationsListOptions
            {
                Limit = 25,
                After = "cursor_xyz",
            });

            mock.AssertQueryParam("limit", "25");
            mock.AssertQueryParam("after", "cursor_xyz");
        }

        private static (HttpMock Mock, WorkOSClient Client) NewClient()
        {
            var mock = new HttpMock();
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                ClientId = "client_test",
                HttpClient = mock.HttpClient,
            });
            return (mock, client);
        }
    }
}
