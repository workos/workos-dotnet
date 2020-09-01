namespace WorkOSTests.Services.SSO
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Xunit;

    using WorkOS;

    public class SSOServiceTest
    {
        private readonly HttpMock httpMock;

        private readonly SSOService service;

        public SSOServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                }
            );

            this.service = new SSOService(client);
        }

        [Fact]
        public void TestGetAuthorizationURLWithNoDomainOrProvider()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "project_123",
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
                ClientId = "project_123",
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
                ClientId = "project_123",
                Domain = "foo-corp.com",
                RedirectURI = "https://example.com/sso/callback",
            };
            string url = this.service.GetAuthorizationURL(options);

            Dictionary<string, string> parameters = RequestUtilities.ParseURLParameters(url);
            Assert.Equal(options.ClientId, parameters["client_id"]);
            Assert.Equal(options.Domain, parameters["domain"]);
            Assert.Equal(options.RedirectURI, parameters["redirect_uri"]);
            Assert.Equal("code", parameters["response_type"]);
        }

        [Fact]
        public void TestGetAuthorizationURLWithState()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "project_123",
                Domain = "foo-corp.com",
                RedirectURI = "https://example.com/sso/callback",
                State = "state",
            };
            string url = this.service.GetAuthorizationURL(options);

            Dictionary<string, string> parameters = RequestUtilities.ParseURLParameters(url);
            Assert.Equal(options.ClientId, parameters["client_id"]);
            Assert.Equal(options.Domain, parameters["domain"]);
            Assert.Equal(options.RedirectURI, parameters["redirect_uri"]);
            Assert.Equal(options.State, parameters["state"]);
            Assert.Equal("code", parameters["response_type"]);
        }

        [Fact]
        public void TestGetProfile()
        {
            var mockProfile = new Profile
            {
                Id = "profile_0",
                IdpId = "123",
                ConnectionType = ConnectionType.OktaSAML,
                Email = "rick@sanchez.com",
                FirstName = "Rick",
                LastName = "Sanchez",
                RawAttributes = new Dictionary<string, string>()
                {
                    { "idp_id", "123" },
                    { "email", "rick@sanchez.com" },
                    { "first_name", "Rick" },
                    { "last_name", "Sanchez" },
                },
            };
            var profileResponse = new GetProfileResponse
            {
                AccessToken = "token",
                Profile = mockProfile,
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/sso/token",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(profileResponse));

            var options = new GetProfileOptions
            {
                ClientId = "project_123",
                Code = "code",
            };
            var response = this.service.GetProfile(options);
            var profile = response.Profile;

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/sso/token");
            Assert.NotNull(profile);
            Assert.Equal(mockProfile, profile);
        }

        [Fact]
        public async void TestGetProfileAsync()
        {
            var mockProfile = new Profile
            {
                Id = "profile_0",
                IdpId = "123",
                ConnectionType = ConnectionType.OktaSAML,
                Email = "rick@sanchez.com",
                FirstName = "Rick",
                LastName = "Sanchez",
                RawAttributes = new Dictionary<string, string>()
                {
                    { "idp_id", "123" },
                    { "email", "rick@sanchez.com" },
                    { "first_name", "Rick" },
                    { "last_name", "Sanchez" },
                },
            };
            var profileResponse = new GetProfileResponse
            {
                AccessToken = "token",
                Profile = mockProfile,
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/sso/token",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(profileResponse));

            var options = new GetProfileOptions
            {
                ClientId = "project_123",
                Code = "code",
            };
            var response = await this.service.GetProfileAsync(options);
            var profile = response.Profile;

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/sso/token");
            Assert.NotNull(profile);
            Assert.Equal(mockProfile, profile);
        }

        [Fact]
        public void TestCreateConnection()
        {
            var mockConnection = new Connection
            {
                Id = "connection_id",
                Name = "Terrace House",
                Status = ConnectionStatus.Linked,
                ConnectionType = ConnectionType.OktaSAML,
                OAuthUid = "",
                OAuthSecret = "",
                OAuthRedirectUri = "",
                SamlEntityId = "http://www.okta.com/terrace-house",
                SamlIdpUrl = "https://terrace-house.okta.com/app/terrace/house/saml",
                SamlRelyingPartyTrustCert = "",
                SamlX509Certs = new string[] {
                    "-----BEGIN CERTIFICATE----------END CERTIFICATE-----"
                },
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/connections",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(mockConnection));

            var options = new CreateConnectionOptions
            {
                Source = "source",
            };
            var connection = this.service.CreateConnection(options);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/connections");
            Assert.NotNull(connection);
            Assert.Equal(mockConnection, connection);
        }

        [Fact]
        public async void TestCreateConnectionAsync()
        {
            var mockConnection = new Connection
            {
                Id = "connection_id",
                Name = "Terrace House",
                Status = ConnectionStatus.Linked,
                ConnectionType = ConnectionType.OktaSAML,
                OAuthUid = "",
                OAuthSecret = "",
                OAuthRedirectUri = "",
                SamlEntityId = "http://www.okta.com/terrace-house",
                SamlIdpUrl = "https://terrace-house.okta.com/app/terrace/house/saml",
                SamlRelyingPartyTrustCert = "",
                SamlX509Certs = new string[] {
                    "-----BEGIN CERTIFICATE----------END CERTIFICATE-----"
                },
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/connections",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(mockConnection));

            var options = new CreateConnectionOptions
            {
                Source = "source",
            };
            var connection = await this.service.CreateConnectionAsync(options);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/connections");
            Assert.NotNull(connection);
            Assert.Equal(mockConnection, connection);
        }
    }
}
