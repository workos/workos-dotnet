// @oagen-ignore-file
namespace WorkOS.WorkloadIdentity.GitHubActionsTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using WorkOSTests;
    using Xunit;

    /// <summary>
    /// The GitHub Actions provider cases, WIF-GHA-01 and WIF-GHA-02, and the rest of the
    /// provider's contract. GitHub's token endpoint is a server on a loopback port, reached
    /// through the provider's real HTTP client.
    /// </summary>
    public class GitHubActionsTest : IDisposable
    {
        private const string UrlVariable = "ACTIONS_ID_TOKEN_REQUEST_URL";
        private const string TokenVariable = "ACTIONS_ID_TOKEN_REQUEST_TOKEN";
        private const string Audience = "https://api.workos.com";

        private readonly string? originalUrl = Environment.GetEnvironmentVariable(UrlVariable);
        private readonly string? originalToken = Environment.GetEnvironmentVariable(TokenVariable);

        public GitHubActionsTest()
        {
            // Start every case as a machine that is not a GitHub Actions runner, even when
            // the suite itself runs on one.
            Environment.SetEnvironmentVariable(UrlVariable, null);
            Environment.SetEnvironmentVariable(TokenVariable, null);
        }

        public void Dispose()
        {
            Environment.SetEnvironmentVariable(UrlVariable, this.originalUrl);
            Environment.SetEnvironmentVariable(TokenVariable, this.originalToken);
        }

        [Fact]
        public async Task WIF_GHA_01_GetsTheRequestUrlWithTheAudienceAppendedAndReturnsTheValue()
        {
            using var server = new LoopbackHttpServer(_ => new LoopbackResponse(200, "{\"value\": \"gha-jwt\"}"));
            Environment.SetEnvironmentVariable(UrlVariable, server.BaseUrl + "/token?api-version=2.0");
            Environment.SetEnvironmentVariable(TokenVariable, "gha-request-token");

            var token = await GitHubActions.Create(Audience).GetTokenAsync(CancellationToken.None);

            Assert.Equal("gha-jwt", token);
            var request = Assert.Single(server.Requests);
            Assert.Equal("GET", request.Method);
            Assert.Equal("/token", request.Path);

            // The existing query string is kept and the encoded audience is added to it.
            Assert.Equal("api-version=2.0&audience=https%3A%2F%2Fapi.workos.com", request.Query);
            Assert.Equal("Bearer gha-request-token", request.Headers["Authorization"]);
            Assert.Equal("application/json", request.Headers["Accept"]);
        }

        [Fact]
        public async Task WIF_GHA_02_MissingVariablesFailWithTheHintToAddThePermission()
        {
            using var server = new LoopbackHttpServer(_ => new LoopbackResponse(200, "{\"value\": \"gha-jwt\"}"));
            var provider = GitHubActions.Create(Audience);

            // The URL is unset.
            Environment.SetEnvironmentVariable(UrlVariable, null);
            Environment.SetEnvironmentVariable(TokenVariable, "gha-request-token");
            var withoutUrl = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetTokenAsync(CancellationToken.None));
            Assert.Contains("permissions: id-token: write", withoutUrl.Message);

            // Separately, the request token is unset.
            Environment.SetEnvironmentVariable(UrlVariable, server.BaseUrl + "/token?api-version=2.0");
            Environment.SetEnvironmentVariable(TokenVariable, null);
            var withoutToken = await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetTokenAsync(CancellationToken.None));
            Assert.Contains("permissions: id-token: write", withoutToken.Message);

            Assert.Empty(server.Requests);
        }

        [Fact]
        public async Task TheAudienceStartsTheQueryWhenTheUrlHasNone()
        {
            using var server = new LoopbackHttpServer(_ => new LoopbackResponse(200, "{\"value\": \"gha-jwt\"}"));
            Environment.SetEnvironmentVariable(UrlVariable, server.BaseUrl + "/token");
            Environment.SetEnvironmentVariable(TokenVariable, "gha-request-token");

            await GitHubActions.Create("api://workos/audience one").GetTokenAsync(CancellationToken.None);

            Assert.Equal("audience=api%3A%2F%2Fworkos%2Faudience%20one", Assert.Single(server.Requests).Query);
        }

        [Fact]
        public async Task TheVariablesAreReadOnEveryCall()
        {
            using var first = new LoopbackHttpServer(_ => new LoopbackResponse(200, "{\"value\": \"first-jwt\"}"));
            using var second = new LoopbackHttpServer(_ => new LoopbackResponse(200, "{\"value\": \"second-jwt\"}"));
            var provider = GitHubActions.Create(Audience);
            Environment.SetEnvironmentVariable(TokenVariable, "gha-request-token");

            Environment.SetEnvironmentVariable(UrlVariable, first.BaseUrl + "/token?api-version=2.0");
            Assert.Equal("first-jwt", await provider.GetTokenAsync(CancellationToken.None));

            Environment.SetEnvironmentVariable(UrlVariable, second.BaseUrl + "/token?api-version=2.0");
            Assert.Equal("second-jwt", await provider.GetTokenAsync(CancellationToken.None));
        }

        [Fact]
        public async Task AStatusOtherThan200IsAnErrorThatNamesTheStatusAndNotTheBody()
        {
            using var server = new LoopbackHttpServer(_ => new LoopbackResponse(403, "{\"value\": \"SECRET-GHA-JWT\"}"));
            Environment.SetEnvironmentVariable(UrlVariable, server.BaseUrl + "/token?api-version=2.0");
            Environment.SetEnvironmentVariable(TokenVariable, "gha-request-token");

            var error = await Assert.ThrowsAsync<HttpRequestException>(() => GitHubActions.Create(Audience).GetTokenAsync(CancellationToken.None));

            Assert.Equal(HttpStatusCode.Forbidden, error.StatusCode);
            Assert.Contains("403", error.Message);
            Assert.DoesNotContain("SECRET-GHA-JWT", error.ToString());
            Assert.DoesNotContain("gha-request-token", error.ToString());
        }

        [Theory]
        [InlineData("{}")]
        [InlineData("{\"value\": \"\"}")]
        [InlineData("{\"value\": 42}")]
        [InlineData("not json")]
        public async Task AResponseWithoutAValueIsAnError(string body)
        {
            using var server = new LoopbackHttpServer(_ => new LoopbackResponse(200, body));
            Environment.SetEnvironmentVariable(UrlVariable, server.BaseUrl + "/token?api-version=2.0");
            Environment.SetEnvironmentVariable(TokenVariable, "gha-request-token");

            await Assert.ThrowsAsync<InvalidOperationException>(() => GitHubActions.Create(Audience).GetTokenAsync(CancellationToken.None));
        }

        [Fact]
        public async Task ARedirectIsNotFollowed()
        {
            using var second = new LoopbackHttpServer(_ => new LoopbackResponse(200, "{\"value\": \"redirected-jwt\"}"));
            using var first = new LoopbackHttpServer(_ => LoopbackResponse.Redirect(302, second.BaseUrl + "/token"));
            Environment.SetEnvironmentVariable(UrlVariable, first.BaseUrl + "/token?api-version=2.0");
            Environment.SetEnvironmentVariable(TokenVariable, "gha-request-token");

            await Assert.ThrowsAsync<HttpRequestException>(() => GitHubActions.Create(Audience).GetTokenAsync(CancellationToken.None));

            Assert.Single(first.Requests);
            Assert.Empty(second.Requests);
        }

        [Fact]
        public void TheHttpClientGivesUpAfter10Seconds()
        {
            Assert.Equal(TimeSpan.FromSeconds(10), GitHubActions.Http.Timeout);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void AnAudienceIsRequired(string? audience)
        {
            Assert.Throws<ArgumentException>(() => GitHubActions.Create(audience!));
        }
    }
}
