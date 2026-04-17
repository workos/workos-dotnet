// @oagen-ignore-file
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    public class PasswordlessServiceTest
    {
        private readonly HttpMock httpMock;
        private readonly PasswordlessService service;

        public PasswordlessServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = this.httpMock.HttpClient,
            });
            this.service = new PasswordlessService(client);
        }

        [Fact]
        public async Task TestCreateSession()
        {
            var fixture = "{\"object\":\"passwordless_session\",\"id\":\"passwordless_session_01\",\"email\":\"test@example.com\",\"expires_at\":\"2025-01-01T00:00:00Z\",\"link\":\"https://auth.workos.com/passwordless/token\"}";
            this.httpMock.MockResponse(HttpMethod.Post, "/passwordless/sessions", HttpStatusCode.OK, fixture);
            var result = await this.service.CreateSession(new CreatePasswordlessSessionOptions { Email = "test@example.com" });
            Assert.NotNull(result);
            Assert.Equal("passwordless_session_01", result.Id);
            Assert.Equal("test@example.com", result.Email);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/passwordless/sessions");
        }

        [Fact]
        public async Task TestSendSession()
        {
            this.httpMock.MockResponse(HttpMethod.Post, "/passwordless/sessions/ps_01/send", HttpStatusCode.OK, "{}");
            await this.service.SendSession("ps_01");
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/passwordless/sessions/ps_01/send");
        }
    }

    public class VaultServiceTest
    {
        private readonly HttpMock httpMock;
        private readonly VaultService service;

        public VaultServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = this.httpMock.HttpClient,
            });
            this.service = new VaultService(client);
        }

        [Fact]
        public async Task TestCreateObject()
        {
            var fixture = "{\"id\":\"obj_01\",\"name\":\"my-secret\",\"environment_id\":\"env_01\"}";
            this.httpMock.MockResponse(HttpMethod.Post, "/vault/v1/kv", HttpStatusCode.OK, fixture);
            var result = await this.service.CreateObjectAsync(new CreateVaultObjectOptions { Name = "my-secret", Value = "secret-value" });
            Assert.NotNull(result);
            Assert.Equal("obj_01", result.Id);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/vault/v1/kv");
        }

        [Fact]
        public async Task TestReadObject()
        {
            var fixture = "{\"id\":\"obj_01\",\"name\":\"my-secret\",\"value\":\"secret-value\"}";
            this.httpMock.MockResponse(HttpMethod.Get, "/vault/v1/kv/obj_01", HttpStatusCode.OK, fixture);
            var result = await this.service.ReadObjectAsync("obj_01");
            Assert.NotNull(result);
            Assert.Equal("secret-value", result.Value);
            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/vault/v1/kv/obj_01");
        }

        [Fact]
        public async Task TestDeleteObject()
        {
            this.httpMock.MockResponse(HttpMethod.Delete, "/vault/v1/kv/obj_01", HttpStatusCode.NoContent, "");
            await this.service.DeleteObjectAsync("obj_01");
            this.httpMock.AssertRequestWasMade(HttpMethod.Delete, "/vault/v1/kv/obj_01");
        }

        [Fact]
        public async Task TestListObjects()
        {
            var fixture = "{\"data\":[{\"id\":\"obj_01\",\"name\":\"secret-1\"}],\"list_metadata\":{\"before\":null,\"after\":null}}";
            this.httpMock.MockResponse(HttpMethod.Get, "/vault/v1/kv", HttpStatusCode.OK, fixture);
            var result = await this.service.ListObjectsAsync();
            Assert.NotNull(result);
            Assert.NotEmpty(result.Data);
            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/vault/v1/kv");
        }

        [Fact]
        public async Task TestCreateDataKey()
        {
            var fixture = "{\"context\":{\"env\":\"test\"},\"data_key\":{\"id\":\"key_01\",\"key\":\"dGVzdC1rZXk=\"},\"encrypted_keys\":\"encrypted_blob\"}";
            this.httpMock.MockResponse(HttpMethod.Post, "/vault/v1/keys/data-key", HttpStatusCode.OK, fixture);
            var result = await this.service.CreateDataKeyAsync(new CreateDataKeyOptions { Context = new Dictionary<string, string> { { "env", "test" } } });
            Assert.NotNull(result);
            Assert.Equal("encrypted_blob", result.EncryptedKeys);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/vault/v1/keys/data-key");
        }

        [Fact]
        public async Task TestDecryptDataKey()
        {
            var fixture = "{\"id\":\"key_01\",\"key\":\"dGVzdC1rZXk=\"}";
            this.httpMock.MockResponse(HttpMethod.Post, "/vault/v1/keys/decrypt", HttpStatusCode.OK, fixture);
            var result = await this.service.DecryptDataKeyAsync(new DecryptDataKeyOptions { Keys = "encrypted_blob" });
            Assert.NotNull(result);
            Assert.Equal("dGVzdC1rZXk=", result.Key);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/vault/v1/keys/decrypt");
        }
    }

    public class ActionsServiceTest
    {
        private readonly ActionsService service = new ActionsService();

        [Fact]
        public void TestSignResponse()
        {
            var result = this.service.SignResponse(ActionType.Authentication, ActionVerdict.Allow, "test_secret");
            Assert.NotNull(result);
            Assert.Equal("authentication_action_response", result.Object);
            Assert.NotNull(result.Signature);
            Assert.NotEmpty(result.Signature);
        }

        [Fact]
        public void TestSignResponseDeny()
        {
            var result = this.service.SignResponse(ActionType.UserRegistration, ActionVerdict.Deny, "test_secret", "Not allowed");
            Assert.NotNull(result);
            Assert.Equal("user_registration_action_response", result.Object);
            Assert.NotNull(result.Signature);
        }

        [Fact]
        public void TestVerifyHeaderInvalidSignature()
        {
            Assert.Throws<InvalidOperationException>(() =>
                this.service.VerifyHeader("{}", "t=1234567890,sig=invalid_signature", "test_secret"));
        }

        [Fact]
        public void TestConstructActionInvalidSignature()
        {
            Assert.Throws<InvalidOperationException>(() =>
                this.service.ConstructAction("{}", "t=1234567890,sig=invalid_signature", "test_secret"));
        }
    }

    public class SessionSealTest
    {
        [Fact]
        public void TestSealUnsealRoundTrip()
        {
            var data = new Dictionary<string, object>
            {
                { "access_token", "test_access_token" },
                { "refresh_token", "test_refresh_token" },
            };
            var password = "a-strong-cookie-password-32chars!";
            var sealed_data = SessionService.SealData(data, password);
            Assert.NotNull(sealed_data);
            Assert.Contains("~", sealed_data);

            var unsealed = SessionService.UnsealData(sealed_data, password);
            Assert.Equal("test_access_token", unsealed["access_token"].ToString());
            Assert.Equal("test_refresh_token", unsealed["refresh_token"].ToString());
        }

        [Fact]
        public void TestSealSessionFromAuthResponse()
        {
            var password = "a-strong-cookie-password-32chars!";
            var sealed_data = SessionService.SealSessionFromAuthResponse("at_123", "rt_456", password);
            Assert.NotNull(sealed_data);

            var unsealed = SessionService.UnsealData(sealed_data, password);
            Assert.Equal("at_123", unsealed["access_token"].ToString());
            Assert.Equal("rt_456", unsealed["refresh_token"].ToString());
        }

        [Fact]
        public void TestUnsealWithWrongPasswordFails()
        {
            var data = new Dictionary<string, object> { { "key", "value" } };
            var sealed_data = SessionService.SealData(data, "correct-password-32characters!!");
            Assert.ThrowsAny<Exception>(() => SessionService.UnsealData(sealed_data, "wrong-password-32characters!!!!"));
        }
    }

    public class PkceUtilitiesTest
    {
        [Fact]
        public void TestGenerateCodeVerifier()
        {
            var verifier = PkceUtilities.GenerateCodeVerifier();
            Assert.Equal(43, verifier.Length);
            Assert.DoesNotContain("+", verifier);
            Assert.DoesNotContain("/", verifier);
            Assert.DoesNotContain("=", verifier);
        }

        [Fact]
        public void TestGenerateCodeVerifierCustomLength()
        {
            var verifier = PkceUtilities.GenerateCodeVerifier(128);
            Assert.Equal(128, verifier.Length);
        }

        [Fact]
        public void TestGenerateCodeVerifierInvalidLength()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PkceUtilities.GenerateCodeVerifier(10));
            Assert.Throws<ArgumentOutOfRangeException>(() => PkceUtilities.GenerateCodeVerifier(200));
        }

        [Fact]
        public void TestGenerateCodeChallenge()
        {
            var challenge = PkceUtilities.GenerateCodeChallenge("test_verifier");
            Assert.NotNull(challenge);
            Assert.NotEmpty(challenge);
            Assert.DoesNotContain("+", challenge);
            Assert.DoesNotContain("/", challenge);
            Assert.DoesNotContain("=", challenge);
        }

        [Fact]
        public void TestGeneratePair()
        {
            var pair = PkceUtilities.Generate();
            Assert.NotNull(pair);
            Assert.Equal(43, pair.CodeVerifier.Length);
            Assert.NotEmpty(pair.CodeChallenge);
            Assert.Equal("S256", pair.CodeChallengeMethod);
        }

        [Fact]
        public void TestCodeChallengeIsDeterministic()
        {
            var c1 = PkceUtilities.GenerateCodeChallenge("fixed_verifier");
            var c2 = PkceUtilities.GenerateCodeChallenge("fixed_verifier");
            Assert.Equal(c1, c2);
        }
    }

    public class AuthorizationUrlBuilderTest
    {
        [Fact]
        public void TestBuildAuthKitAuthorizationUrl()
        {
            var url = AuthorizationUrlBuilder.BuildAuthKitAuthorizationUrl(
                "https://api.workos.com",
                "client_01",
                new AuthKitAuthorizationUrlOptions
                {
                    RedirectUri = "https://example.com/callback",
                    Provider = "authkit",
                });
            Assert.Contains("client_id=client_01", url);
            Assert.Contains("redirect_uri=", url);
            Assert.Contains("response_type=code", url);
            Assert.Contains("provider=authkit", url);
            Assert.StartsWith("https://api.workos.com/user_management/authorize?", url);
        }

        [Fact]
        public void TestBuildSSOAuthorizationUrl()
        {
            var url = AuthorizationUrlBuilder.BuildSSOAuthorizationUrl(
                "https://api.workos.com",
                "client_01",
                new SSOAuthorizationUrlOptions
                {
                    RedirectUri = "https://example.com/callback",
                    Connection = "conn_01",
                });
            Assert.Contains("client_id=client_01", url);
            Assert.Contains("connection=conn_01", url);
            Assert.StartsWith("https://api.workos.com/sso/authorize?", url);
        }

        [Fact]
        public void TestBuildSSOAuthorizationUrlRequiresIdentifier()
        {
            Assert.Throws<ArgumentException>(() =>
                AuthorizationUrlBuilder.BuildSSOAuthorizationUrl(
                    "https://api.workos.com",
                    "client_01",
                    new SSOAuthorizationUrlOptions
                    {
                        RedirectUri = "https://example.com/callback",
                    }));
        }

        [Fact]
        public void TestBuildLogoutUrl()
        {
            var url = AuthorizationUrlBuilder.BuildLogoutUrl("https://api.workos.com", "session_01", "https://example.com");
            Assert.Contains("session_id=session_01", url);
            Assert.Contains("return_to=", url);
            Assert.StartsWith("https://api.workos.com/user_management/sessions/logout?", url);
        }

        [Fact]
        public void TestBuildAuthKitAuthorizationUrlWithPkce()
        {
            var result = AuthorizationUrlBuilder.BuildAuthKitAuthorizationUrlWithPkce(
                "https://api.workos.com",
                "client_01",
                new AuthKitAuthorizationUrlOptions
                {
                    RedirectUri = "https://example.com/callback",
                });
            Assert.NotNull(result.Url);
            Assert.NotEmpty(result.State);
            Assert.NotEmpty(result.CodeVerifier);
            Assert.Contains("code_challenge=", result.Url);
            Assert.Contains("code_challenge_method=S256", result.Url);
        }
    }

    public class PublicWorkOSClientTest
    {
        [Fact]
        public void TestConstructionRequiresClientId()
        {
            Assert.Throws<ArgumentException>(() => new PublicWorkOSClient(new PublicWorkOSOptions()));
        }

        [Fact]
        public void TestGetAuthorizationUrl()
        {
            var client = new PublicWorkOSClient(new PublicWorkOSOptions { ClientId = "client_01" });
            var url = client.GetAuthorizationUrl(new AuthKitAuthorizationUrlOptions
            {
                RedirectUri = "https://example.com/callback",
            });
            Assert.Contains("client_id=client_01", url);
        }

        [Fact]
        public void TestGetJwksUrl()
        {
            var client = new PublicWorkOSClient(new PublicWorkOSOptions { ClientId = "client_01" });
            var url = client.GetJwksUrl();
            Assert.Equal("https://api.workos.com/sso/jwks/client_01", url);
        }

        [Fact]
        public void TestGetSSOAuthorizationUrl()
        {
            var client = new PublicWorkOSClient(new PublicWorkOSOptions { ClientId = "client_01" });
            var url = client.GetSSOAuthorizationUrl(new SSOAuthorizationUrlOptions
            {
                RedirectUri = "https://example.com/callback",
                Connection = "conn_01",
            });
            Assert.Contains("connection=conn_01", url);
        }
    }
}
