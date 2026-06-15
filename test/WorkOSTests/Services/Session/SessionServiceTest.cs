// @oagen-ignore-file
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
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
        /// Verifies that AuthenticateAsync succeeds when the JWKS endpoint
        /// returns a raw JWKS document (not an OIDC discovery document).
        /// This is the exact scenario that was broken before the fix.
        /// </summary>
        [Fact]
        public async Task AuthenticateAsync_ValidJwt_WithRawJwks_ReturnsAuthenticated()
        {
            // Arrange: generate an RSA key pair
            using var rsa = RSA.Create(2048);
            var rsaSecurityKey = new RsaSecurityKey(rsa)
            {
                KeyId = "test-key-id",
            };

            // Build a raw JWKS document ({ "keys": [...] })
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaSecurityKey);
            jwk.Use = "sig";
            jwk.Alg = "RS256";
            var jwksJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                keys = new[] { jwk },
            });

            // Create a valid JWT signed with the private key
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

            // Seal the session
            var sealedSession = SessionService.SealSessionFromAuthResponse(
                accessToken,
                "refresh_token_value",
                CookiePassword);

            // Set up the client with the required ClientId
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                ClientId = ClientId,
            });

            var sessionService = client.Session;

            // Inject a ConfigurationManager that uses our in-memory JWKS
            // via a custom IDocumentRetriever that returns the raw JWKS JSON
            sessionService.SetJwksManagerForTesting(new ConfigurationManager<OpenIdConnectConfiguration>(
                $"https://api.workos.com/sso/jwks/{ClientId}",
                new JwksConfigurationRetriever(),
                new StaticDocumentRetriever(jwksJson)));

            // Act
            var result = await sessionService.AuthenticateAsync(
                sealedSession,
                CookiePassword,
                CancellationToken.None);

            // Assert
            Assert.True(result.Authenticated);
            Assert.Equal("session_01TEST", result.SessionId);
            Assert.Equal("org_01TEST", result.OrganizationId);
            Assert.Equal("admin", result.Role);
            Assert.Equal(accessToken, result.AccessToken);
            Assert.Null(result.Reason);
        }

        /// <summary>
        /// Verifies that an expired JWT returns InvalidJwt failure.
        /// </summary>
        [Fact]
        public async Task AuthenticateAsync_ExpiredJwt_ReturnsInvalidJwt()
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

            // Create an expired JWT
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", "user_01TEST") }),
                Expires = now.AddHours(-1),
                NotBefore = now.AddHours(-2),
                IssuedAt = now.AddHours(-2),
                SigningCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.CreateEncodedJwt(tokenDescriptor);

            var sealedSession = SessionService.SealSessionFromAuthResponse(
                accessToken, "rt", CookiePassword);

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
                sealedSession, CookiePassword, CancellationToken.None);

            Assert.False(result.Authenticated);
            Assert.Equal(SessionFailureReason.InvalidJwt, result.Reason);
        }

        /// <summary>
        /// Verifies that a JWT signed with a different key returns InvalidJwt.
        /// </summary>
        [Fact]
        public async Task AuthenticateAsync_WrongSigningKey_ReturnsInvalidJwt()
        {
            using var signingRsa = RSA.Create(2048);
            using var differentRsa = RSA.Create(2048);

            // JWKS contains the different key (not the signing key)
            var differentKey = new RsaSecurityKey(differentRsa) { KeyId = "different-key" };
            var jwk = JsonWebKeyConverter.ConvertFromRSASecurityKey(differentKey);
            jwk.Use = "sig";
            jwk.Alg = "RS256";
            var jwksJson = System.Text.Json.JsonSerializer.Serialize(new
            {
                keys = new[] { jwk },
            });

            // Sign the JWT with a key NOT in the JWKS
            var signingKey = new RsaSecurityKey(signingRsa) { KeyId = "signing-key" };
            var now = DateTime.UtcNow;
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("sub", "user_01TEST") }),
                Expires = now.AddHours(1),
                NotBefore = now.AddSeconds(-5),
                IssuedAt = now,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.RsaSha256),
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.CreateEncodedJwt(tokenDescriptor);

            var sealedSession = SessionService.SealSessionFromAuthResponse(
                accessToken, "rt", CookiePassword);

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
                sealedSession, CookiePassword, CancellationToken.None);

            Assert.False(result.Authenticated);
            Assert.Equal(SessionFailureReason.InvalidJwt, result.Reason);
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
