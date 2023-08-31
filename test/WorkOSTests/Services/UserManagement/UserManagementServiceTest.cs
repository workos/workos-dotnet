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

        private readonly ListUsersOptions mockListUsersOptions;

        private readonly User mockUser;

        private readonly User mockUser2;

        private readonly SendMagicAuthCodeResponse mockSendMagicAuthCodeResponse;

        private readonly Session mockSession;

        private readonly Organization mockOrganization;

        private readonly Organization mockOrganization2;

        private readonly UnauthorizedOrganization mockUnauthorizedOrganization;

        private readonly AuthorizedOrganization mockAuthorizedOrganization;

        private readonly Reason mockReasons;

        private readonly string mockToken;

        private readonly SendVerificationEmailResponse mockSendVerificationEmailResponse;
        private readonly AuthenticateUserResponse mockAuthenticateUserResponse;

        private readonly VerifyEmailResponse mockVerifyEmailResponse;

        private readonly CreateUserOptions mockCreateUserOptions;

        private readonly AuthenticateUserWithPasswordOptions mockAuthenticateUserWithPasswordOptions;

        private readonly AuthenticateUserWithCodeOptions mockAuthenticateUserWithCodeOptions;

        private readonly AuthenticateUserWithMagicAuthOptions mockAuthenticateUserWithMagicAuthOptions;

        private readonly VerifyEmailOptions mockVerifyEmailOptions;

        private readonly SendMagicAuthCodeOptions mockSendMagicAuthCodeOptions;

        private readonly AddUserToOrganizationOptions mockAddUserToOrganizationOptions;

        private readonly CreatePasswordResetChallengeOptions mockCreatePasswordResetChallengeOptions;

        private readonly CompletePasswordResetOptions mockCompletePasswordResetOptions;

        private readonly UpdateUserOptions mockUpdateUserOptions;

        private readonly UpdateUserPasswordOptions mockUpdateUserPasswordOptions;

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
                Email = "marcelina.davis@gmail.com",
                FirstName = "Marcelina",
                LastName = "Davis",
                CreatedAt = "2021-06-25T19:07:33.155Z",
                UpdatedAt = "2021-08-27T19:07:33.155Z",
            };

            this.mockUser2 = new User
            {
                Id = "user_4DK2CR3C56J083X43JQXF3JK5",
                Email = "baron.bavis@gmail.com",
                FirstName = "Baron",
                LastName = "Bavis",
                CreatedAt = "2022-06-25T19:07:33.155Z",
                UpdatedAt = "2022-08-27T19:07:33.155Z",
            };

            this.mockSendMagicAuthCodeResponse = new SendMagicAuthCodeResponse
            {
                User = this.mockUser,
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

            this.mockToken = "token_1234";

            this.mockSendVerificationEmailResponse = new SendVerificationEmailResponse
            {
                User = this.mockUser,
            };

            this.mockAuthenticateUserResponse = new AuthenticateUserResponse
            {
                User = this.mockUser,
            };

            this.mockVerifyEmailResponse = new VerifyEmailResponse
            {
                User = this.mockUser,
            };

            this.mockListUsersOptions = new ListUsersOptions
            {
                Email = "marcelina.davis@gmail.com",
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
            };

            this.mockSendMagicAuthCodeOptions = new SendMagicAuthCodeOptions
            {
                Email = "marcelina.davis@gmail.com",
            };

            this.mockVerifyEmailOptions = new VerifyEmailOptions
            {
                UserId = this.mockUser.Id,
                Code = "code_1234",
            };
            this.mockAddUserToOrganizationOptions = new AddUserToOrganizationOptions
            {
                Organization = "org_1234",
            };

            this.mockCreatePasswordResetChallengeOptions = new CreatePasswordResetChallengeOptions
            {
                Email = "marcelina.davis@gmail.com",
                PasswordResetUrl = "password_1234",
            };

            this.mockCompletePasswordResetOptions = new CompletePasswordResetOptions
            {
                Token = "token_1234",
                NewPassword = "new_password_1234",
            };

            this.mockUpdateUserOptions = new UpdateUserOptions
            {
                Id = "user_1234",
                FirstName = "Marcelina",
                LastName = "Davis",
                Email = "marcelina.davis@gmail.com",
                IsEmailVerified = true,
            };

            this.mockUpdateUserPasswordOptions = new UpdateUserPasswordOptions
            {
                Id = "user_1234",
                Password = "password_1234",
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
            var email = user.Email;

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/users/{this.mockUser.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(user));
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
        public async void TestListUsers()
        {
            var mockResponse = new WorkOSList<User>
            {
                Data = new List<User>
                {
                    this.mockUser,
                    this.mockUser2,
                },
            };

            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/users",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));

            var response = await this.service.ListUsers(this.mockListUsersOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/users");

            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestAuthenticateUserWithPassword()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/authenticate",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockAuthenticateUserResponse));

            var response = await this.service.AuthenticateUserWithPassword(this.mockAuthenticateUserWithPasswordOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/authenticate");
            Assert.Equal(
                JsonConvert.SerializeObject(response),
                JsonConvert.SerializeObject(this.mockAuthenticateUserResponse));
        }

        [Fact]
        public async void TestAuthenticateUserWithCode()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/authenticate",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockAuthenticateUserResponse));
            var response = await this.service.AuthenticateUserWithCode(this.mockAuthenticateUserWithCodeOptions);
            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/authenticate");
            Assert.Equal(
                JsonConvert.SerializeObject(response),
                JsonConvert.SerializeObject(this.mockAuthenticateUserResponse));
        }

        [Fact]
        public async void TestAuthenticateUserWithMagicAuth()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/authenticate",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockAuthenticateUserResponse));

            var response = await this.service.AuthenticateUserWithMagicAuth(this.mockAuthenticateUserWithMagicAuthOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/authenticate");
            Assert.Equal(
                JsonConvert.SerializeObject(response),
                JsonConvert.SerializeObject(this.mockAuthenticateUserResponse));
        }

        [Fact]
        public async void TestSendMagicAuthCode()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/magic_auth/send",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockSendMagicAuthCodeResponse));

            var user = await this.service.SendMagicAuthCode(this.mockSendMagicAuthCodeOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/magic_auth/send");
            Assert.Equal(
                JsonConvert.SerializeObject(user),
                JsonConvert.SerializeObject(this.mockSendMagicAuthCodeResponse));
        }

        [Fact]
        public async void TestSendVerificationEmail()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/{this.mockUser.Id}/send_verification_email",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockSendVerificationEmailResponse));

            var response = await this.service.SendVerificationEmail(this.mockUser.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/{this.mockUser.Id}/send_verification_email");
            Assert.Equal(
                JsonConvert.SerializeObject(response),
                JsonConvert.SerializeObject(this.mockSendVerificationEmailResponse));
        }

        [Fact]
        public async void TestVerifyEmail()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/{this.mockUser.Id}/verify_email",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockUser));

            var response = await this.service.VerifyEmail(this.mockVerifyEmailOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/{this.mockUser.Id}/verify_email");
            Assert.Equal(
                JsonConvert.SerializeObject(response),
                JsonConvert.SerializeObject(this.mockVerifyEmailResponse));
        }

        [Fact]
        public async void TestCreatePasswordResetChallenge()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/password_reset_challenge",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString((this.mockToken, this.mockUser)));

            var (token, user) = await this.service.CreatePasswordResetChallenge(this.mockCreatePasswordResetChallengeOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/password_reset_challenge");
            Assert.Equal(
                JsonConvert.SerializeObject(token),
                JsonConvert.SerializeObject(this.mockToken));
        }

        [Fact]
        public async void TestCompletePasswordReset()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/password_reset",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockUser));

            var user = await this.service.CompletePasswordReset(this.mockCompletePasswordResetOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/password_reset");
            Assert.Equal(
                JsonConvert.SerializeObject(user),
                JsonConvert.SerializeObject(this.mockUser));
        }

        [Fact]
        public async void TestAddUserToOrganization()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/users/{this.mockUser.Id}/organizations",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockUser));

            var user = await this.service.AddUserToOrganziation(this.mockUser.Id, this.mockAddUserToOrganizationOptions);
            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/users/{this.mockUser.Id}/organizations");
            Assert.Equal(
                JsonConvert.SerializeObject(user),
                JsonConvert.SerializeObject(this.mockUser));
        }

        [Fact]
        public async void TestRemoveUserFromOrganization()
        {
            this.httpMock.MockResponse(
                HttpMethod.Delete,
                $"/users/{this.mockUser.Id}/{this.mockOrganization2.Id}",
                HttpStatusCode.Accepted,
                "Accepted");

            await this.service.RemoveUserFromOrganziation(this.mockUser.Id, this.mockOrganization2.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Delete,
                $"/users/{this.mockUser.Id}/{this.mockOrganization2.Id}");
        }

        [Fact]
        public async void TestUpdateUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Put,
                $"/users/{this.mockUpdateUserOptions.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));

            var user = await this.service.UpdateUser(this.mockUpdateUserOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Put,
                $"/users/{this.mockUpdateUserOptions.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(user),
                JsonConvert.SerializeObject(this.mockUser));
        }

        [Fact]
        public async void TestUpdateUserPassword()
        {
            this.httpMock.MockResponse(
                HttpMethod.Put,
                $"/users/{this.mockUpdateUserPasswordOptions.Id}/password",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));

            var user = await this.service.UpdateUserPassword(this.mockUpdateUserPasswordOptions);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Put,
                $"/users/{this.mockUpdateUserPasswordOptions.Id}/password");
            Assert.Equal(
                JsonConvert.SerializeObject(user),
                JsonConvert.SerializeObject(this.mockUser));
        }

        [Fact]
        public async void TestDeleteUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Delete,
                $"/users/{this.mockUser.Id}",
                HttpStatusCode.Accepted,
                "Accepted");

            await this.service.DeleteUser(this.mockUser.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Delete,
                $"/users/{this.mockUser.Id}");
        }
    }
}
