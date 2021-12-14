namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class SSOServiceTest
    {
        private readonly HttpMock httpMock;

        private readonly SSOService service;

        private readonly ListConnectionsOptions listConnectionsOptions;

        private readonly Connection mockConnection;

        public SSOServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });

            this.service = new SSOService(client);

            this.listConnectionsOptions = new ListConnectionsOptions
            {
                Domain = "foo-corp.com",
            };

            this.mockConnection = new Connection
            {
                Id = "connection_id",
                Name = "Foo Corp",
                State = ConnectionState.Active,
                ConnectionType = ConnectionType.OktaSAML,
                CreatedAt = "2021-07-26T18:55:16.072Z",
                UpdatedAt = "2021-07-26T18:55:16.072Z",
            };
        }

        [Fact]
        public void TestGetAuthorizationURLWithNoConnectionDomainOrProvider()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "client_123",
                RedirectURI = "https://example.com/sso/callback",
            };
            var exception = Assert.Throws<ArgumentNullException>(() =>
                this.service.GetAuthorizationURL(options));
        }

        [Fact]
        public void TestGetAuthorizationURLWithProvider()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "client_123",
                Provider = ConnectionType.GoogleOAuth,
                RedirectURI = "https://example.com/sso/callback",
            };
            string url = this.service.GetAuthorizationURL(options);

            Dictionary<string, string> parameters = RequestUtilities.ParseURLParameters(url);
            Assert.Equal(options.ClientId, parameters["client_id"]);
            Assert.Equal(options.Provider.ToString(), parameters["provider"]);
            Assert.Equal(options.RedirectURI, parameters["redirect_uri"]);
            Assert.Equal("code", parameters["response_type"]);
        }

        [Fact]
        public void TestGetAuthorizationURLWithDomain()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "client_123",
#pragma warning disable CS0618 // GetAuthorizationURLOptions.Domain' is obsolete: 'The Domain property is deprecated. Please use Organization instead.
                Domain = "foo-corp.com",
#pragma warning restore CS0618 // GetAuthorizationURLOptions.Domain' is obsolete: 'The Domain property is deprecated. Please use Organization instead.
                RedirectURI = "https://example.com/sso/callback",
            };
            string url = this.service.GetAuthorizationURL(options);

            Dictionary<string, string> parameters = RequestUtilities.ParseURLParameters(url);
            Assert.Equal(options.ClientId, parameters["client_id"]);
#pragma warning disable CS0618 // GetAuthorizationURLOptions.Domain' is obsolete: 'The Domain property is deprecated. Please use Organization instead.
            Assert.Equal(options.Domain, parameters["domain"]);
#pragma warning restore CS0618 // GetAuthorizationURLOptions.Domain' is obsolete: 'The Domain property is deprecated. Please use Organization instead.
            Assert.Equal(options.RedirectURI, parameters["redirect_uri"]);
            Assert.Equal("code", parameters["response_type"]);
        }

        [Fact]
        public void TestGetAuthorizationURLWithConnection()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "client_123",
                Connection = "connection_123",
                RedirectURI = "https://example.com/sso/callback",
            };
            string url = this.service.GetAuthorizationURL(options);

            Dictionary<string, string> parameters = RequestUtilities.ParseURLParameters(url);
            Assert.Equal(options.ClientId, parameters["client_id"]);
            Assert.Equal(options.Connection, parameters["connection"]);
            Assert.Equal(options.RedirectURI, parameters["redirect_uri"]);
            Assert.Equal("code", parameters["response_type"]);
        }

        [Fact]
        public void TestGetAuthorizationURLWithOrganization()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "client_123",
                Organization = "organization_123",
                RedirectURI = "https://example.com/sso/callback",
            };
            string url = this.service.GetAuthorizationURL(options);

            Dictionary<string, string> parameters = RequestUtilities.ParseURLParameters(url);
            Assert.Equal(options.ClientId, parameters["client_id"]);
            Assert.Equal(options.Organization, parameters["organization"]);
            Assert.Equal(options.RedirectURI, parameters["redirect_uri"]);
            Assert.Equal("code", parameters["response_type"]);
        }

        [Fact]
        public void TestGetAuthorizationURLWithState()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "client_123",
                Organization = "organization_123",
                RedirectURI = "https://example.com/sso/callback",
                State = "state",
            };
            string url = this.service.GetAuthorizationURL(options);

            Dictionary<string, string> parameters = RequestUtilities.ParseURLParameters(url);
            Assert.Equal(options.ClientId, parameters["client_id"]);
            Assert.Equal(options.Organization, parameters["organization"]);
            Assert.Equal(options.RedirectURI, parameters["redirect_uri"]);
            Assert.Equal(options.State, parameters["state"]);
            Assert.Equal("code", parameters["response_type"]);
        }

        [Fact]
        public async void TestGetProfileAndToken()
        {
            var mockProfile = new Profile
            {
                Id = "profile_0",
                IdpId = "123",
                OrganizationId = "org_123",
                ConnectionId = "conn_123",
                ConnectionType = ConnectionType.OktaSAML,
                Email = "rick@sanchez.com",
                FirstName = "Rick",
                LastName = "Sanchez",
                RawAttributes = new Dictionary<string, object>()
                {
                    { "idp_id", "123" },
                    { "email", "rick@sanchez.com" },
                    { "first_name", "Rick" },
                    { "last_name", "Sanchez" },
                },
            };
            var profileAndTokenResponse = new GetProfileAndTokenResponse
            {
                AccessToken = "token",
                Profile = mockProfile,
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/sso/token",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(profileAndTokenResponse));

            var options = new GetProfileAndTokenOptions
            {
                ClientId = "client_123",
                Code = "code",
            };
            var response = await this.service.GetProfileAndToken(options);
            var profile = response.Profile;

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/sso/token");
            Assert.NotNull(profile);
            Assert.Equal(
                JsonConvert.SerializeObject(mockProfile),
                JsonConvert.SerializeObject(profile));
        }

        [Fact]
        public async void TestGetProfile()
        {
            var mockProfile = new Profile
            {
                Id = "profile_0",
                IdpId = "123",
                OrganizationId = "org_123",
                ConnectionId = "conn_123",
                ConnectionType = ConnectionType.OktaSAML,
                Email = "rick@sanchez.com",
                FirstName = "Rick",
                LastName = "Sanchez",
                RawAttributes = new Dictionary<string, object>()
                {
                    { "idp_id", "123" },
                    { "email", "rick@sanchez.com" },
                    { "first_name", "Rick" },
                    { "last_name", "Sanchez" },
                },
            };

            this.httpMock.MockResponseWithAuthorizationHeader(
                HttpMethod.Get,
                "/sso/profile",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockProfile),
                "access_token");

            var options = new GetProfileOptions
            {
                AccessToken = "access_token",
            };
            var profile = await this.service.GetProfile(options);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/sso/profile");
            this.httpMock.AssertAuthorizationBearerHeader("access_token");
        }

        [Fact]
        public async void TestListConnections()
        {
            var mockResponse = new WorkOSList<Connection>
            {
                Data = new List<Connection>
                {
                    this.mockConnection,
                },
            };
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/connections",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));

            var response = await this.service.ListConnections(this.listConnectionsOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/connections");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetConnection()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/connections/{this.mockConnection.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockConnection));

            var response = await this.service.GetConnection(this.mockConnection.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/connections/{this.mockConnection.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockConnection),
                JsonConvert.SerializeObject(response));
        }
    }
}
