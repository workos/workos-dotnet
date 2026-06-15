// @oagen-ignore-file
namespace WorkOSTests
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using WorkOS;
    using Xunit;

    public class SessionServiceTest
    {
        private const string CookiePassword = "test-password-at-least-32-chars!";
        private const string ClientId = "client_01TESTCLIENT";

        /// <summary>
        /// End-to-end repro for #266: a valid sealed session created by
        /// SealSessionFromAuthResponse must authenticate successfully when
        /// the JWKS endpoint returns a raw JWKS document.
        /// </summary>
        [Fact]
        public async Task AuthenticateAsync_ValidJwt_WithRawJwks_ReturnsAuthenticated()
        {
            using var rsa = RSA.Create(2048);
            var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = "test-key-id" };

            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaSecurityKey);
            jwk.Use = "sig";
            jwk.Alg = "RS256";
            var jwksJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                keys = new[] { jwk },
            });

            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("sid", "session_01TEST"),
                    new Claim("org_id", "org_01TEST"),
                    new Claim("role", "admin"),
                    new Claim("sub", "user_01TEST"),
                }),
                Expires = now.AddHours(1),
                NotBefore = now.AddSeconds(-5),
                IssuedAt = now,
                SigningCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.CreateEncodedJwt(tokenDescriptor);

            var sealedSession = SessionService.SealSessionFromAuthResponse(
                accessToken,
                "refresh_token_value",
                CookiePassword);

            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                ClientId = ClientId,
            });

            var sessionService = client.Session;
            sessionService.SetJwksManagerForTesting(new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://api.workos.com/sso/jwks/{ClientId}",
                new JwksConfigurationRetriever(),
                new StaticDocumentRetriever(jwksJson)));

            var result = await sessionService.AuthenticateAsync(
                sealedSession,
                CookiePassword,
                CancellationToken.None);

            Assert.True(result.Authenticated);
            Assert.Equal("session_01TEST", result.SessionId);
            Assert.Equal("org_01TEST", result.OrganizationId);
            Assert.Equal("admin", result.Role);
            Assert.Equal(accessToken, result.AccessToken);
            Assert.Null(result.Reason);
        }

        /// <summary>
        /// Verifies that JwksConfigurationRetriever correctly parses a raw JWKS
        /// document and returns the expected signing keys.
        /// </summary>
        [Fact]
        public async Task JwksConfigurationRetriever_ParsesRawJwks_ReturnsSigningKeys()
        {
            using var rsa = RSA.Create(2048);
            var rsaSecurityKey = new RsaSecurityKey(rsa) { KeyId = "test-key-id" };

            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaSecurityKey);
            jwk.Use = "sig";
            jwk.Alg = "RS256";
            var jwksJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                keys = new[] { jwk },
            });

            var retriever = new JwksConfigurationRetriever();
            var config = await retriever.GetConfigurationAsync(
                "https://api.workos.com/sso/jwks/client_01TEST",
                new StaticDocumentRetriever(jwksJson),
                CancellationToken.None);

            Assert.NotEmpty(config.SigningKeys);
            Assert.Single(config.SigningKeys);
            Assert.Equal("test-key-id", config.SigningKeys.First().KeyId);
        }

        /// <summary>
        /// Verifies that multiple keys in the JWKS are all returned.
        /// </summary>
        [Fact]
        public async Task JwksConfigurationRetriever_MultipleKeys_ReturnsAll()
        {
            using var rsa1 = RSA.Create(2048);
            using var rsa2 = RSA.Create(2048);

            var key1 = new RsaSecurityKey(rsa1) { KeyId = "key-1" };
            var key2 = new RsaSecurityKey(rsa2) { KeyId = "key-2" };

            var jwk1 = JsonWebKeyConverter.ConvertFromRSASecurityKey(key1);
            jwk1.Use = "sig";
            jwk1.Alg = "RS256";
            var jwk2 = JsonWebKeyConverter.ConvertFromRSASecurityKey(key2);
            jwk2.Use = "sig";
            jwk2.Alg = "RS256";

            var jwksJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                keys = new[] { jwk1, jwk2 },
            });

            var retriever = new JwksConfigurationRetriever();
            var config = await retriever.GetConfigurationAsync(
                "https://api.workos.com/sso/jwks/client_01TEST",
                new StaticDocumentRetriever(jwksJson),
                CancellationToken.None);

            Assert.Equal(2, config.SigningKeys.Count);
            var keyIds = config.SigningKeys.Select(k => k.KeyId).OrderBy(k => k).ToList();
            Assert.Equal("key-1", keyIds[0]);
            Assert.Equal("key-2", keyIds[1]);
        }

        /// <summary>
        /// Verifies that an empty JWKS document results in no signing keys.
        /// </summary>
        [Fact]
        public async Task JwksConfigurationRetriever_EmptyKeys_ReturnsNoSigningKeys()
        {
            var jwksJson = "{\"keys\":[]}";

            var retriever = new JwksConfigurationRetriever();
            var config = await retriever.GetConfigurationAsync(
                "https://api.workos.com/sso/jwks/client_01TEST",
                new StaticDocumentRetriever(jwksJson),
                CancellationToken.None);

            Assert.Empty(config.SigningKeys);
        }

        /// <summary>
        /// A simple IDocumentRetriever that returns a fixed string,
        /// allowing us to test JWKS parsing without network calls.
        /// </summary>
        private sealed class StaticDocumentRetriever : IDocumentRetriever
        {
            private readonly string document;

            public StaticDocumentRetriever(string document)
            {
                this.document = document;
            }

            public Task<string> GetDocumentAsync(string address, CancellationToken cancel)
            {
                return Task.FromResult(this.document);
            }
        }
    }
}
