namespace WorkOSTests
{
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
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/enroll");
            Assert.NotNull(response);
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
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/enroll");
            Assert.NotNull(response);
            Assert.Equal("+15555555555", response.Sms.PhoneNumber);
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
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/enroll");
            Assert.NotNull(response);
            Assert.NotNull(response.Totp);
        }

        [Fact]
        public async void TestChallengeWithCode()
        {
            var challengeResponse = new Challenge
            {
                Id = "auth_challenge_test123",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                ExpiresAt = "2022-02-18T16:08:03.205Z",
                Code = "12345",
                FactorId = "auth_factor_test123",
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/auth/factors/challenge",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(challengeResponse));

            var options = new ChallengeFactorOptions
            {
                FactorId = "auth_factor_test123",
            };
            var response = await this.service.ChallengeFactor(options);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/challenge");
            Assert.NotNull(response);
            Assert.NotNull(response.Code);
        }

        [Fact]
        public async void TestChallengeWithoutCode()
        {
            var challengeResponse = new Challenge
            {
                Id = "auth_challenge_test123",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                FactorId = "auth_factor_test123",
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/auth/factors/challenge",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(challengeResponse));

            var options = new ChallengeFactorOptions
            {
                FactorId = "auth_factor_test123",
            };
            var response = await this.service.ChallengeFactor(options);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/challenge");
            Assert.NotNull(response);
            Assert.NotNull(response.Id);
        }

        [Fact]
        public async void TestVerify()
        {
            var verifyChallenge = new Challenge
            {
                Id = "auth_challenge_test123",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                ExpiresAt = "2022-02-18T16:08:03.205Z",
                Code = "12345",
                FactorId = "auth_factor_test123",
            };

            var verifyResponse = new VerifyFactorResponseSuccess
            {
                Challenge = verifyChallenge,
                IsValid = true,
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/auth/factors/verify",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(verifyResponse));

            var options = new VerifyFactorOptions
            {
                ChallengeId = "auth_challenge_test123",
                Code = "12345",
            };
            var response = await this.service.VerifyFactor(options);
            if (response is VerifyFactorResponseSuccess successResponse)
            {
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/auth/factors/verify");
            Assert.NotNull(successResponse);
            Assert.True(successResponse.IsValid);
            }
        }

        [Fact]
        public async void TestGetFactor()
        {
            var mockFactor = new Factor
            {
                Id = "auth_factor_test123",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                Type = "generic_otp",
                EnvironmentId = "environment_test123",
            };

            this.httpMock.MockResponse(
                HttpMethod.Get,
                $"/auth/factors/{mockFactor.Id}",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(mockFactor));

            var response = await this.service.GetFactor(mockFactor.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Get,
                $"/auth/factors/{mockFactor.Id}");
            Assert.Equal(
                JsonConvert.SerializeObject(mockFactor),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestDeleteFactor()
        {
            var mockFactor = new Factor
            {
                Id = "auth_factor_test123",
                CreatedAt = "2022-02-17T22:39:26.616Z",
                UpdatedAt = "2022-02-17T22:39:26.616Z",
                Type = "generic_otp",
                EnvironmentId = "environment_test123",
            };

            this.httpMock.MockResponse(
                HttpMethod.Delete,
                $"/auth/factors/{mockFactor.Id}",
                HttpStatusCode.Accepted,
                "Accepted");

            await this.service.DeleteFactor(mockFactor.Id);

            this.httpMock.AssertRequestWasMade(
                HttpMethod.Delete,
                $"/auth/factors/{mockFactor.Id}");
        }
    }
}
