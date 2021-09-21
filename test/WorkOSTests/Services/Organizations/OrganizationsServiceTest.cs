namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class OrganizationsServiceTest
    {
        private readonly HttpMock httpMock;

        private readonly OrganizationsService service;

        private readonly CreateOrganizationOptions createOrganizationOptions;

        private readonly ListOrganizationsOptions listOrganizationsOptions;

        private readonly UpdateOrganizationOptions updateOrganizationOptions;

        private readonly Organization mockOrganization;

        public OrganizationsServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new OrganizationsService(client);

            this.createOrganizationOptions = new CreateOrganizationOptions
            {
                Name = "Foo Corp",
                Domains = new string[]
                {
                    "foo-corp.com",
                },
            };

            this.updateOrganizationOptions = new UpdateOrganizationOptions
            {
                Organization = "org_123",
                Domains = new string[]
                {
                    "foo-corp.com",
                },
                Name = "Foo Corp 2",
            };

            this.listOrganizationsOptions = new ListOrganizationsOptions
            {
                Domains = new string[]
                {
                    "foo-corp.com",
                },
            };

            this.mockOrganization = new Organization
            {
                Id = "org_123",
                Name = "Foo Corp",
                AllowProfilesOutsideOrganization = false,
                Domains = new OrganizationDomain[]
                {
                    new OrganizationDomain
                    {
                        Id = "org_domain_123",
                        Domain = "foo-corp.com",
                    },
                },
                CreatedAt = "2021-07-26T18:55:16.072Z",
                UpdatedAt = "2021-07-26T18:55:16.072Z",
            };
        }

        [Fact]
        public async void TestListOrganizations()
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
            var response = await this.service.ListOrganizations(this.listOrganizationsOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/organizations");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestCreateOrganization()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/organizations",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockOrganization));
            var response = await this.service.CreateOrganization(this.createOrganizationOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/organizations");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganization),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetOrganization()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/organizations/{this.mockOrganization.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockOrganization));

            var response = await this.service.GetOrganization(this.mockOrganization.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/organizations/{this.mockOrganization.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganization),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestDeleteOrganization()
        {
            this.httpMock.MockResponse(
                HttpMethod.Delete,
                $"/organizations/{this.mockOrganization.Id}",
                HttpStatusCode.Accepted,
                "Accepted");

            await this.service.DeleteOrganization(this.mockOrganization.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Delete,
                $"/organizations/{this.mockOrganization.Id}");
        }

        [Fact]
        public async void TestUpdateOrganization()
        {
            this.httpMock.MockResponse(
                HttpMethod.Put,
                $"/organizations/{this.updateOrganizationOptions.Organization}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockOrganization));
            var response = await this.service.UpdateOrganization(this.updateOrganizationOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Put, $"/organizations/{this.updateOrganizationOptions.Organization}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganization),
                JsonConvert.SerializeObject(response));
        }
    }
}
