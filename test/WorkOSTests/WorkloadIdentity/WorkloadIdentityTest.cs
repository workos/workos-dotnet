// @oagen-ignore-file
#nullable enable
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Time.Testing;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// The core workload identity conformance cases, WIF-01 to WIF-24. Each case ID is in the
    /// name of the test that covers it. Everything runs against a mock token endpoint and a
    /// mock API; no test reaches the network or sleeps on the real clock for a token.
    /// </summary>
    [Collection(WorkloadIdentityCollection.Name)]
    public class WorkloadIdentityTest : IDisposable
    {
        private const string Bearer1 = "Bearer access-1";
        private const string Bearer2 = "Bearer access-2";

        private readonly EnvironmentScope environment = new EnvironmentScope();

        public WorkloadIdentityTest()
        {
            // Every case starts from an environment with none of the SDK's variables set, so
            // a developer's shell cannot change what a case sends.
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
        public async Task WIF_01_FirstRequestSendsOneExchangePostWithTheRightShape()
        {
            var token = new TokenEndpoint();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, Wif.Api());

            await client.MakeRawAPIRequest(Wif.Get());

            var exchange = Assert.Single(token.Requests);
            Assert.Equal(HttpMethod.Post, exchange.Method);
            Assert.Equal(Wif.TokenUrl, exchange.Uri.ToString());
            Assert.Equal("application/x-www-form-urlencoded", exchange.ContentType);
            Assert.Equal("urn:ietf:params:oauth:grant-type:jwt-bearer", exchange.Form["grant_type"]);
            Assert.Equal("assertion-1", exchange.Form["assertion"]);
            Assert.Equal("client_wif_test", exchange.Form["client_id"]);
            Assert.Equal("https://api.workos.com", exchange.Form["resource"]);
            Assert.DoesNotContain("scope", exchange.Form.AllKeys);
            Assert.DoesNotContain("client_secret", exchange.Form.AllKeys);
            Assert.False(exchange.Headers.ContainsKey("Authorization"));
            Assert.Contains("resource=https%3A%2F%2Fapi.workos.com", exchange.Body);

            // The same, with a scope, on a new instance.
            var scopedToken = new TokenEndpoint();
            var scopedOptions = Wif.Options(new CountingProvider("assertion-1"));
            scopedOptions.Scope = "organizations:write";
            using var scopedCredentials = Wif.Credentials(scopedOptions, scopedToken);
            using var scopedClient = Wif.Client(scopedCredentials, Wif.Api());

            await scopedClient.MakeRawAPIRequest(Wif.Get());

            var scopedExchange = Assert.Single(scopedToken.Requests);
            Assert.Equal("organizations:write", scopedExchange.Form["scope"]);
            Assert.DoesNotContain("client_secret", scopedExchange.Form.AllKeys);
            Assert.False(scopedExchange.Headers.ContainsKey("Authorization"));
        }

        [Fact]
        public async Task WIF_02_ApiRequestCarriesTheAccessTokenAndTheEnvironmentHeader()
        {
            var api = Wif.Api();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), new TokenEndpoint());
            using var client = Wif.Client(credentials, api);

            await client.MakeRawAPIRequest(Wif.Get());

            var request = Assert.Single(api.Requests);
            Assert.Equal(Bearer1, request.Headers["Authorization"]);
            Assert.Equal("environment_wif_test", request.Headers[Wif.EnvironmentHeader]);
        }

        [Fact]
        public async Task WIF_03_SecondRequestWithinTheLifetimeReusesTheCachedToken()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token, new FakeTimeProvider());
            using var client = Wif.Client(credentials, api);

            await client.MakeRawAPIRequest(Wif.Get());
            await client.MakeRawAPIRequest(Wif.Get());

            Assert.Equal(1, token.ExchangeCount);
            Assert.Equal(2, api.Count);
            Assert.All(api.Requests, request => Assert.Equal(Bearer1, request.Headers["Authorization"]));
        }

        [Fact]
        public async Task WIF_04_TokenWith300SecondLifetimeIsRefreshedWhenLessThan60SecondsRemain()
        {
            var token = new TokenEndpoint((endpoint, _, _) => endpoint.Success(expiresIn: 300));
            var api = Wif.Api();
            var clock = new FakeTimeProvider();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token, clock);
            using var client = Wif.Client(credentials, api);

            await client.MakeRawAPIRequest(Wif.Get());

            clock.Advance(TimeSpan.FromSeconds(239));
            await client.MakeRawAPIRequest(Wif.Get());
            Assert.Equal(1, token.ExchangeCount);

            clock.Advance(TimeSpan.FromSeconds(2));
            await client.MakeRawAPIRequest(Wif.Get());
            Assert.Equal(2, token.ExchangeCount);
            Assert.Equal(Bearer2, api.Requests[2].Headers["Authorization"]);
        }

        [Fact]
        public async Task WIF_05_TokenWith100SecondLifetimeIsRefreshedAt20SecondsNot60()
        {
            var token = new TokenEndpoint((endpoint, _, _) => endpoint.Success(expiresIn: 100));
            var clock = new FakeTimeProvider();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token, clock);
            using var client = Wif.Client(credentials, Wif.Api());

            await client.MakeRawAPIRequest(Wif.Get());

            clock.Advance(TimeSpan.FromSeconds(79));
            await client.MakeRawAPIRequest(Wif.Get());
            Assert.Equal(1, token.ExchangeCount);

            clock.Advance(TimeSpan.FromSeconds(2));
            await client.MakeRawAPIRequest(Wif.Get());
            Assert.Equal(2, token.ExchangeCount);
        }

        [Fact]
        public async Task WIF_06_ResponseWithoutExpiresInIsTreatedAs300Seconds()
        {
            var token = new TokenEndpoint((endpoint, _, _) => endpoint.Success(expiresIn: null));
            var clock = new FakeTimeProvider();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token, clock);
            using var client = Wif.Client(credentials, Wif.Api());

            await client.MakeRawAPIRequest(Wif.Get());

            clock.Advance(TimeSpan.FromSeconds(239));
            await client.MakeRawAPIRequest(Wif.Get());
            Assert.Equal(1, token.ExchangeCount);

            clock.Advance(TimeSpan.FromSeconds(2));
            await client.MakeRawAPIRequest(Wif.Get());
            Assert.Equal(2, token.ExchangeCount);
        }

        [Fact]
        public async Task WIF_07_TwoCredentialsInstancesNeverShareACachedToken()
        {
            var token = new TokenEndpoint();
            var firstApi = Wif.Api();
            var secondApi = Wif.Api();

            // Identical configuration, two instances, each on its own client.
            using var first = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var second = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var firstClient = Wif.Client(first, firstApi);
            using var secondClient = Wif.Client(second, secondApi);

            await firstClient.MakeRawAPIRequest(Wif.Get());
            await secondClient.MakeRawAPIRequest(Wif.Get());

            Assert.Equal(2, token.ExchangeCount);
            Assert.Equal(Bearer1, Assert.Single(firstApi.Requests).Headers["Authorization"]);
            Assert.Equal(Bearer2, Assert.Single(secondApi.Requests).Headers["Authorization"]);
        }

        [Fact]
        public async Task WIF_08_ConcurrentFirstRequestsOnOneInstanceProduceExactlyOneExchange()
        {
            const int Callers = 20;
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var token = TokenEndpoint.Async(async (endpoint, _, _, exchangeToken) =>
            {
                // Hold the response until every caller is waiting.
                await release.Task.WaitAsync(exchangeToken);
                return endpoint.Success();
            });
            var api = Wif.Api();
            var provider = new CountingProvider("assertion-1");
            using var credentials = Wif.Credentials(Wif.Options(provider), token);
            var observed = new ObservedCredentials(credentials, Callers);
            using var client = Wif.Client(observed, api);

            var start = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var requests = Enumerable.Range(0, Callers)
                .Select(_ => Task.Run(async () =>
                {
                    await start.Task;
                    return await client.MakeRawAPIRequest(Wif.Get());
                }))
                .ToArray();

            start.SetResult(true);
            await observed.AllWaiting.WaitAsync(TimeSpan.FromSeconds(10));
            release.SetResult(true);
            var responses = await Task.WhenAll(requests).WaitAsync(TimeSpan.FromSeconds(10));

            Assert.Equal(1, token.ExchangeCount);
            Assert.Equal(1, provider.Count);
            Assert.All(responses, response => Assert.Equal(HttpStatusCode.OK, response.StatusCode));
            Assert.Equal(Callers, api.Count);
            Assert.All(api.Requests, request => Assert.Equal(Bearer1, request.Headers["Authorization"]));
        }

        [Fact]
        public async Task WIF_09_A401InvalidatesThatGenerationExchangesAgainAndRetriesOnce()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api(number => number == 1
                ? Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}")
                : Wif.Json(HttpStatusCode.OK, "{}"));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api);

            var response = await client.MakeRawAPIRequest(Wif.Get());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, token.ExchangeCount);
            Assert.Equal(2, api.Count);
            Assert.Equal(Bearer1, api.Requests[0].Headers["Authorization"]);
            Assert.Equal(Bearer2, api.Requests[1].Headers["Authorization"]);
        }

        [Fact]
        public async Task WIF_10_ASecond401AfterRecoveryIsReturnedToTheCaller()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api(_ => Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api);

            await Assert.ThrowsAsync<AuthenticationException>(() => client.MakeRawAPIRequest(Wif.Get()));

            Assert.Equal(2, token.ExchangeCount);
            Assert.Equal(2, api.Count);
        }

        [Fact]
        public async Task WIF_11_ALate401ForAnOldGenerationDoesNotDiscardANewerToken()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api);

            var first = await credentials.ResolveAsync(CancellationToken.None);
            Assert.Equal(Bearer1, first.Headers["Authorization"]);

            credentials.Invalidate(first.Generation);
            var second = await credentials.ResolveAsync(CancellationToken.None);
            Assert.Equal(Bearer2, second.Headers["Authorization"]);
            Assert.NotEqual(first.Generation, second.Generation);

            // A late 401 for the first generation arrives after the second is in place.
            credentials.Invalidate(first.Generation);
            await client.MakeRawAPIRequest(Wif.Get());

            var current = await credentials.ResolveAsync(CancellationToken.None);
            Assert.Equal(second.Generation, current.Generation);
            Assert.Equal(2, token.ExchangeCount);
            Assert.Equal(Bearer2, Assert.Single(api.Requests).Headers["Authorization"]);
        }

        [Fact]
        public async Task WIF_12_TransportRetriesDoNotResetTheSingle401Recovery()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api(number => number switch
            {
                1 => Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"),
                2 => Wif.Status(HttpStatusCode.ServiceUnavailable, retryAfter: "0"),
                3 => Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"),
                _ => Wif.Json(HttpStatusCode.OK, "{}"),
            });
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api);

            await Assert.ThrowsAsync<AuthenticationException>(() => client.MakeRawAPIRequest(Wif.Get()));

            Assert.Equal(2, token.ExchangeCount);
            Assert.Equal(3, api.Count);
            Assert.Equal(Bearer1, api.Requests[0].Headers["Authorization"]);
            Assert.Equal(Bearer2, api.Requests[1].Headers["Authorization"]);
            Assert.Equal(Bearer2, api.Requests[2].Headers["Authorization"]);
        }

        [Fact]
        public async Task WIF_13_The401RetrySendsTheSameBodyAndTheSameIdempotencyKey()
        {
            var api = Wif.Api(number => number == 1
                ? Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}")
                : Wif.Json(HttpStatusCode.OK, "{}"));
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), new TokenEndpoint());
            using var client = Wif.Client(credentials, api);

            // A POST with no explicit key: the SDK generates one for the logical request.
            await client.MakeRawAPIRequest(new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = "/widgets",
                Options = new WidgetOptions { Name = "Widget" },
            });

            Assert.Equal(2, api.Count);
            var first = api.Requests[0];
            var second = api.Requests[1];
            Assert.NotEmpty(first.BodyBytes);
            Assert.Equal(first.BodyBytes, second.BodyBytes);
            Assert.False(string.IsNullOrEmpty(first.Headers["Idempotency-Key"]));
            Assert.Equal(first.Headers["Idempotency-Key"], second.Headers["Idempotency-Key"]);
            Assert.Equal(Bearer1, first.Headers["Authorization"]);
            Assert.Equal(Bearer2, second.Headers["Authorization"]);
        }

        [Fact]
        public async Task WIF_14_ExchangeIsRetriedOnConnectionFailure429And5xxHonoringRetryAfter()
        {
            // (a) 503, then 429 with Retry-After: 0, then 200.
            var statusToken = new TokenEndpoint((endpoint, number, _) => number switch
            {
                1 => Wif.Status(HttpStatusCode.ServiceUnavailable),
                2 => Wif.Status(HttpStatusCode.TooManyRequests, retryAfter: "0"),
                _ => endpoint.Success(),
            });
            var statusProvider = new CountingProvider("assertion-1");
            using (var credentials = Wif.Credentials(Wif.Options(statusProvider), statusToken))
            using (var client = Wif.Client(credentials, Wif.Api()))
            {
                var response = await client.MakeRawAPIRequest(Wif.Get());

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(3, statusToken.ExchangeCount);
                Assert.Equal(3, statusProvider.Count);
            }

            // (b) The first attempt fails to connect; the second returns 200.
            var connectToken = new TokenEndpoint((endpoint, number, _) => number == 1
                ? throw new HttpRequestException("Connection refused")
                : endpoint.Success());
            var connectProvider = new CountingProvider("assertion-1");
            using (var credentials = Wif.Credentials(Wif.Options(connectProvider), connectToken))
            using (var client = Wif.Client(credentials, Wif.Api()))
            {
                var response = await client.MakeRawAPIRequest(Wif.Get());

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(2, connectProvider.Count);
            }

            // (c) The provider raises on its first call and returns assertion-2 on its second.
            var providerToken = new TokenEndpoint();
            var failingProvider = new CountingProvider(number => number == 1
                ? throw new InvalidOperationException("The token source is unavailable.")
                : "assertion-2");
            using (var credentials = Wif.Credentials(Wif.Options(failingProvider), providerToken))
            using (var client = Wif.Client(credentials, Wif.Api()))
            {
                var response = await client.MakeRawAPIRequest(Wif.Get());

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(2, failingProvider.Count);
                Assert.Equal(1, providerToken.ExchangeCount);
                Assert.Equal("assertion-2", Assert.Single(providerToken.Requests).Form["assertion"]);
            }

            // Fourth run: 429 with Retry-After: 1, then 200, with an injected sleeper.
            var waitToken = new TokenEndpoint((endpoint, number, _) => number == 1
                ? Wif.Status(HttpStatusCode.TooManyRequests, retryAfter: "1")
                : endpoint.Success());
            var sleeper = new SleepRecorder();
            using (var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), waitToken, sleeper: sleeper))
            using (var client = Wif.Client(credentials, Wif.Api()))
            {
                var response = await client.MakeRawAPIRequest(Wif.Get());

                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                Assert.Equal(2, waitToken.ExchangeCount);
                var delay = Assert.Single(sleeper.Delays);
                Assert.True(delay >= TimeSpan.FromSeconds(1), $"The sleeper was asked to wait {delay}, less than Retry-After.");
            }
        }

        [Theory]
        [InlineData("invalid_grant")]
        [InlineData("invalid_target")]
        [InlineData("invalid_client")]
        [InlineData("unauthorized_client")]
        [InlineData("invalid_scope")]
        [InlineData("invalid_request")]
        public async Task WIF_15_AnOAuthErrorIsNotRetriedAndSurfacesAsATypedError(string code)
        {
            var description = $"The server said {code}.";
            var token = new TokenEndpoint((_, _, _) => Wif.OAuthError(code, description));
            var api = Wif.Api();
            var provider = new CountingProvider("assertion-1");
            using var credentials = Wif.Credentials(Wif.Options(provider), token);
            using var client = Wif.Client(credentials, api);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => client.MakeRawAPIRequest(Wif.Get()));

            Assert.Equal(code, error.Code);
            Assert.Equal(description, error.Description);
            Assert.False(string.IsNullOrWhiteSpace(error.Hint));
            Assert.StartsWith($"{code}: {error.Hint}", error.Message);
            Assert.Equal(1, token.ExchangeCount);
            Assert.Equal(1, provider.Count);
            Assert.Equal(0, api.Count);
        }

        [Fact]
        public async Task WIF_16_AfterAFailedExchangeTheNextRequestAttemptsAFreshExchange()
        {
            var token = new TokenEndpoint((endpoint, number, _) => number == 1
                ? Wif.OAuthError("invalid_grant", "The token was rejected.")
                : endpoint.Success());
            var api = Wif.Api();
            using var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), token);
            using var client = Wif.Client(credentials, api);

            await Assert.ThrowsAsync<WorkloadIdentityException>(() => client.MakeRawAPIRequest(Wif.Get()));
            var response = await client.MakeRawAPIRequest(Wif.Get());

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, token.ExchangeCount);
            Assert.Equal(Bearer1, Assert.Single(api.Requests).Headers["Authorization"]);
        }

        [Fact]
        public async Task WIF_17_NeitherTokenAppearsInAnyErrorMessageOrLogLine()
        {
            const string Assertion = "SECRET-ASSERTION-VALUE";
            const string AccessToken = "SECRET-ACCESS-VALUE";

            // The SDK has no logger. Trace and Debug output is the nearest thing to SDK
            // logging in .NET, so it is captured at every level for the whole case.
            var log = new CapturingTraceListener();
            Trace.Listeners.Add(log);
            var errors = new List<Exception>();
            try
            {
                // An invalid_grant exchange. The server echoes the assertion in its
                // description, which is the worst case for the error message.
                var rejected = new TokenEndpoint((_, _, _) => Wif.OAuthError("invalid_grant", $"No trust rule matched {Assertion}."));
                errors.Add(await FailureAsync(Wif.Options(new CountingProvider(Assertion)), rejected, Wif.Api()));

                // A 500 from the token endpoint until the retries run out.
                var failing = new TokenEndpoint((_, _, _) => Wif.OAuthError("server_error", $"Could not process {Assertion}.", HttpStatusCode.InternalServerError));
                errors.Add(await FailureAsync(Wif.Options(new CountingProvider(Assertion)), failing, Wif.Api()));
                Assert.Equal(3, failing.ExchangeCount);

                // A provider that raises.
                var raising = new CountingProvider(_ => throw new InvalidOperationException("The token source is unavailable."));
                errors.Add(await FailureAsync(Wif.Options(raising), new TokenEndpoint(), Wif.Api()));

                // A second 401 from the API, with the access token in play.
                var issuing = new TokenEndpoint((endpoint, _, _) => endpoint.Success(accessToken: AccessToken));
                var unauthorized = Wif.Api(_ => Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"));
                errors.Add(await FailureAsync(Wif.Options(new CountingProvider(Assertion)), issuing, unauthorized));
                Assert.All(unauthorized.Requests, request => Assert.Equal($"Bearer {AccessToken}", request.Headers["Authorization"]));
            }
            finally
            {
                Trace.Listeners.Remove(log);
            }

            Assert.Equal(4, errors.Count);
            Assert.IsType<WorkloadIdentityException>(errors[0]);
            Assert.IsType<WorkloadIdentityException>(errors[1]);
            Assert.IsType<WorkloadIdentityException>(errors[2]);
            Assert.IsType<AuthenticationException>(errors[3]);

            foreach (var error in errors)
            {
                // Message, then the full string form, which includes inner errors and stack traces.
                var texts = new List<string> { error.Message, error.ToString() };
                if (error is WorkloadIdentityException identityError)
                {
                    texts.Add(identityError.Code);
                    texts.Add(identityError.Description);
                    texts.Add(identityError.Hint);
                }

                foreach (var text in texts)
                {
                    Assert.DoesNotContain(Assertion, text);
                    Assert.DoesNotContain(AccessToken, text);
                }
            }

            Assert.DoesNotContain(Assertion, log.Text);
            Assert.DoesNotContain(AccessToken, log.Text);
        }

        [Fact]
        public async Task WIF_18_TheExchangeRequestDoesNotFollowRedirects()
        {
            // Real loopback servers and the real handler: a stubbed handler never follows a
            // redirect, so it could not show that the SDK refuses to.
            using var second = new LoopbackHttpServer(_ => new LoopbackResponse(200, "{\"access_token\":\"access-from-redirect\",\"expires_in\":300}"));
            using var first = new LoopbackHttpServer(_ => LoopbackResponse.Redirect(307, second.BaseUrl + "/oauth2/token"));

            var options = Wif.Options(new CountingProvider("assertion-1"));
            options.TokenUrl = first.BaseUrl + "/oauth2/token";
            using var credentials = new WorkloadIdentityCredentials(options);
            var api = Wif.Api();
            using var client = Wif.Client(credentials, api);

            var error = await Assert.ThrowsAsync<WorkloadIdentityException>(() => client.MakeRawAPIRequest(Wif.Get()));

            Assert.Equal("exchange_failed", error.Code);
            Assert.Empty(second.Requests);
            var post = Assert.Single(first.Requests);
            Assert.Equal("POST", post.Method);
            Assert.Equal("/oauth2/token", post.Path);
            Assert.Equal(0, api.Count);
        }

        [Fact]
        public void WIF_19_AnApiKeyTogetherWithCredentialsIsAConfigurationError()
        {
            var token = new TokenEndpoint();
            var api = Wif.Api();
            var provider = new CountingProvider("assertion-1");
            using var credentials = Wif.Credentials(Wif.Options(provider), token);

            var error = Assert.Throws<ArgumentException>(() => new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test_explicit",
                Credentials = credentials,
                HttpClient = new HttpClient(api),
            }));

            Assert.Contains("ApiKey", error.Message);
            Assert.Contains("Credentials", error.Message);

            // No HTTP request is sent: not to the API, and not to the token endpoint.
            Assert.Equal(0, api.Count);
            Assert.Equal(0, token.ExchangeCount);
            Assert.Equal(0, provider.Count);
        }

        [Fact]
        public async Task WIF_20_CredentialsWinOverTheEnvironmentKeyAndAFailingConfigurationNeverFallsBack()
        {
            this.environment.Set("WORKOS_API_KEY", "sk_test_env");

            // (a) Valid credentials: the API sees the access token, never the environment key.
            var api = Wif.Api();
            using (var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), new TokenEndpoint()))
            using (var client = Wif.Client(credentials, api))
            {
                await client.MakeRawAPIRequest(Wif.Get());
            }

            Assert.Equal(Bearer1, Assert.Single(api.Requests).Headers["Authorization"]);

            // (b) No token URL, explicitly or in the environment: constructing is an error.
            var incomplete = Wif.Options(new CountingProvider("assertion-1"));
            incomplete.TokenUrl = null;
            var configurationError = Assert.Throws<WorkloadIdentityException>(() => new WorkloadIdentityCredentials(incomplete));
            Assert.Equal("invalid_configuration", configurationError.Code);
            Assert.Contains("WORKOS_WIF_TOKEN_URL", configurationError.Description);

            // (c) Valid credentials whose exchange fails: the request fails; no fallback.
            var failingApi = Wif.Api();
            var rejecting = new TokenEndpoint((_, _, _) => Wif.OAuthError("invalid_client", "Unknown client."));
            using (var credentials = Wif.Credentials(Wif.Options(new CountingProvider("assertion-1")), rejecting))
            using (var client = Wif.Client(credentials, failingApi))
            {
                var exchangeError = await Assert.ThrowsAsync<WorkloadIdentityException>(() => client.MakeRawAPIRequest(Wif.Get()));
                Assert.Equal("invalid_client", exchangeError.Code);
            }

            Assert.Equal(0, failingApi.Count);
            foreach (var request in api.Requests.Concat(failingApi.Requests))
            {
                Assert.DoesNotContain("sk_test_env", request.Headers["Authorization"]);
            }
        }

        [Fact]
        public async Task WIF_21_TokenFileIsReadAgainOnEveryExchange()
        {
            var path = Path.Combine(Path.GetTempPath(), $"workos-wif-{Guid.NewGuid():N}.jwt");
            try
            {
                await File.WriteAllTextAsync(path, "assertion-1\n");
                var token = new TokenEndpoint();
                var clock = new FakeTimeProvider();
                var options = Wif.Options(IdentityTokenProvider.TokenFile(path));
                using var credentials = Wif.Credentials(options, token, clock);
                using var client = Wif.Client(credentials, Wif.Api());

                await client.MakeRawAPIRequest(Wif.Get());

                // Rotate the file, then force a second exchange by passing the refresh threshold.
                await File.WriteAllTextAsync(path, "  assertion-2\n");
                clock.Advance(TimeSpan.FromSeconds(241));
                await client.MakeRawAPIRequest(Wif.Get());

                Assert.Equal(2, token.ExchangeCount);
                Assert.Equal("assertion-1", token.Requests[0].Form["assertion"]);
                Assert.Equal("assertion-2", token.Requests[1].Form["assertion"]);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Fact]
        public async Task WIF_22_TokenEnvAndCallbackSupplyTheAssertionAndOmittedFieldsDefaultFromTheEnvironment()
        {
            // (a) token_env reads the named variable.
            this.environment.Set("MY_JWT", "assertion-env");
            var envToken = new TokenEndpoint();
            var envOptions = new WorkloadIdentityCredentialsOptions
            {
                ClientId = Wif.ClientId,
                TokenUrl = Wif.TokenUrl,
                EnvironmentId = Wif.EnvironmentId,
                IdentityTokenProvider = IdentityTokenProvider.TokenEnv("MY_JWT"),
            };
            using (var credentials = Wif.Credentials(envOptions, envToken))
            using (var client = Wif.Client(credentials, Wif.Api()))
            {
                await client.MakeRawAPIRequest(Wif.Get());
            }

            Assert.Equal("assertion-env", Assert.Single(envToken.Requests).Form["assertion"]);

            // (b) A callback is a provider.
            var callbackToken = new TokenEndpoint();
            var callbackOptions = new WorkloadIdentityCredentialsOptions
            {
                ClientId = Wif.ClientId,
                TokenUrl = Wif.TokenUrl,
                EnvironmentId = Wif.EnvironmentId,
                IdentityTokenCallback = _ => Task.FromResult("assertion-cb"),
            };
            using (var credentials = Wif.Credentials(callbackOptions, callbackToken))
            using (var client = Wif.Client(credentials, Wif.Api()))
            {
                await client.MakeRawAPIRequest(Wif.Get());
            }

            Assert.Equal("assertion-cb", Assert.Single(callbackToken.Requests).Form["assertion"]);

            // (c) Only a provider is passed; everything else comes from the environment.
            const string EnvironmentTokenUrl = "https://env.workos.test/oauth2/token";
            this.environment
                .Set("WORKOS_CLIENT_ID", "client_env")
                .Set("WORKOS_WIF_TOKEN_URL", EnvironmentTokenUrl)
                .Set("WORKOS_WIF_SCOPE", "scope_env")
                .Set("WORKOS_ENVIRONMENT_ID", "environment_env");

            var defaultsToken = new TokenEndpoint();
            var defaultsApi = Wif.Api();
            var defaultsOptions = new WorkloadIdentityCredentialsOptions
            {
                IdentityTokenProvider = new CountingProvider("assertion-1"),
            };
            using (var credentials = Wif.Credentials(defaultsOptions, defaultsToken))
            using (var client = Wif.Client(credentials, defaultsApi))
            {
                await client.MakeRawAPIRequest(Wif.Get());
            }

            var defaultsExchange = Assert.Single(defaultsToken.Requests);
            Assert.Equal(EnvironmentTokenUrl, defaultsExchange.Uri.ToString());
            Assert.Equal("client_env", defaultsExchange.Form["client_id"]);
            Assert.Equal("scope_env", defaultsExchange.Form["scope"]);
            Assert.Equal("environment_env", Assert.Single(defaultsApi.Requests).Headers[Wif.EnvironmentHeader]);

            // (d) The same environment, with explicit values: the explicit values win.
            var explicitToken = new TokenEndpoint();
            var explicitApi = Wif.Api();
            var explicitOptions = new WorkloadIdentityCredentialsOptions
            {
                ClientId = "client_explicit",
                Scope = "scope_explicit",
                EnvironmentId = "environment_explicit",
                IdentityTokenProvider = new CountingProvider("assertion-1"),
            };
            using (var credentials = Wif.Credentials(explicitOptions, explicitToken))
            using (var client = Wif.Client(credentials, explicitApi))
            {
                await client.MakeRawAPIRequest(Wif.Get());
            }

            var explicitExchange = Assert.Single(explicitToken.Requests);
            Assert.Equal("client_explicit", explicitExchange.Form["client_id"]);
            Assert.Equal("scope_explicit", explicitExchange.Form["scope"]);
            Assert.Equal("environment_explicit", Assert.Single(explicitApi.Requests).Headers[Wif.EnvironmentHeader]);
        }

        [Fact]
        public async Task WIF_23_CancelingOneWaitingCallerDoesNotCancelTheSharedExchange()
        {
            var reached = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var exchangeWasCanceled = false;
            var token = TokenEndpoint.Async(async (endpoint, _, _, exchangeToken) =>
            {
                // Hold the response until the test releases it.
                reached.SetResult(true);
                await release.Task;
                exchangeWasCanceled = exchangeToken.IsCancellationRequested;
                return endpoint.Success();
            });
            var api = Wif.Api();
            var provider = new CountingProvider("assertion-1");
            using var credentials = Wif.Credentials(Wif.Options(provider), token);
            using var client = Wif.Client(credentials, api);
            using var cancelA = new CancellationTokenSource();

            var callerA = client.MakeRawAPIRequest(Wif.Get(), cancelA.Token);
            var callerB = client.MakeRawAPIRequest(Wif.Get());
            await reached.Task.WaitAsync(TimeSpan.FromSeconds(10));

            cancelA.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => callerA);

            release.SetResult(true);
            var response = await callerB.WaitAsync(TimeSpan.FromSeconds(10));

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.False(exchangeWasCanceled);
            Assert.Equal(1, token.ExchangeCount);
            Assert.Equal(1, provider.Count);
            Assert.Equal(Bearer1, Assert.Single(api.Requests).Headers["Authorization"]);
        }

        [Fact]
        public async Task WIF_24_StaticApiKeyClientsAreUnchanged()
        {
            // The WIF variables are set, and still nothing switches WIF on.
            this.environment
                .Set("WORKOS_CLIENT_ID", "client_env")
                .Set("WORKOS_WIF_TOKEN_URL", "https://env.workos.test/oauth2/token")
                .Set("WORKOS_ENVIRONMENT_ID", "environment_env");

            var api = Wif.Api(number => number == 1
                ? Wif.Json(HttpStatusCode.OK, "{}")
                : Wif.Json(HttpStatusCode.Unauthorized, "{\"message\":\"Unauthorized\"}"));
            using var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test_static",
                HttpClient = new HttpClient(api),
            });

            await client.MakeRawAPIRequest(Wif.Get());

            var request = Assert.Single(api.Requests);
            Assert.Equal("Bearer sk_test_static", request.Headers["Authorization"]);
            Assert.False(request.Headers.ContainsKey(Wif.EnvironmentHeader));

            // A 401 is returned after exactly one attempt. The API mock is the client's only
            // HTTP route, so two requests in total also means no exchange was attempted.
            await Assert.ThrowsAsync<AuthenticationException>(() => client.MakeRawAPIRequest(Wif.Get()));

            Assert.Equal(2, api.Count);
            Assert.Equal("Bearer sk_test_static", api.Requests[1].Headers["Authorization"]);
            Assert.False(api.Requests[1].Headers.ContainsKey(Wif.EnvironmentHeader));
        }

        private static async Task<Exception> FailureAsync(
            WorkloadIdentityCredentialsOptions options,
            TokenEndpoint token,
            RecordingHandler api)
        {
            using var credentials = Wif.Credentials(options, token);
            using var client = Wif.Client(credentials, api);
            var error = await Record.ExceptionAsync(() => client.MakeRawAPIRequest(Wif.Get()));
            Assert.NotNull(error);
            return error!;
        }

        private sealed class WidgetOptions : BaseOptions
        {
            [JsonProperty("name")]
            public string? Name { get; set; }
        }

        /// <summary>
        /// Passes every call through, and signals once the expected number of callers hold a
        /// pending resolve, which is the moment "all callers are waiting".
        /// </summary>
        private sealed class ObservedCredentials : IWorkOSCredentials
        {
            private readonly IWorkOSCredentials inner;
            private readonly int expected;
            private readonly TaskCompletionSource<bool> allWaiting = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private int resolving;

            internal ObservedCredentials(IWorkOSCredentials inner, int expected)
            {
                this.inner = inner;
                this.expected = expected;
            }

            internal Task AllWaiting => this.allWaiting.Task;

            public Task<CredentialSnapshot> ResolveAsync(CancellationToken cancellationToken)
            {
                var pending = this.inner.ResolveAsync(cancellationToken);
                if (Interlocked.Increment(ref this.resolving) == this.expected)
                {
                    this.allWaiting.TrySetResult(true);
                }

                return pending;
            }

            public void Invalidate(long generation) => this.inner.Invalidate(generation);
        }

        private sealed class CapturingTraceListener : TraceListener
        {
            private readonly StringBuilder text = new StringBuilder();

            internal string Text
            {
                get
                {
                    lock (this.text)
                    {
                        return this.text.ToString();
                    }
                }
            }

            public override void Write(string? message)
            {
                lock (this.text)
                {
                    this.text.Append(message);
                }
            }

            public override void WriteLine(string? message)
            {
                lock (this.text)
                {
                    this.text.AppendLine(message);
                }
            }
        }
    }

    /// <summary>
    /// The xunit collection every workload identity test class belongs to. Classes in one
    /// collection never run in parallel, which matters here because some cases set process
    /// environment variables that the others would read.
    /// </summary>
    [CollectionDefinition(Name)]
    public class WorkloadIdentityCollection
    {
        public const string Name = "Workload identity";
    }
}
