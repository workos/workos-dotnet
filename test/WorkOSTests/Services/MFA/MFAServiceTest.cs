namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class MfaServiceTest
    {
        private readonly HttpMock httpMock;
        private readonly MfaService service;

        // private readonly Factor mockFactor;
        public MfaServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new MfaService(client);
        }

        [Fact]
        public async void TestGenericEnroll()
        {
            var enrollFactorResponse = new Factor
            {
                Object = "authentication_factor",
                Id = "auth_factor_01FW4XE6WTNHABQD6TGP6125AV",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                Type = "generic_otp",
                EnvironmentId = "environment_01EPZWK497BAJ96SW5Q99RWH3C",
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/auth/factors/enroll",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(enrollFactorResponse));

            var options = new EnrollFactorOptions("generic_oidc");
            var response = await this.service.EnrollFactor(options);
            var responseObject = response.Object;
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/enroll");
            Assert.NotNull(response);
            Assert.Equal("authentication_factor", responseObject);
        }

        [Fact]
        public void TestSmsEnroll()
        {
            var options = new EnrollSmsFactorOptions("+15555555555");
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.service.EnrollFactor(options));
        }

        [Fact]
        public void TestTotpEnroll()
        {
            var options = new EnrollTotpFactorOptions("WorkOS", "some_user");
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.service.EnrollFactor(options));
        }
    }
}
