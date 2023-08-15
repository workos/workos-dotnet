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
    }
}