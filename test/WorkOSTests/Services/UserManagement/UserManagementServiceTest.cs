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

        private readonly UnauthorizedOrganization mockUnauthorizedOrganization;

        private readonly AuthorizedOrganization mockAuthorizedOrganization;

        private readonly Reason mockReasons;

        private readonly CreateUserOptions mockCreateUserOptions;

        private readonly AuthenticateUserWithPasswordOptions mockAuthenticateUserWithPasswordOptions;

<<<<<<< HEAD
        private readonly AuthenticateUserWithCodeOptions mockAuthenticateUserWithCodeOptions;

        private readonly AuthenticateUserWithMagicAuthOptions mockAuthenticateUserWithMagicAuthOptions;
=======
        private readonly AuthenticateUserWithMagicAuthOptions mockAuthenticateUserWithMagicAuthOptions;

        private readonly AuthenticateUserWithCodeOptions mockAuthenticateUserWithCodeOptions;
>>>>>>> cd076e2c1c669939f1db6d139c6d4c70ccf2e1b0

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

            this.mockReasons = new Reason
            {
                AllowedAuthenticationMethods = new List<AuthenticationMethod>
                {
                    AuthenticationMethod.AuthenticationMethodRequired,
                    AuthenticationMethod.Password,
                    AuthenticationMethod.MagicAuth,
                    AuthenticationMethod.MicrosoftOauth,
                },
            };

            this.mockAuthorizedOrganization = new AuthorizedOrganization
            {
                Organization = this.mockOrganization,
            };

            this.mockUnauthorizedOrganization = new UnauthorizedOrganization
            {
                Organization = this.mockOrganization2,
                Reasons = new List<Reason>
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
                AuthorizedOrganizations = new List<AuthorizedOrganization>
                    {
                        this.mockAuthorizedOrganization,
                    },
                UnauthorizedOrganizations = new List<UnauthorizedOrganization>
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

            this.mockAuthenticateUserWithCodeOptions = new AuthenticateUserWithCodeOptions
            {
                ClientId = "client_123",
                ClientSecret = "client_secret_123",
                Code = "code_123",
            };

            this.mockAuthenticateUserWithMagicAuthOptions = new AuthenticateUserWithMagicAuthOptions
            {
                ClientId = "client_123",
                ClientSecret = "client_secret_123",
                Code = "code_123",
                MagicAuthChallengeId = "auth_challenge_123",
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
        public async void TestAuthenticateUserWithCode()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/sessions/token",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString((this.mockUser, this.mockSession)));

            var (user, session) = await this.service.AuthenticateUserWithCode(this.mockAuthenticateUserWithCodeOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/sessions/token");
            Assert.Equal(
                JsonConvert.SerializeObject(session),
                JsonConvert.SerializeObject(this.mockSession));
        }

        [Fact]
        public async void TestAuthenticateUserWithMagicAuth()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/sessions/token",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString((this.mockUser, this.mockSession)));

            var (user, session) = await this.service.AuthenticateUserWithMagicAuth(this.mockAuthenticateUserWithMagicAuthOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/sessions/token");
            Assert.Equal(
                JsonConvert.SerializeObject(session),
                JsonConvert.SerializeObject(this.mockSession));
        }
    }
}
