namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class PortalServiceTest
    {
        private readonly HttpMock httpMock;

        private readonly PortalService service;

        private readonly GenerateLinkOptions generateLinkOptionsSSO;

        private readonly GenerateLinkOptions generateLinkOptionsDSync;

        private readonly GenerateLinkOptions generateLinkOptionsAuditLogs;

        private readonly GenerateLinkOptions generateLinkOptionsLogStreams;

        private readonly GenerateLinkResponse mockGenerateLinkResponse;

        public PortalServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new PortalService(client);

            this.generateLinkOptionsSSO = new GenerateLinkOptions
            {
                Intent = Intent.SSO,
                Organization = "org_123",
                ReturnURL = "https://foo-corp.app.com/settings",
            };

            this.generateLinkOptionsDSync = new GenerateLinkOptions
            {
                Intent = Intent.DSync,
                Organization = "org_123",
                ReturnURL = "https://foo-corp.app.com/settings",
            };

            this.generateLinkOptionsAuditLogs = new GenerateLinkOptions
            {
                Intent = Intent.AuditLogs,
                Organization = "org_123",
                ReturnURL = "https://foo-corp.app.com/settings",
            };

            this.generateLinkOptionsLogStreams = new GenerateLinkOptions
            {
                Intent = Intent.LogStreams,
                Organization = "org_123",
                ReturnURL = "https://foo-corp.app.com/settings",
            };

            this.mockGenerateLinkResponse = new GenerateLinkResponse
            {
                Link = "https://id.workos.test/portal/launch?secret=1234",
            };
        }

        [Fact]
        public async void TestGenerateLinkSSO()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/portal/generate_link",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockGenerateLinkResponse));
            var link = await this.service.GenerateLink(this.generateLinkOptionsSSO);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/portal/generate_link");
            Assert.Equal(this.mockGenerateLinkResponse.Link, link);
        }

        [Fact]
        public async void TestGenerateLinkDSync()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/portal/generate_link",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockGenerateLinkResponse));
            var link = await this.service.GenerateLink(this.generateLinkOptionsDSync);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/portal/generate_link");
            Assert.Equal(this.mockGenerateLinkResponse.Link, link);
        }

        [Fact]
        public async void TestGenerateLinkAuditLogs()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/portal/generate_link",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockGenerateLinkResponse));
            var link = await this.service.GenerateLink(this.generateLinkOptionsAuditLogs);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/portal/generate_link");
            Assert.Equal(this.mockGenerateLinkResponse.Link, link);
        }

        [Fact]
        public async void TestGenerateLinkLogStreams()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/portal/generate_link",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockGenerateLinkResponse));
            var link = await this.service.GenerateLink(this.generateLinkOptionsLogStreams);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/portal/generate_link");
            Assert.Equal(this.mockGenerateLinkResponse.Link, link);
        }
    }
}
