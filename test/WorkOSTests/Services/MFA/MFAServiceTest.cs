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
                Id = "auth_factor_test123",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                Type = "generic_otp",
                EnvironmentId = "environment_test123",
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
        public async void TestSmsEnroll()
        {
            var phoneDetails = new Sms
            {
                PhoneNumber = "+15555555555",
            };

            var enrollFactorResponse = new Factor
            {
                Object = "authentication_factor",
                Id = "auth_factor_test123",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                Type = "sms",
                EnvironmentId = "environment_test123",
                Sms = phoneDetails,
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/auth/factors/enroll",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(enrollFactorResponse));

            var options = new EnrollSmsFactorOptions("+15555555555");
            var response = await this.service.EnrollFactor(options);
            var responseNumber = response.Sms.PhoneNumber;
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/enroll");
            Assert.NotNull(response);
            Assert.Equal("+15555555555", responseNumber);
        }

        [Fact]
        public async void TestTotpEnroll()
        {
            var totpDetails = new Totp
            {
                QrCode = "data:image/png;base64,some long text",
                Secret = "secret",
                Uri = "otpauth://totp/Issuer:some_user?secret=secret",
            };

            var enrollFactorResponse = new Factor
            {
                Object = "authentication_factor",
                Id = "auth_factor_test123",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                Type = "sms",
                EnvironmentId = "environment_test123",
                Totp = totpDetails,
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/auth/factors/enroll",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(enrollFactorResponse));

            var options = new EnrollTotpFactorOptions("Issuer", "some_user");
            var response = await this.service.EnrollFactor(options);
            var responseTotp = response.Totp;
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/enroll");
            Assert.NotNull(response);
            Assert.NotNull(response.Totp);
        }
    }
}
