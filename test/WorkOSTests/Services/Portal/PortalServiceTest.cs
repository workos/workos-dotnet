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

        private readonly CreateOrganizationOptions createOrganizationOptions;

        private readonly ListOrganizationsOptions listOrganizationsOptions;

        private readonly GenerateLinkOptionsSSO generateLinkOptionsSSO;

        private readonly GenerateLinkOptionsDSync generateLinkOptionsDSync;

        private readonly GenerateLinkResponse mockGenerateLinkResponse;

        private readonly Organization mockOrganization;

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

            this.createOrganizationOptions = new CreateOrganizationOptions
            {
                Name = "Foo Corp",
                Domains = new string[]
                {
                    "foo-corp.com",
                },
            };

            this.generateLinkOptionsSSO = new GenerateLinkOptionsSSO
            {
                Intent = Intent.SSO,
                Organization = "org_123",
                ReturnURL = "https://foo-corp.app.com/settings",
            };

            this.generateLinkOptionsDSync = new GenerateLinkOptionsDSync
            {
                Intent = Intent.DSync,
                Organization = "org_123",
                ReturnURL = "https://foo-corp.app.com/settings",
            };

            this.listOrganizationsOptions = new ListOrganizationsOptions
            {
                Domains = new string[]
                {
                    "foo-corp.com",
                },
            };

            this.mockGenerateLinkResponse = new GenerateLinkResponse
            {
                Link = "https://id.workos.test/portal/launch?secret=1234",
            };

            this.mockOrganization = new Organization
            {
                Id = "org_123",
                Name = "Foo Corp",
                Domains = new OrganizationDomain[]
                {
                    new OrganizationDomain
                    {
                        Id = "org_domain_123",
                        Domain = "foo-corp.com",
                    },
                },
            };
        }

        [Fact]
        public void TestListOrganizations()
        {
            var mockResponse = new WorkOSList<Organization>
            {
                Data = new List<Organization>
                {
                    this.mockOrganization,
                },
            };
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/organizations",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));
            var response = this.service.ListOrganizations(this.listOrganizationsOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/organizations");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestListOrganizationsAsync()
        {
            var mockResponse = new WorkOSList<Organization>
            {
                Data = new List<Organization>
                {
                    this.mockOrganization,
                },
            };
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/organizations",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));
            var response = await this.service.ListOrganizationsAsync(this.listOrganizationsOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/organizations");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public void TestCreateOrganization()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/organizations",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockOrganization));
            var response = this.service.CreateOrganization(this.createOrganizationOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/organizations");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganization),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestCreateOrganizationAsync()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/organizations",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockOrganization));
            var response = await this.service.CreateOrganizationAsync(this.createOrganizationOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/organizations");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganization),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public void TestGenerateLinkSSO()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/portal/generate_link",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockGenerateLinkResponse));
            var link = this.service.GenerateLink(this.generateLinkOptionsSSO);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/portal/generate_link");
            Assert.Equal(this.mockGenerateLinkResponse.Link, link);
        }

        [Fact]
        public void TestGenerateLinkDSync()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/portal/generate_link",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockGenerateLinkResponse));
            var link = this.service.GenerateLink(this.generateLinkOptionsDSync);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/portal/generate_link");
            Assert.Equal(this.mockGenerateLinkResponse.Link, link);
        }

        [Fact]
        public async void TestGenerateLinkAsyncSSO()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/portal/generate_link",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockGenerateLinkResponse));
            var link = await this.service.GenerateLinkAsync(this.generateLinkOptionsSSO);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/portal/generate_link");
            Assert.Equal(this.mockGenerateLinkResponse.Link, link);
        }

        [Fact]
        public async void TestGenerateLinkAsyncDSync()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/portal/generate_link",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockGenerateLinkResponse));
            var link = await this.service.GenerateLinkAsync(this.generateLinkOptionsDSync);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/portal/generate_link");
            Assert.Equal(this.mockGenerateLinkResponse.Link, link);
        }
    }
}
