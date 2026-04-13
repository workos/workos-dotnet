// @oagen-ignore-file
namespace WorkOSTests
{
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Verifies wire-shape behavior shared across the AuthenticateWith*
    /// wrappers, in particular: every grant carries client_id, every grant
    /// EXCEPT device_code carries client_secret.
    /// </summary>
#pragma warning disable SA1202 // helpers placed beside the tests that use them
    public class AuthenticateWrappersTest
    {
        private const string AuthFixture = "{\"access_token\":\"a\",\"refresh_token\":\"r\",\"user\":{\"object\":\"user\",\"id\":\"user_1\",\"email\":\"x@y.com\",\"first_name\":null,\"last_name\":null,\"email_verified\":true,\"created_at\":\"2026-01-01T00:00:00Z\",\"updated_at\":\"2026-01-01T00:00:00Z\"}}";

        [Fact]
        public async Task AuthenticateWithPassword_SendsClientSecret()
        {
            var (mock, svc) = NewClient();
            mock.MockResponse(HttpMethod.Post, "/user_management/authenticate", HttpStatusCode.OK, AuthFixture);
            await svc.AuthenticateWithPassword(new AuthenticateWithPasswordOptions { Email = "u@e.com", Password = "p" });
            var body = await mock.GetLastRequestBodyAsync();
            Assert.Contains("\"client_id\":\"client_test\"", body);
            Assert.Contains("\"client_secret\":\"sk_test\"", body);
            Assert.Contains("\"grant_type\":\"password\"", body);
        }

        [Fact]
        public async Task AuthenticateWithCode_SendsClientSecret()
        {
            var (mock, svc) = NewClient();
            mock.MockResponse(HttpMethod.Post, "/user_management/authenticate", HttpStatusCode.OK, AuthFixture);
            await svc.AuthenticateWithCode(new AuthenticateWithCodeOptions { Code = "c" });
            var body = await mock.GetLastRequestBodyAsync();
            Assert.Contains("\"client_secret\":\"sk_test\"", body);
            Assert.Contains("\"grant_type\":\"authorization_code\"", body);
        }

        [Fact]
        public async Task AuthenticateWithDeviceCode_OmitsClientSecret()
        {
            var (mock, svc) = NewClient();
            mock.MockResponse(HttpMethod.Post, "/user_management/authenticate", HttpStatusCode.OK, AuthFixture);
            await svc.AuthenticateWithDeviceCode(new AuthenticateWithDeviceCodeOptions { DeviceCode = "dc" });
            var body = await mock.GetLastRequestBodyAsync();
            Assert.Contains("\"client_id\":\"client_test\"", body);
            Assert.DoesNotContain("\"client_secret\"", body);
            Assert.Contains("\"grant_type\":\"urn:ietf:params:oauth:grant-type:device_code\"", body);
        }

        private static (HttpMock Mock, UserManagementService Service) NewClient()
        {
            var mock = new HttpMock();
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                ClientId = "client_test",
                HttpClient = mock.HttpClient,
            });
            return (mock, new UserManagementService(client));
        }
    }
#pragma warning restore SA1202
}
