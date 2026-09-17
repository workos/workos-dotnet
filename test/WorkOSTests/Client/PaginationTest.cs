// @oagen-ignore-file
namespace WorkOSTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Verifies pagination behavior: empty pages, multi-page auto-pagination,
    /// and cursor advancement (sdk-runtime-contract §4, §6).
    /// </summary>
    public class PaginationTest
    {
        private readonly HttpMock httpMock;
        private readonly WorkOSClient client;
        private readonly OrganizationsService service;

        public PaginationTest()
        {
            this.httpMock = new HttpMock();
            this.client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = this.httpMock.HttpClient,
                MaxRetries = 0,
            });
            this.service = new OrganizationsService(this.client);
        }

        [Fact]
        public async Task ListReturnsEmptyPage()
        {
            var emptyPage = "{\"data\":[],\"list_metadata\":{\"before\":null,\"after\":null}}";
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/organizations",
                HttpStatusCode.OK,
                emptyPage);

            var result = await this.service.List();

            Assert.NotNull(result);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data);
            Assert.Null(result.ListMetadata?.After);
        }

        [Fact]
        public async Task ListAutoPagingAsyncFetchesMultiplePages()
        {
            var page1 = @"{
                ""data"": [
                    {""object"":""organization"",""id"":""org_1"",""name"":""Org 1"",""domains"":[],""metadata"":{},""created_at"":""2026-01-01T00:00:00Z"",""updated_at"":""2026-01-01T00:00:00Z""}
                ],
                ""list_metadata"": {""before"":null,""after"":""cursor_abc""}
            }";
            var page2 = @"{
                ""data"": [
                    {""object"":""organization"",""id"":""org_2"",""name"":""Org 2"",""domains"":[],""metadata"":{},""created_at"":""2026-01-01T00:00:00Z"",""updated_at"":""2026-01-01T00:00:00Z""}
                ],
                ""list_metadata"": {""before"":null,""after"":null}
            }";

            this.httpMock.MockSequentialResponses(
                HttpMethod.Get,
                "/organizations",
                HttpStatusCode.OK,
                new[] { page1, page2 });

            var items = new List<Organization>();
            await foreach (var org in this.service.ListAutoPagingAsync())
            {
                items.Add(org);
            }

            Assert.Equal(2, items.Count);
            Assert.Equal("org_1", items[0].Id);
            Assert.Equal("org_2", items[1].Id);
        }

        [Fact]
        public async Task ListAutoPagingAsyncEmptyFirstPage()
        {
            var emptyPage = "{\"data\":[],\"list_metadata\":{\"before\":null,\"after\":null}}";
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/organizations",
                HttpStatusCode.OK,
                emptyPage);

            var items = new List<Organization>();
            await foreach (var org in this.service.ListAutoPagingAsync())
            {
                items.Add(org);
            }

            Assert.Empty(items);
        }

        [Fact]
        public async Task ListAutoPagingAsyncDoesNotMutateCallerOptions()
        {
            var page1 = @"{
                ""data"": [
                    {""object"":""organization"",""id"":""org_1"",""name"":""Org 1"",""domains"":[],""metadata"":{},""created_at"":""2026-01-01T00:00:00Z"",""updated_at"":""2026-01-01T00:00:00Z""}
                ],
                ""list_metadata"": {""before"":null,""after"":""cursor_abc""}
            }";
            var page2 = @"{
                ""data"": [
                    {""object"":""organization"",""id"":""org_2"",""name"":""Org 2"",""domains"":[],""metadata"":{},""created_at"":""2026-01-01T00:00:00Z"",""updated_at"":""2026-01-01T00:00:00Z""}
                ],
                ""list_metadata"": {""before"":null,""after"":null}
            }";

            this.httpMock.MockSequentialResponses(
                HttpMethod.Get,
                "/organizations",
                HttpStatusCode.OK,
                new[] { page1, page2 });

            var opts = new OrganizationsListOptions { Limit = 1 };
            var items = new List<Organization>();
            await foreach (var item in this.service.ListAutoPagingAsync(opts))
            {
                items.Add(item);
            }

            // The caller's options must not have been mutated with cursor state.
            Assert.Null(opts.After);
        }

        [Fact]
        public async Task ListAutoPagingAsyncCarriesGroupedQueryParamsToEveryPage()
        {
            // Regression guard for VULN-1274. Grouped (mutually exclusive)
            // options such as Parent are JsonIgnore'd and only reach the wire
            // through the service's per-variant dispatch. The pager used to
            // bypass that dispatch, so a parent-scoped listing silently came
            // back as the unfiltered environment-wide listing.
            var fixture = System.IO.File.ReadAllText("testdata/authorization_resource.json");
            var page1 = "{\"data\":[" + fixture + "],\"list_metadata\":{\"before\":null,\"after\":\"cursor_abc\"}}";
            var page2 = "{\"data\":[" + fixture + "],\"list_metadata\":{\"before\":null,\"after\":null}}";
            this.httpMock.MockSequentialResponses(
                HttpMethod.Get,
                "/authorization/resources",
                HttpStatusCode.OK,
                new[] { page1, page2 });

            var authorization = new AuthorizationService(this.client);
            var options = new AuthorizationListResourcesOptions
            {
                Parent = new AuthorizationParentById { ParentResourceId = "authz_resource_parent" },
            };

            var items = new List<AuthorizationResource>();
            await foreach (var item in authorization.ListResourcesAutoPagingAsync(options))
            {
                items.Add(item);
            }

            Assert.Equal(2, items.Count);
            Assert.Equal(2, this.httpMock.CapturedRequests.Count);
            Assert.All(this.httpMock.CapturedRequests, request =>
            {
                var query = System.Web.HttpUtility.ParseQueryString(request.RequestUri.Query);
                Assert.Equal("authz_resource_parent", query["parent_resource_id"]);
            });

            var secondPage = System.Web.HttpUtility.ParseQueryString(this.httpMock.CapturedRequests[1].RequestUri.Query);
            Assert.Equal("cursor_abc", secondPage["after"]);
        }
    }
}
