namespace WorkOSTests
{
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class UserManagementServiceTest
    {
        private readonly HttpMock httpMock;

        private readonly UserManagementService service;

        private readonly User mockUser;

        private readonly Session mockSession;

        private readonly Organization mockOrganization;

        private readonly Organization mockOrganization2;

        private readonly UnauthorizedOrganizations mockUnauthorizedOrganization;

        private readonly AuthorizedOrganizations mockAuthorizedOrganization;

        private readonly Reasons mockReasons;

        private readonly CreateUserOptions mockCreateUserOptions;

        private readonly AuthenticateUserWithPasswordOptions mockAuthenticateUserWithPasswordOptions;

        private readonly AuthenticateUserWithTokenOptions mockAuthenticateUserWithTokenOptions;

        public UserManagementServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new UserManagementService(client);

            this.mockUser = new User
            {
                Id = "user_01E4ZCR3C56J083X43JQXF3JK5",
                UserType = UserType.Unmanaged,
                Email = "marcelina.davis@gmail.com",
                FirstName = "Marcelina",
                LastName = "Davis",
                EmailVerifiedAt = "2021-07-25T19:07:33.155Z",
                CreatedAt = "2021-06-25T19:07:33.155Z",
                UpdatedAt = "2021-08-27T19:07:33.155Z",
            };

            this.mockOrganization = new Organization
            {
                Id = "org_01EHZNVPK3SFK441A1RGBFSHRT",
                Name = "AuthorizedOrgExample",
                AllowProfilesOutsideOrganization = false,
                CreatedAt = "2021-06-25T19:07:33.155Z",
                UpdatedAt = "2021-06-25T19:07:33.155Z",
                Domains = new OrganizationDomain[]
                {
                    new OrganizationDomain
                    {
                        Id = "org_domain_123",
                        Domain = "foo-corp.com",
                    },
                },
            };

            this.mockOrganization2 = new Organization
            {
                Id = "org_12AVDCSEW4DBN552B2ZHLGRWMO",
                Name = "UnauthorizedOrgExample",
                AllowProfilesOutsideOrganization = false,
                CreatedAt = "2021-06-25T19:07:33.155Z",
                UpdatedAt = "2021-06-25T19:07:33.155Z",
                Domains = new OrganizationDomain[]
                {
                    new OrganizationDomain
                    {
                        Id = "org_domain_123",
                        Domain = "foo-corp.com",
                    },
                },
            };

            this.mockReasons = new Reasons
            {
                AllowedAuthenticationMethods = new List<AuthenticationMethods>
                {
                    AuthenticationMethods.AuthenticationMethodRequired,
                    AuthenticationMethods.Password,
                    AuthenticationMethods.MagicAuth,
                    AuthenticationMethods.MicrosoftOauth,
                },
            };

            this.mockAuthorizedOrganization = new AuthorizedOrganizations
            {
                Organization = this.mockOrganization,
            };

            this.mockUnauthorizedOrganization = new UnauthorizedOrganizations
            {
                Organization = this.mockOrganization2,
                Reasons = new Reasons[]
                {
                    this.mockReasons,
                },
            };

            this.mockSession = new Session
            {
                Id = "session_01E4ZCR3C56J083X43JQXF3JK5",
                CreatedAt = "2021-06-25T19:07:33.155Z",
                ExpiresAt = "2022-06-25T19:07:33.155Z",
                Token = "session_token_123abc",
                AuthorizedOrganizations = new AuthorizedOrganizations[]
                    {
                        this.mockAuthorizedOrganization,
                    },
                UnauthorizedOrganizations = new UnauthorizedOrganizations[]
                    {
                        this.mockUnauthorizedOrganization,
                    },
            };

            this.mockCreateUserOptions = new CreateUserOptions
            {
                Email = "marcelina.davis@gmail.com",
                Password = "pass_123",
                FirstName = "Marcelina",
                LastName = "Davis",
                IsEmailVerified = true,
            };

            this.mockAuthenticateUserWithPasswordOptions = new AuthenticateUserWithPasswordOptions
            {
                ClientId = "client_123",
                ClientSecret = "client_secret_123",
                Email = "marcelina.davis@gmail.com",
                Password = "password_123",
            };

            this.mockAuthenticateUserWithTokenOptions = new AuthenticateUserWithTokenOptions
            {
                ClientId = "client_123",
                ClientSecret = "client_secret_123",
                Code = "code_123",
            };
        }

        [Fact]
        public async void TestGetUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/users/{this.mockUser.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));

            var user = await this.service.GetUser(this.mockUser.Id);
            var type = user.UserType;
            var email = user.Email;

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/users/{this.mockUser.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser.UserType),
                JsonConvert.SerializeObject(type));
        }

        [Fact]
        public async void TestCreateUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockUser));

            var user = await this.service.CreateUser(this.mockCreateUserOptions);
            var email = user.Email;

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockCreateUserOptions.Email),
                JsonConvert.SerializeObject(email));
        }

        [Fact]
        public async void TestAuthenticateUserWithPassword()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/sessions/token",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString((this.mockUser, this.mockSession)));

            var (user, session) = await this.service.AuthenticateUserWithPassword(this.mockAuthenticateUserWithPasswordOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/sessions/token");
            Assert.Equal(
                JsonConvert.SerializeObject(session),
                JsonConvert.SerializeObject(this.mockSession));
        }

        [Fact]
        public async void TestAuthenticateUserWithToken()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/sessions/token",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString((this.mockUser, this.mockSession)));

            var (user, session) = await this.service.AuthenticateUserWithToken(this.mockAuthenticateUserWithTokenOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/sessions/token");
            Assert.Equal(
                JsonConvert.SerializeObject(session),
                JsonConvert.SerializeObject(this.mockSession));
        }
    }
}
