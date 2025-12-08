namespace WorkOSTests
{
    using System;
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

        private readonly CreateUserOptions createUserOptions;

        private readonly ListUsersOptions listUsersOptions;

        private readonly UpdateUserOptions updateUserOptions;

        private readonly User mockUser;

        private readonly CreatePasswordResetOptions createPasswordResetOptions;

        private readonly ResetPasswordOptions resetPasswordOptions;

        private readonly PasswordReset mockPasswordReset;

        private readonly CreateOrganizationMembershipOptions createOrganizationMembershipOptions;

        private readonly UpdateOrganizationMembershipOptions updateOrganizationMembershipOptions;

        private readonly ListOrganizationMembershipsOptions listOrganizationMembershipsOptions;

        private readonly OrganizationMembership mockOrganizationMembership;

        private readonly AuthenticateWithCodeOptions authenticateWithCodeOptions;

        private readonly AuthenticationResponse mockAuthenticationResponse;

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

            this.createUserOptions = new CreateUserOptions
            {
                Email = "user@example.com",
                FirstName = "John",
                LastName = "Doe",
                Password = "SecurePassword123",
                EmailVerified = true,
            };

            this.updateUserOptions = new UpdateUserOptions
            {
                Id = "user_123",
                FirstName = "Jane",
                LastName = "Smith",
                EmailVerified = false,
            };

            this.listUsersOptions = new ListUsersOptions
            {
                Email = "user@example.com",
            };

            this.createPasswordResetOptions = new CreatePasswordResetOptions
            {
                Email = "user@example.com",
            };

            this.resetPasswordOptions = new ResetPasswordOptions
            {
                Token = "test_reset_token_123",
                NewPassword = "NewSecurePassword456",
            };

            this.mockUser = new User
            {
                Id = "user_123",
                Email = "user@example.com",
                FirstName = "John",
                LastName = "Doe",
                EmailVerified = true,
                ProfilePictureUrl = "https://example.com/avatar.jpg",
                ExternalId = "ext_123",
                Locale = "en-US",
                CreatedAt = "2021-07-26T18:55:16.072Z",
                UpdatedAt = "2021-07-26T18:55:16.072Z",
            };

            this.mockPasswordReset = new PasswordReset
            {
                Id = "password_reset_123",
                UserId = "user_123",
                Email = "user@example.com",
                PasswordResetToken = "Z1uX3RbwcIl5fIGJJJCXXisdI",
                PasswordResetUrl = "https://your-app.com/reset-password?token=Z1uX3RbwcIl5fIGJJJCXXisdI",
                ExpiresAt = "2025-07-14T18:00:54.578Z",
                CreatedAt = "2025-07-14T17:45:54.578Z",
            };

            this.createOrganizationMembershipOptions = new CreateOrganizationMembershipOptions
            {
                UserId = "user_123",
                OrganizationId = "org_123",
                RoleSlug = "admin",
            };

            this.updateOrganizationMembershipOptions = new UpdateOrganizationMembershipOptions
            {
                Id = "om_123",
                RoleSlug = "member",
            };

            this.listOrganizationMembershipsOptions = new ListOrganizationMembershipsOptions
            {
                OrganizationId = "org_123",
            };

            this.mockOrganizationMembership = new OrganizationMembership
            {
                Id = "om_123",
                UserId = "user_123",
                OrganizationId = "org_123",
                OrganizationName = "Acme Corp",
                Role = new OrganizationMembershipRole { Slug = "admin" },
                Roles = new List<OrganizationMembershipRole> { new OrganizationMembershipRole { Slug = "admin" } },
                Status = "active",
                CreatedAt = "2021-07-26T18:55:16.072Z",
                UpdatedAt = "2021-07-26T18:55:16.072Z",
            };

            this.authenticateWithCodeOptions = new AuthenticateWithCodeOptions
            {
                Code = "auth_code_123",
                ClientId = "client_123",
                ClientSecret = "client_secret_123",
            };

            this.mockAuthenticationResponse = new AuthenticationResponse
            {
                User = this.mockUser,
                OrganizationId = "org_123",
                AccessToken = "access_token_abc123",
                RefreshToken = "refresh_token_def456",
                AuthenticationMethod = AuthenticationMethod.Password,
                OAuthTokens = new OAuthTokens
                {
                    AccessToken = "oauth_access_token_123",
                    RefreshToken = "oauth_refresh_token_456",
                    ExpiresAt = 1625162113,
                    Scopes = new List<string> { "email", "profile" },
                },
            };
        }

        [Fact]
        public async void TestListUsers()
        {
            var mockResponse = new WorkOSList<User>
            {
                Data = new List<User>
                {
                    this.mockUser,
                },
            };
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/user_management/users",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));
            var response = await this.service.ListUsers(this.listUsersOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/user_management/users");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestCreateUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/user_management/users",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockUser));
            var response = await this.service.CreateUser(this.createUserOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/user_management/users");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestCreateUserWithIdempotency()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/user_management/users",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockUser));
            var response = await this.service.CreateUser(this.createUserOptions, "idemKey123456");

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/user_management/users");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/user_management/users/{this.mockUser.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));

            var response = await this.service.GetUser(this.mockUser.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/user_management/users/{this.mockUser.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetUserByExternalId()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/user_management/users/external_id/{this.mockUser.ExternalId}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));

            var response = await this.service.GetUserByExternalId(this.mockUser.ExternalId);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/user_management/users/external_id/{this.mockUser.ExternalId}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestDeleteUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Delete,
                $"/user_management/users/{this.mockUser.Id}",
                HttpStatusCode.Accepted,
                "Accepted");

            await this.service.DeleteUser(this.mockUser.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Delete,
                $"/user_management/users/{this.mockUser.Id}");
        }

        [Fact]
        public async void TestUpdateUser()
        {
            this.httpMock.MockResponse(
                HttpMethod.Put,
                $"/user_management/users/{this.updateUserOptions.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));
            var response = await this.service.UpdateUser(this.updateUserOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Put, $"/user_management/users/{this.updateUserOptions.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetPasswordReset()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/user_management/password_reset/{this.mockPasswordReset.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockPasswordReset));

            var response = await this.service.GetPasswordReset(this.mockPasswordReset.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/user_management/password_reset/{this.mockPasswordReset.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockPasswordReset),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestCreatePasswordReset()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/user_management/password_reset",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockPasswordReset));
            var response = await this.service.CreatePasswordReset(this.createPasswordResetOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/user_management/password_reset");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockPasswordReset),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestResetPassword()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/user_management/password_reset/confirm",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockUser));
            var response = await this.service.ResetPassword(this.resetPasswordOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/user_management/password_reset/confirm");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockUser),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetOrganizationMembership()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/user_management/organization_memberships/{this.mockOrganizationMembership.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockOrganizationMembership));

            var response = await this.service.GetOrganizationMembership(this.mockOrganizationMembership.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/user_management/organization_memberships/{this.mockOrganizationMembership.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganizationMembership),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestListOrganizationMemberships()
        {
            var mockResponse = new WorkOSList<OrganizationMembership>
            {
                Data = new List<OrganizationMembership>
                {
                    this.mockOrganizationMembership,
                },
            };
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/user_management/organization_memberships",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockResponse));
            var response = await this.service.ListOrganizationMemberships(this.listOrganizationMembershipsOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/user_management/organization_memberships");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestCreateOrganizationMembership()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/user_management/organization_memberships",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockOrganizationMembership));
            var response = await this.service.CreateOrganizationMembership(this.createOrganizationMembershipOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/user_management/organization_memberships");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganizationMembership),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestUpdateOrganizationMembership()
        {
            this.httpMock.MockResponse(
                HttpMethod.Put,
                $"/user_management/organization_memberships/{this.updateOrganizationMembershipOptions.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockOrganizationMembership));
            var response = await this.service.UpdateOrganizationMembership(this.updateOrganizationMembershipOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Put, $"/user_management/organization_memberships/{this.updateOrganizationMembershipOptions.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganizationMembership),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestDeleteOrganizationMembership()
        {
            this.httpMock.MockResponse(
                HttpMethod.Delete,
                $"/user_management/organization_memberships/{this.mockOrganizationMembership.Id}",
                HttpStatusCode.Accepted,
                "Accepted");

            await this.service.DeleteOrganizationMembership(this.mockOrganizationMembership.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Delete,
                $"/user_management/organization_memberships/{this.mockOrganizationMembership.Id}");
        }

        [Fact]
        public async void TestDeactivateOrganizationMembership()
        {
            this.httpMock.MockResponse(
                HttpMethod.Put,
                $"/user_management/organization_memberships/{this.mockOrganizationMembership.Id}/deactivate",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockOrganizationMembership));

            var response = await this.service.DeactivateOrganizationMembership(this.mockOrganizationMembership.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Put,
                $"/user_management/organization_memberships/{this.mockOrganizationMembership.Id}/deactivate");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganizationMembership),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestReactivateOrganizationMembership()
        {
            this.httpMock.MockResponse(
                HttpMethod.Put,
                $"/user_management/organization_memberships/{this.mockOrganizationMembership.Id}/reactivate",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockOrganizationMembership));

            var response = await this.service.ReactivateOrganizationMembership(this.mockOrganizationMembership.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Put,
                $"/user_management/organization_memberships/{this.mockOrganizationMembership.Id}/reactivate");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockOrganizationMembership),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public void TestGetLogoutUrl()
        {
            var sessionId = "session_01HQAG1HENBZMAZD82YRXDFC0B";
            var logoutUrl = this.service.GetLogoutUrl(sessionId);

            Assert.NotNull(logoutUrl);
            Assert.Contains($"/user_management/sessions/logout", logoutUrl);
            Assert.Contains($"session_id={System.Uri.EscapeDataString(sessionId)}", logoutUrl);
        }

        [Fact]
        public void TestGetLogoutUrlWithReturnTo()
        {
            var sessionId = "session_01HQAG1HENBZMAZD82YRXDFC0B";
            var returnTo = "https://your-app.com/signed-out";
            var logoutUrl = this.service.GetLogoutUrl(sessionId, returnTo);

            Assert.NotNull(logoutUrl);
            Assert.Contains($"/user_management/sessions/logout", logoutUrl);
            Assert.Contains($"session_id={System.Uri.EscapeDataString(sessionId)}", logoutUrl);
            Assert.Contains($"return_to={System.Uri.EscapeDataString(returnTo)}", logoutUrl);
        }

        [Fact]
        public void TestGetLogoutUrlWithNullSessionId()
        {
            Assert.Throws<ArgumentNullException>(() => this.service.GetLogoutUrl(null));
        }

        [Fact]
        public void TestGetLogoutUrlWithEmptySessionId()
        {
            Assert.Throws<ArgumentNullException>(() => this.service.GetLogoutUrl(string.Empty));
        }

        [Fact]
        public void TestGetJwksUrl()
        {
            var clientId = "client_ABCDEF0123456789";
            var jwksUrl = this.service.GetJwksUrl(clientId);

            Assert.NotNull(jwksUrl);
            Assert.Contains($"/sso/jwks/{clientId}", jwksUrl);
        }

        [Fact]
        public void TestGetJwksUrlWithSpecialCharacters()
        {
            var clientId = "org_test@example.com";
            var jwksUrl = this.service.GetJwksUrl(clientId);

            Assert.NotNull(jwksUrl);
            Assert.Contains($"/sso/jwks/{System.Uri.EscapeDataString(clientId)}", jwksUrl);
        }

        [Fact]
        public void TestGetJwksUrlWithNullClientId()
        {
            Assert.Throws<ArgumentNullException>(() => this.service.GetJwksUrl(null));
        }

        [Fact]
        public void TestGetJwksUrlWithEmptyClientId()
        {
            Assert.Throws<ArgumentNullException>(() => this.service.GetJwksUrl(string.Empty));
        }

        [Fact]
        public async void TestAuthenticateWithCode()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/user_management/authenticate",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockAuthenticationResponse));
            var response = await this.service.AuthenticateWithCode(this.authenticateWithCodeOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/user_management/authenticate");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockAuthenticationResponse),
                JsonConvert.SerializeObject(response));
        }
    }
}
