// @oagen-ignore-file
namespace WorkOS.WorkloadIdentity.GcpTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using WorkOSTests;
    using Xunit;

    /// <summary>
    /// The Google Cloud provider cases, WIF-GCP-01 and WIF-GCP-02, and the rest of the
    /// provider's contract. The metadata server is a server on a loopback port, reached
    /// through the provider's real HTTP client.
    /// </summary>
    public class GcpMetadataTest
    {
        private const string Audience = "https://api.workos.com";

        [Fact]
        public async Task WIF_GCP_01_GetsTheIdentityUrlWithTheAudienceAndTheMetadataHeaderAndReturnsTheBody()
        {
            using var server = new LoopbackHttpServer(_ => new LoopbackResponse(200, "gcp-jwt\n"));

            var token = await GcpMetadata.Create(Audience, server.BaseUrl).GetTokenAsync(CancellationToken.None);

            Assert.Equal("gcp-jwt", token);
            var request = Assert.Single(server.Requests);
            Assert.Equal("GET", request.Method);
            Assert.Equal("/computeMetadata/v1/instance/service-accounts/default/identity", request.Path);
            Assert.Equal("audience=https%3A%2F%2Fapi.workos.com&format=full", request.Query);
            Assert.Equal("Google", request.Headers["Metadata-Flavor"]);
        }

        [Fact]
        public async Task WIF_GCP_02_ANon200OrEmptyBodyIsAnErrorAndNoRedirectIsFollowed()
        {
            // The metadata server answers 404.
            using var missing = new LoopbackHttpServer(_ => new LoopbackResponse(404, "Not Found"));
            var notFound = await Assert.ThrowsAsync<HttpRequestException>(
                () => GcpMetadata.Create(Audience, missing.BaseUrl).GetTokenAsync(CancellationToken.None));
            Assert.Equal(HttpStatusCode.NotFound, notFound.StatusCode);
            Assert.Contains("404", notFound.Message);

            // It answers 200 with an empty body.
            using var empty = new LoopbackHttpServer(_ => new LoopbackResponse(200, string.Empty));
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => GcpMetadata.Create(Audience, empty.BaseUrl).GetTokenAsync(CancellationToken.None));

            // It answers 302 to a second server, which must receive nothing.
            using var second = new LoopbackHttpServer(_ => new LoopbackResponse(200, "redirected-jwt"));
            using var first = new LoopbackHttpServer(_ => LoopbackResponse.Redirect(302, second.BaseUrl + "/identity"));
            await Assert.ThrowsAsync<HttpRequestException>(
                () => GcpMetadata.Create(Audience, first.BaseUrl).GetTokenAsync(CancellationToken.None));

            Assert.Single(first.Requests);
            Assert.Empty(second.Requests);
        }

        [Fact]
        public async Task ABodyOfOnlyWhitespaceIsAnError()
        {
            using var server = new LoopbackHttpServer(_ => new LoopbackResponse(200, " \n"));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => GcpMetadata.Create(Audience, server.BaseUrl).GetTokenAsync(CancellationToken.None));
        }

        [Fact]
        public async Task AnErrorNamesTheStatusAndNotTheBody()
        {
            using var server = new LoopbackHttpServer(_ => new LoopbackResponse(500, "SECRET-GCP-JWT"));

            var error = await Assert.ThrowsAsync<HttpRequestException>(
                () => GcpMetadata.Create(Audience, server.BaseUrl).GetTokenAsync(CancellationToken.None));

            Assert.Contains("500", error.Message);
            Assert.DoesNotContain("SECRET-GCP-JWT", error.ToString());
        }

        [Fact]
        public void TheDefaultBaseUrlIsTheMetadataServer()
        {
            Assert.Equal("http://metadata.google.internal", GcpMetadata.DefaultBaseUrl);
        }

        [Fact]
        public void TheHttpClientGivesUpAfter10Seconds()
        {
            Assert.Equal(TimeSpan.FromSeconds(10), GcpMetadata.Http.Timeout);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void AnAudienceIsRequired(string? audience)
        {
            Assert.Throws<ArgumentException>(() => GcpMetadata.Create(audience!));
        }
    }
}
