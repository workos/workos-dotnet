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
    }
}
