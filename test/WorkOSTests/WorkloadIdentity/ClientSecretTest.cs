// @oagen-ignore-file
#nullable enable
namespace WorkOSTests
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// <see cref="WorkOSClient.GetClientSecret"/> hands the API key to the calls that send it
    /// as <c>client_secret</c>. A client built with a key gets the key. A client built with
    /// workload identity credentials has none, and is refused before anything is sent. A
    /// client with neither cannot be built (see <c>WorkOSClientTest.TestEmptyAPIKey</c>).
    /// </summary>
    public class ClientSecretTest
    {
        [Fact]
        public void AClientBuiltWithAnApiKeyGetsTheKey()
        {
            var api = Wif.Api();
            using var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test_static",
                HttpClient = new HttpClient(api),
            });

            Assert.Equal("sk_test_static", client.GetClientSecret());
            Assert.Equal(0, api.Count);
        }

        [Fact]
        public void AWorkloadIdentityClientIsRefusedBeforeAnyRequestOrExchange()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api();
            var provider = new CountingProvider("assertion-1");
            using var credentials = Wif.Credentials(Wif.Options(provider), token);
            using var client = Wif.Client(credentials, api);

            var error = Assert.Throws<InvalidOperationException>(
                () => client.GetClientSecret("AuthenticateWithPasswordAsync"));

            Assert.Equal(
                "AuthenticateWithPasswordAsync needs an API key: this request sends it as the client secret. "
                + "A client that authenticates with workload identity has no API key, so use a client built with one for this call.",
                error.Message);

            // Nothing was sent: not to the API, and not to the token endpoint.
            Assert.Equal(0, api.Count);
            Assert.Equal(0, token.ExchangeCount);
            Assert.Equal(0, provider.Count);
        }

        [Fact]
        public async Task TheErrorNamesTheCallingMethodWhenNoNameIsPassed()
        {
            var token = new TokenEndpoint();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, Wif.Api());

            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => GetProfileAndTokenAsync(client));

            Assert.StartsWith("GetProfileAndTokenAsync needs an API key: ", error.Message);
            Assert.Equal(0, token.ExchangeCount);
        }

        [Fact]
        public void APublicClientStillBuildsAPkceAuthorizationUrlWithoutAnApiKey()
        {
            var client = new PublicWorkOSClient(new PublicWorkOSOptions { ClientId = "client_01" });

            var result = client.GetAuthorizationUrlWithPkce(new AuthKitAuthorizationUrlOptions
            {
                RedirectUri = "myapp://callback",
            });

            Assert.StartsWith("https://api.workos.com/user_management/authorize?", result.Url);
            Assert.Contains("client_id=client_01", result.Url);
            Assert.Contains("code_challenge=" + PkceUtilities.GenerateCodeChallenge(result.CodeVerifier), result.Url);
            Assert.Contains("code_challenge_method=S256", result.Url);
            Assert.DoesNotContain("client_secret", result.Url);
        }

        // Shaped like a service method that sends the key: an async method that passes no
        // argument, so the compiler supplies this method's name.
        private static async Task<string> GetProfileAndTokenAsync(WorkOSClient client)
        {
            var secret = client.GetClientSecret();
            await Task.CompletedTask;
            return secret;
        }
    }
}
