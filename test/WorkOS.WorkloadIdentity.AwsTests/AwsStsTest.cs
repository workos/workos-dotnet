// @oagen-ignore-file
namespace WorkOS.WorkloadIdentity.AwsTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Net.Sockets;
    using System.Text;
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

        [Theory]
        [InlineData(HttpStatusCode.BadRequest, "Throttling")]
        [InlineData(HttpStatusCode.InternalServerError, "InternalFailure")]
        [InlineData(HttpStatusCode.ServiceUnavailable, "ServiceUnavailable")]
        public async Task OneProviderCallIsOneStsRequestWhenStsThrottlesOrFails(HttpStatusCode status, string code)
        {
            // A real STS client with the provider's own configuration, over a stub HTTP
            // layer that counts requests. The AWS SDK would retry each of these answers.
            var sts = new FailingSts(status, code);
            var config = AwsSts.BuildConfig("us-east-1", () => null);
            config.HttpClientFactory = new StubHttpClientFactory(sts);
            using var client = new AmazonSecurityTokenServiceClient(TestCredentials(), config);

            var error = await Assert.ThrowsAsync<InvalidOperationException>(
                () => AwsSts.Create(Audience, client).GetTokenAsync(CancellationToken.None));

            Assert.Contains(code, error.Message);
            Assert.Equal(1, sts.Count);
        }

        [Fact]
        public void TheStsClientMakesOneAttemptAndAllowsTenSecondsToConnectAndTenForTheRequest()
        {
            var config = AwsSts.BuildConfig("us-east-1", () => null);

            Assert.Equal(0, config.MaxErrorRetry);
            Assert.Equal(TimeSpan.FromSeconds(10), config.ConnectTimeout);
            Assert.Equal(TimeSpan.FromSeconds(10), config.Timeout);
        }

        [Fact]
        public async Task AnStsThatNeverAnswersFailsAtTheRequestLimitAfterOneRequest()
        {
            // The provider's configuration with its 10-second request limit shortened, against
            // a local socket that accepts a connection and never answers.
            using var sts = new SilentSts();
            var config = AwsSts.BuildConfig("us-east-1", () => null);
            config.ServiceURL = $"http://127.0.0.1:{sts.Port}/";
            config.AuthenticationRegion = "us-east-1";
            config.Timeout = TimeSpan.FromMilliseconds(250);
            using var client = new AmazonSecurityTokenServiceClient(TestCredentials(), config);

            await Assert.ThrowsAsync<TimeoutException>(
                () => AwsSts.Create(Audience, client).GetTokenAsync(CancellationToken.None));

            // The timed-out request was not sent again. A retry would have connected before
            // the call failed, so the count is final once the listener has seen the first.
            await sts.Connected.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(1, sts.Connections);
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

        // Static credentials, so that building a client asks no credential source.
        private static AWSCredentials TestCredentials()
        {
            return new BasicAWSCredentials("test-access-key", "test-secret-key");
        }

        /// <summary>Answers every STS request with the same error and counts the requests.</summary>
        private sealed class FailingSts : HttpMessageHandler
        {
            private readonly HttpStatusCode status;
            private readonly string code;
            private int count;

            internal FailingSts(HttpStatusCode status, string code)
            {
                this.status = status;
                this.code = code;
            }

            internal int Count => Volatile.Read(ref this.count);

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                Interlocked.Increment(ref this.count);
                var body = "<ErrorResponse xmlns=\"https://sts.amazonaws.com/doc/2011-06-15/\">"
                    + $"<Error><Type>Sender</Type><Code>{this.code}</Code><Message>The request failed.</Message></Error>"
                    + "<RequestId>request-id</RequestId></ErrorResponse>";
                return Task.FromResult(new HttpResponseMessage(this.status)
                {
                    Content = new StringContent(body, Encoding.UTF8, "text/xml"),
                    RequestMessage = request,
                });
            }
        }

        /// <summary>Makes the STS client send through a stub handler instead of the network.</summary>
        private sealed class StubHttpClientFactory : HttpClientFactory
        {
            private readonly HttpMessageHandler handler;

            internal StubHttpClientFactory(HttpMessageHandler handler) => this.handler = handler;

            public override HttpClient CreateHttpClient(IClientConfig clientConfig)
            {
                return new HttpClient(this.handler, disposeHandler: false);
            }

            public override bool UseSDKHttpClientCaching(IClientConfig clientConfig) => false;

            public override bool DisposeHttpClientsAfterUse(IClientConfig clientConfig) => true;
        }

        /// <summary>A loopback listener that accepts connections, counts them, and never answers.</summary>
        private sealed class SilentSts : IDisposable
        {
            private readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            private readonly List<Socket> accepted = new List<Socket>();
            private readonly TaskCompletionSource<bool> connected = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private int connections;

            internal SilentSts()
            {
                this.listener.Start();
                _ = this.AcceptAsync();
            }

            internal int Port => ((IPEndPoint)this.listener.LocalEndpoint).Port;

            internal int Connections => Volatile.Read(ref this.connections);

            /// <summary>Completes when the listener has accepted its first connection.</summary>
            internal Task Connected => this.connected.Task;

            public void Dispose()
            {
                this.listener.Stop();
                lock (this.accepted)
                {
                    this.accepted.ForEach(socket => socket.Dispose());
                }
            }

            private async Task AcceptAsync()
            {
                try
                {
                    while (true)
                    {
                        var socket = await this.listener.AcceptSocketAsync();
                        Interlocked.Increment(ref this.connections);
                        lock (this.accepted)
                        {
                            this.accepted.Add(socket);
                        }

                        this.connected.TrySetResult(true);
                    }
                }
                catch (Exception ex) when (ex is SocketException || ex is ObjectDisposedException)
                {
                    // The listener was stopped.
                }
            }
        }
    }
}
