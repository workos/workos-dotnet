// @oagen-ignore-file
namespace WorkOSTests
{
    using System.Linq;
    using System.Security.Cryptography;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Tokens;
    using WorkOS;
    using Xunit;

    public class SessionServiceTest
    {
        /// <summary>
        /// Verifies that JwksConfigurationRetriever correctly parses a raw JWKS
        /// document and returns the expected signing keys. This is the exact
        /// scenario that was broken before the fix: the endpoint serves
        /// { "keys": [...] } directly (not an OIDC discovery document).
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
