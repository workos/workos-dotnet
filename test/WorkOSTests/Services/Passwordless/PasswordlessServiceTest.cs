namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class PasswordlessServiceTest
    {
        private readonly HttpMock httpMock;

        private readonly PasswordlessService service;

        private readonly CreatePasswordlessSessionOptions createPasswordlessSessionOptions;

        private readonly PasswordlessSession mockPasswordlessSession;

        public PasswordlessServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new PasswordlessService(client);

            this.createPasswordlessSessionOptions = new CreatePasswordlessSessionOptions
            {
                Email = "test@foo-corp.com",
                Type = PasswordlessSessionType.MagicLink,
            };

            this.mockPasswordlessSession = new PasswordlessSession
            {
                Id = "passwordless_session_123",
                Email = "test@foo-corp.com",
                ExpiresAt = DateTime.Now.AddMinutes(5),
                Link = "https://auth.workos.com/passwordless/token_123/confirm",
            };
        }

        [Fact]
        public void TestCreateSession()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/passwordless/sessions",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockPasswordlessSession));
            var response = this.service.CreateSession(this.createPasswordlessSessionOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/passwordless/sessions");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockPasswordlessSession),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestCreateSessionAsync()
        {
            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/passwordless/sessions",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(this.mockPasswordlessSession));
            var response = await this.service.CreateSessionAsync(this.createPasswordlessSessionOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/passwordless/sessions");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockPasswordlessSession),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public void TestSendSession()
        {
            var id = this.mockPasswordlessSession.Id;

            var mockResponse = new Dictionary<string, bool>
            {
                { "success", true },
            };
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/passwordless/sessions/{id}/send",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(mockResponse));
            var success = this.service.SendSession(id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/passwordless/sessions/{id}/send");
            Assert.True(success);
        }

        [Fact]
        public async void TestSendSessionAsync()
        {
            var id = this.mockPasswordlessSession.Id;

            var mockResponse = new Dictionary<string, bool>
            {
                { "success", true },
            };
            this.httpMock.MockResponse(
                HttpMethod.Post,
                $"/passwordless/sessions/{id}/send",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(mockResponse));
            var success = await this.service.SendSessionAsync(id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Post,
                $"/passwordless/sessions/{id}/send");
            Assert.True(success);
        }
    }
}
