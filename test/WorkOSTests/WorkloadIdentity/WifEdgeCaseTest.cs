// @oagen-ignore-file
#nullable enable
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Time.Testing;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Behavior from the shared design that the numbered conformance cases do not pin down:
    /// the rest of the exchange's error table, per-request overrides, configuration errors
    /// and the exchange deadline.
    /// </summary>
    [Collection(WorkloadIdentityCollection.Name)]
    public class WifEdgeCaseTest : IDisposable
    {
        private const int Attempts = WorkOSOptions.DefaultMaxRetries + 1;

        private readonly EnvironmentScope environment = new EnvironmentScope();

        public WifEdgeCaseTest()
        {
            foreach (var name in Wif.EnvironmentVariables)
            {
                this.environment.Set(name, null);
            }
        }

        public void Dispose()
        {
            this.environment.Dispose();
        }

        [Fact]
        public async Task PerRequestAccessTokenWinsOverCredentialsAndIsNeverRecovered()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api(_ => Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api);

            var request = Wif.Get();
            request.AccessToken = "user-access-token";
            await Assert.ThrowsAsync<AuthenticationException>(() => client.MakeRawAPIRequest(request));

            var sent = Assert.Single(api.Requests);
            Assert.Equal("Bearer user-access-token", sent.Headers["Authorization"]);
            Assert.False(sent.Headers.ContainsKey(Wif.EnvironmentHeader));
            Assert.Equal(0, token.ExchangeCount);
        }

        [Fact]
        public async Task PerRequestApiKeyWinsOverCredentialsAndIsNeverRecovered()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api(_ => Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api);

            var request = Wif.Get();
            request.RequestOptions = new RequestOptions { ApiKey = "sk_test_override" };
            await Assert.ThrowsAsync<AuthenticationException>(() => client.MakeRawAPIRequest(request));

            var sent = Assert.Single(api.Requests);
            Assert.Equal("Bearer sk_test_override", sent.Headers["Authorization"]);
            Assert.False(sent.Headers.ContainsKey(Wif.EnvironmentHeader));
            Assert.Equal(0, token.ExchangeCount);
        }

        [Fact]
        public async Task CustomCredentialsAreResolvedPerAttemptAndInvalidatedByGeneration()
        {
            var custom = new SequenceCredentials();
            var api = Wif.Api(number => number == 1
                ? Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}")
                : Wif.Json(HttpStatusCode.OK, "{}"));
            using var client = Wif.Client(custom, api);

            await client.MakeRawAPIRequest(Wif.Get());

            Assert.Equal(2, api.Count);
            Assert.Equal("Bearer custom-1", api.Requests[0].Headers["Authorization"]);
            Assert.Equal("Bearer custom-2", api.Requests[1].Headers["Authorization"]);
            Assert.Equal("tenant-a", api.Requests[1].Headers["x-custom-tenant"]);
            Assert.Equal(new long[] { 1 }, custom.Invalidated);
        }

        [Fact]
        public async Task AMalformedAuthorizationFromCustomCredentialsIsNotQuotedInTheError()
        {
            // No scheme, and characters a scheme cannot have: the runtime would quote it.
            const string Secret = "abc/def==";
            var api = Wif.Api();
            using var client = Wif.Client(new FixedHeaderCredentials(Secret), api);

            var error = await Assert.ThrowsAsync<FormatException>(() => client.MakeRawAPIRequest(Wif.Get()));

            Assert.DoesNotContain(Secret, error.ToString());
            Assert.Equal(0, api.Count);
        }

        [Fact]
        public async Task The401RecoverySpendsNoTransportRetry()
        {
            // With no retries allowed at all, the one recovery still happens.
            var token = new TokenEndpoint();
            var api = Wif.Api(number => number == 1
                ? Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}")
                : Wif.Json(HttpStatusCode.OK, "{}"));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api, maxRetries: 0);

            var response = await client.MakeRawAPIRequest(Wif.Get());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, api.Count);
            Assert.Equal(2, token.ExchangeCount);
        }

        [Fact]
        public async Task The401RecoveryLeavesTheTransportRetryLimitIntact()
        {
            // One retry allowed: 401 (recovered), 503 (the one retry), 401 (returned).
            var token = new TokenEndpoint();
            var api = Wif.Api(number => number switch
            {
                1 => Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"),
                2 => Wif.Status(HttpStatusCode.ServiceUnavailable, retryAfter: "0"),
                _ => Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"),
            });
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api, maxRetries: 1);

            await Assert.ThrowsAsync<AuthenticationException>(() => client.MakeRawAPIRequest(Wif.Get()));

            Assert.Equal(3, api.Count);
            Assert.Equal(2, token.ExchangeCount);
        }

        [Fact]
        public async Task ATransportRetryResolvesAgainSoItNeverSendsAnExpiredToken()
        {
            var token = new TokenEndpoint();
            var clock = new FakeTimeProvider();
            var api = Wif.Api(number =>
            {
                if (number == 1)
                {
                    // The token expires while the first attempt is in flight.
                    clock.Advance(TimeSpan.FromSeconds(301));
                    return Wif.Status(HttpStatusCode.ServiceUnavailable, retryAfter: "0");
                }

                return Wif.Json(HttpStatusCode.OK, "{}");
            });
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token, clock);
            using var client = Wif.Client(credentials, api);

            await client.MakeRawAPIRequest(Wif.Get());

            Assert.Equal(2, api.Count);
            Assert.Equal(2, token.ExchangeCount);
            Assert.Equal("Bearer access-1", api.Requests[0].Headers["Authorization"]);
            Assert.Equal("Bearer access-2", api.Requests[1].Headers["Authorization"]);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task AProviderThatNeverReturnsCannotOutliveTheDeadline(bool blocksItsThread)
        {
            var reached = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var unblock = new ManualResetEventSlim(false);
            var calls = 0;
            var options = new WorkloadIdentityCredentialsOptions
            {
                ClientId = Wif.ClientId,
                TokenUrl = Wif.TokenUrl,
                EnvironmentId = Wif.EnvironmentId,

                // The first call ignores its cancellation token and never produces a value.
                IdentityTokenCallback = _ =>
                {
                    if (Interlocked.Increment(ref calls) > 1)
                    {
                        return Task.FromResult("assertion-1");
                    }

                    reached.SetResult(true);
                    if (blocksItsThread)
                    {
                        unblock.Wait();
                    }

                    return new TaskCompletionSource<string>().Task;
                },
            };
            var token = new TokenEndpoint();
            var clock = new FakeTimeProvider();
            using var credentials = Wif.Credentials(options, token, clock);

            try
            {
                var first = credentials.ResolveAsync(CancellationToken.None);
                var second = credentials.ResolveAsync(CancellationToken.None);
                await reached.Task.WaitAsync(TimeSpan.FromSeconds(10));

                clock.Advance(TimeSpan.FromSeconds(31));

                var firstError = await Assert.ThrowsAsync<WorkloadIdentityException>(() => first.WaitAsync(TimeSpan.FromSeconds(10)));
                var secondError = await Assert.ThrowsAsync<WorkloadIdentityException>(() => second.WaitAsync(TimeSpan.FromSeconds(10)));
                Assert.Equal("provider_failed", firstError.Code);
                Assert.Same(firstError, secondError);
                Assert.Equal(0, token.ExchangeCount);

                // Nothing is left in flight: the next call starts a new exchange.
                var snapshot = await credentials.ResolveAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(10));
                Assert.Equal("Bearer access-1", snapshot.Headers["Authorization"]);
                Assert.Equal(1, token.ExchangeCount);
            }
            finally
            {
                // Let the deliberately blocked thread go.
                unblock.Set();
            }
        }

        [Fact]
        public async Task ServerErrorsUntilTheRetriesRunOutFailWithExchangeFailed()
        {
            var token = new TokenEndpoint((_, _, _) => Wif.Status(HttpStatusCode.ServiceUnavailable));
            var provider = new CountingProvider("assertion-1");
            using var credentials = Wif.Credentials(Wif.Options(provider), token);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => credentials.ResolveAsync(CancellationToken.None));

            Assert.Equal("exchange_failed", error.Code);
            Assert.Contains("503", error.Description);
            Assert.Equal(Attempts, token.ExchangeCount);
            Assert.Equal(Attempts, provider.Count);
        }

        [Fact]
        public async Task ARetriedResponseWithAnOAuthErrorGivesItsCodeAfterTheLastAttempt()
        {
            var token = new TokenEndpoint((_, _, _) => Wif.OAuthError("temporarily_unavailable", "Try later.", HttpStatusCode.ServiceUnavailable));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => credentials.ResolveAsync(CancellationToken.None));

            // The status is checked before the body: the 503 was retried despite its OAuth error.
            Assert.Equal(Attempts, token.ExchangeCount);
            Assert.Equal("temporarily_unavailable", error.Code);
            Assert.Equal("Try later.", error.Description);
            Assert.Equal("The token endpoint rejected the exchange.", error.Hint);
        }

        [Fact]
        public async Task ConnectionFailuresUntilTheRetriesRunOutFailWithExchangeFailed()
        {
            var token = new TokenEndpoint((_, _, _) => throw new HttpRequestException("Connection refused"));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => credentials.ResolveAsync(CancellationToken.None));

            Assert.Equal("exchange_failed", error.Code);
            Assert.IsType<HttpRequestException>(error.InnerException);
            Assert.Equal(Attempts, token.ExchangeCount);
        }

        [Fact]
        public async Task ProviderFailuresUntilTheRetriesRunOutFailWithProviderFailed()
        {
            var token = new TokenEndpoint();
            var cause = new InvalidOperationException("The token source is unavailable.");
            var provider = new CountingProvider(_ => throw cause);
            using var credentials = Wif.Credentials(Wif.Options(provider), token);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => credentials.ResolveAsync(CancellationToken.None));

            Assert.Equal("provider_failed", error.Code);
            Assert.Same(cause, error.InnerException);
            Assert.Equal(Attempts, provider.Count);
            Assert.Equal(0, token.ExchangeCount);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AnEmptyAssertionIsAProviderFailure(string assertion)
        {
            var token = new TokenEndpoint();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider(assertion)), token);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => credentials.ResolveAsync(CancellationToken.None));

            Assert.Equal("provider_failed", error.Code);
            Assert.Equal(0, token.ExchangeCount);
        }

        [Fact]
        public async Task AMissingTokenFileAndAnUnsetVariableAreProviderFailures()
        {
            var missingFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"workos-wif-missing-{Guid.NewGuid():N}.jwt");
            using var fileCredentials = Wif.Credentials(Wif.Options(IdentityTokenProvider.TokenFile(missingFile)), new TokenEndpoint());
            var fileError = await Assert.ThrowsAsync<WorkloadIdentityException>(() => fileCredentials.ResolveAsync(CancellationToken.None));
            Assert.Equal("provider_failed", fileError.Code);

            using var envCredentials = Wif.Credentials(Wif.Options(IdentityTokenProvider.TokenEnv("WORKOS_WIF_TEST_UNSET_VARIABLE")), new TokenEndpoint());
            var envError = await Assert.ThrowsAsync<WorkloadIdentityException>(() => envCredentials.ResolveAsync(CancellationToken.None));
            Assert.Equal("provider_failed", envError.Code);
            Assert.Contains("WORKOS_WIF_TEST_UNSET_VARIABLE", envError.InnerException!.Message);
        }

        [Theory]
        [InlineData(200, "not json")]
        [InlineData(200, "[1, 2]")]
        [InlineData(200, "{\"token_type\":\"Bearer\",\"expires_in\":300}")]
        [InlineData(200, "{\"access_token\":\"\",\"expires_in\":300}")]
        [InlineData(201, "{\"access_token\":\"access-1\"}")]
        [InlineData(404, "Not Found")]
        public async Task AnUnreadableResponseIsInvalidResponseAndIsNotRetried(int status, string body)
        {
            var token = new TokenEndpoint((_, _, _) => new HttpResponseMessage((HttpStatusCode)status) { Content = new StringContent(body) });
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => credentials.ResolveAsync(CancellationToken.None));

            Assert.Equal("invalid_response", error.Code);
            Assert.Equal(1, token.ExchangeCount);
        }

        [Theory]
        [InlineData("\"soon\"")]
        [InlineData("0")]
        [InlineData("-30")]
        [InlineData("null")]
        public async Task AnUnusableExpiresInIsTreatedAs300Seconds(string expiresIn)
        {
            var issued = 0;
            var token = new TokenEndpoint((_, _, _) => Wif.Json(
                HttpStatusCode.OK,
                $"{{\"access_token\":\"access-{++issued}\",\"expires_in\":{expiresIn}}}"));
            var clock = new FakeTimeProvider();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token, clock);

            await credentials.ResolveAsync(CancellationToken.None);
            clock.Advance(TimeSpan.FromSeconds(239));
            await credentials.ResolveAsync(CancellationToken.None);
            Assert.Equal(1, token.ExchangeCount);

            clock.Advance(TimeSpan.FromSeconds(2));
            await credentials.ResolveAsync(CancellationToken.None);
            Assert.Equal(2, token.ExchangeCount);
        }

        [Fact]
        public async Task TheDescriptionIsCutTo500Characters()
        {
            var token = new TokenEndpoint((_, _, _) => Wif.OAuthError("invalid_grant", new string('x', 800)));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => credentials.ResolveAsync(CancellationToken.None));

            Assert.Equal(new string('x', 500), error.Description);
            Assert.EndsWith(" " + new string('x', 500), error.Message);
        }

        [Theory]
        [InlineData("ClientId", "WORKOS_CLIENT_ID")]
        [InlineData("TokenUrl", "WORKOS_WIF_TOKEN_URL")]
        [InlineData("EnvironmentId", "WORKOS_ENVIRONMENT_ID")]
        public void AMissingRequiredSettingNamesTheFieldAndItsVariable(string field, string variable)
        {
            var options = Wif.Options(new CountingProvider("assertion-1"));
            typeof(WorkloadIdentityCredentialsOptions).GetProperty(field)!.SetValue(options, null);

            var error = Assert.Throws<WorkloadIdentityException>(() => new WorkloadIdentityCredentials(options));

            Assert.Equal("invalid_configuration", error.Code);
            Assert.Equal($"{field} is required (or set {variable})", error.Description);
            Assert.Equal("Workload identity credentials are missing a required setting.", error.Hint);
        }

        [Fact]
        public void AMissingAmbiguousOrMalformedSettingIsAConfigurationError()
        {
            var noProvider = Wif.Options(new CountingProvider("assertion-1"));
            noProvider.IdentityTokenProvider = null;
            Assert.Equal("invalid_configuration", Assert.Throws<WorkloadIdentityException>(() => new WorkloadIdentityCredentials(noProvider)).Code);

            var twoProviders = Wif.Options(new CountingProvider("assertion-1"));
            twoProviders.IdentityTokenCallback = _ => Task.FromResult("assertion-cb");
            Assert.Equal("invalid_configuration", Assert.Throws<WorkloadIdentityException>(() => new WorkloadIdentityCredentials(twoProviders)).Code);

            var relativeUrl = Wif.Options(new CountingProvider("assertion-1"));
            relativeUrl.TokenUrl = "/oauth2/token";
            Assert.Equal("invalid_configuration", Assert.Throws<WorkloadIdentityException>(() => new WorkloadIdentityCredentials(relativeUrl)).Code);

            Assert.Throws<ArgumentNullException>(() => new WorkloadIdentityCredentials(null!));
        }

        [Fact]
        public async Task AnExchangeThatOutlivesItsDeadlineFailsEveryWaiterAndTheNextCallStartsAfresh()
        {
            var reached = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var token = TokenEndpoint.Async(async (endpoint, number, _, exchangeToken) =>
            {
                if (number == 1)
                {
                    // Never answers; only the exchange's own deadline ends it.
                    reached.SetResult(true);
                    await Task.Delay(Timeout.InfiniteTimeSpan, exchangeToken);
                }

                return endpoint.Success();
            });
            var clock = new FakeTimeProvider();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token, clock);

            var first = credentials.ResolveAsync(CancellationToken.None);
            var second = credentials.ResolveAsync(CancellationToken.None);
            await reached.Task.WaitAsync(TimeSpan.FromSeconds(10));

            clock.Advance(TimeSpan.FromSeconds(31));

            // Not a cancellation: the callers did not cancel anything.
            var firstError = await Assert.ThrowsAsync<WorkloadIdentityException>(() => first.WaitAsync(TimeSpan.FromSeconds(10)));
            var secondError = await Assert.ThrowsAsync<WorkloadIdentityException>(() => second.WaitAsync(TimeSpan.FromSeconds(10)));
            Assert.Equal("exchange_failed", firstError.Code);
            Assert.Same(firstError, secondError);

            // The failure is not cached, and no in-flight state is left behind.
            var snapshot = await credentials.ResolveAsync(CancellationToken.None).WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal("Bearer access-1", snapshot.Headers["Authorization"]);
            Assert.Equal(2, token.ExchangeCount);
        }

        [Fact]
        public void TheCredentialsContractDoesNotShadowSystemNetICredentials()
        {
            // This file imports System.Net and WorkOS, as a caller that sets proxy credentials
            // does. If the SDK declared a type named ICredentials, the unqualified name below
            // would be ambiguous (CS0104) and this file, like that caller's, would not compile.
            ICredentials proxyCredentials = CredentialCache.DefaultCredentials;
            IWorkOSCredentials credentials = new FixedHeaderCredentials("Bearer fixed");

            Assert.NotNull(proxyCredentials);
            Assert.NotNull(credentials);
        }

        /// <summary>A caller's own credentials that return one fixed Authorization value.</summary>
        private sealed class FixedHeaderCredentials : IWorkOSCredentials
        {
            private readonly string authorization;

            internal FixedHeaderCredentials(string authorization) => this.authorization = authorization;

            public Task<CredentialSnapshot> ResolveAsync(CancellationToken cancellationToken)
            {
                var headers = new Dictionary<string, string> { ["Authorization"] = this.authorization };
                return Task.FromResult(new CredentialSnapshot(headers, generation: 1, recoverable: false));
            }

            public void Invalidate(long generation)
            {
            }
        }

        /// <summary>
        /// A caller's own credentials: a new bearer value after each invalidation, plus a
        /// header of its own.
        /// </summary>
        private sealed class SequenceCredentials : IWorkOSCredentials
        {
            private readonly List<long> invalidated = new List<long>();
            private long generation = 1;

            internal IReadOnlyList<long> Invalidated => this.invalidated;

            public Task<CredentialSnapshot> ResolveAsync(CancellationToken cancellationToken)
            {
                var headers = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer custom-{this.generation}",
                    ["x-custom-tenant"] = "tenant-a",
                };
                return Task.FromResult(new CredentialSnapshot(headers, this.generation, recoverable: true));
            }

            public void Invalidate(long generation)
            {
                this.invalidated.Add(generation);
                if (generation == this.generation)
                {
                    this.generation++;
                }
            }
        }
    }
}
