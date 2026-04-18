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
        private readonly OrganizationsService service;

        public PaginationTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = this.httpMock.HttpClient,
                MaxRetries = 0,
            });
            this.service = new OrganizationsService(client);
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
    }
}
