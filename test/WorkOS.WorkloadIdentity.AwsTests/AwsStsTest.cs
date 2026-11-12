// @oagen-ignore-file
namespace WorkOS.WorkloadIdentity.AwsTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.SecurityToken;
    using Amazon.SecurityToken.Model;
    using Moq;
    using Xunit;

    /// <summary>
    /// The AWS provider cases, WIF-AWS-01 and WIF-AWS-02, and the rest of the provider's
    /// contract. The STS client is a stub; no test calls AWS.
    /// </summary>
    public class AwsStsTest
    {
        private const string Audience = "https://api.workos.com";

        [Fact]
        public async Task WIF_AWS_01_CallsGetWebIdentityTokenWithTheAudienceAndRs256OnARegionalEndpoint()
        {
            var requests = new List<GetWebIdentityTokenRequest>();
            var sts = new Mock<IAmazonSecurityTokenService>(MockBehavior.Strict);
            sts.Setup(client => client.GetWebIdentityTokenAsync(It.IsAny<GetWebIdentityTokenRequest>(), It.IsAny<CancellationToken>()))
                .Callback<GetWebIdentityTokenRequest, CancellationToken>((request, _) => requests.Add(request))
                .ReturnsAsync(new GetWebIdentityTokenResponse { WebIdentityToken = "aws-jwt" });

            var token = await AwsSts.Create(Audience, sts.Object).GetTokenAsync(CancellationToken.None);

            Assert.Equal("aws-jwt", token);
            var sent = Assert.Single(requests);
            Assert.Equal(new[] { Audience }, sent.Audience);
            Assert.Equal("RS256", sent.SigningAlgorithm);
            Assert.Null(sent.DurationSeconds);

            // The client the provider builds for us-east-1 is regional, not global.
            var config = AwsSts.BuildConfig("us-east-1", () => null);
            Assert.Equal("us-east-1", config.RegionEndpoint.SystemName);
            using (var client = new AmazonSecurityTokenServiceClient(new AnonymousAWSCredentials(), config))
            {
                var endpoint = client.DetermineServiceOperationEndpoint(new GetWebIdentityTokenRequest());
                Assert.Equal("https://sts.us-east-1.amazonaws.com/", endpoint.URL);
            }

            // With no region available, building the provider is an error.
            var noRegion = Assert.Throws<InvalidOperationException>(() => AwsSts.BuildConfig(null, () => null));
            Assert.Contains("region", noRegion.Message);
        }

        [Fact]
        public async Task WIF_AWS_02_AnStsErrorSurfacesAsAProviderErrorNamingTheCall()
        {
            var denied = new AmazonSecurityTokenServiceException(
                "User is not authorized to perform: sts:GetWebIdentityToken",
                ErrorType.Sender,
                "AccessDenied",
                "request-id",
                HttpStatusCode.Forbidden);
            var sts = new Mock<IAmazonSecurityTokenService>(MockBehavior.Strict);
            sts.Setup(client => client.GetWebIdentityTokenAsync(It.IsAny<GetWebIdentityTokenRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(denied);

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => AwsSts.Create(Audience, sts.Object).GetTokenAsync(CancellationToken.None));

            Assert.Contains("sts:GetWebIdentityToken", error.Message);
            Assert.Contains("AccessDenied", error.Message);
            Assert.Same(denied, error.InnerException);
            Assert.DoesNotContain("aws-jwt", error.ToString());
        }

        [Fact]
        public async Task AClientSideFailureAlsoNamesTheCall()
        {
            var sts = new Mock<IAmazonSecurityTokenService>(MockBehavior.Strict);
            sts.Setup(client => client.GetWebIdentityTokenAsync(It.IsAny<GetWebIdentityTokenRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonClientException("Unable to get IAM security credentials."));

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => AwsSts.Create(Audience, sts.Object).GetTokenAsync(CancellationToken.None));

            Assert.Contains("sts:GetWebIdentityToken", error.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task AResponseWithoutATokenIsAnError(string? webIdentityToken)
        {
            var sts = new Mock<IAmazonSecurityTokenService>(MockBehavior.Strict);
            sts.Setup(client => client.GetWebIdentityTokenAsync(It.IsAny<GetWebIdentityTokenRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetWebIdentityTokenResponse { WebIdentityToken = webIdentityToken });

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => AwsSts.Create(Audience, sts.Object).GetTokenAsync(CancellationToken.None));

            Assert.Contains("sts:GetWebIdentityToken", error.Message);
        }

        [Fact]
        public void TheRegionArgumentWinsOverTheResolvedRegion()
        {
            var config = AwsSts.BuildConfig("eu-west-1", () => RegionEndpoint.USEast1);

            Assert.Equal("eu-west-1", config.RegionEndpoint.SystemName);
        }

        [Fact]
        public void TheResolvedRegionIsUsedWhenNoneIsGiven()
        {
            var config = AwsSts.BuildConfig(null, () => RegionEndpoint.APSoutheast2);

            Assert.Equal("ap-southeast-2", config.RegionEndpoint.SystemName);
            using var client = new AmazonSecurityTokenServiceClient(new AnonymousAWSCredentials(), config);
            Assert.Equal(
                "https://sts.ap-southeast-2.amazonaws.com/",
                client.DetermineServiceOperationEndpoint(new GetWebIdentityTokenRequest()).URL);
        }

        [Fact]
        public void TheGlobalEndpointIsRejected()
        {
            Assert.Throws<InvalidOperationException>(() => AwsSts.BuildConfig("aws-global", () => null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void AnAudienceIsRequired(string? audience)
        {
            var sts = new Mock<IAmazonSecurityTokenService>(MockBehavior.Strict);

            Assert.Throws<ArgumentException>(() => AwsSts.Create(audience!, sts.Object));
            Assert.Throws<ArgumentException>(() => AwsSts.Create(audience!, "us-east-1"));
        }
    }
}
