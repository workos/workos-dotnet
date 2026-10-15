// @oagen-ignore-file
#nullable enable
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Channels;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Time.Testing;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class FeatureFlagsRuntimeClientTest
    {
        private const string OrderedPayload = """
            {"version":2,"flags":{
              "ordered":{"slug":"ordered","enabled":true,"default_value":true,"off_value":false,"rules":[
                {"id":"r1","kind":"conditions","value":false,"conditions":[{"target_type":"organization","operator":"one_of","values":["org_acme"]}]},
                {"id":"r2","kind":"conditions","value":true,"conditions":[{"target_type":"user","operator":"one_of","values":["user_jp"]}]}
              ]},
              "disabled":{"slug":"disabled","enabled":false,"default_value":false,"off_value":true,"rules":[
                {"id":"r1","kind":"conditions","value":false,"conditions":[{"target_type":"organization","operator":"one_of","values":["org_acme"]}]}
              ]},
              "both":{"slug":"both","enabled":true,"default_value":false,"off_value":false,"rules":[
                {"id":"r1","kind":"conditions","value":true,"conditions":[
                  {"target_type":"organization","operator":"one_of","values":["org_acme"]},
                  {"target_type":"user","operator":"one_of","values":["user_jp"]}
                ]}
              ]},
              "custom":{"slug":"custom","enabled":true,"default_value":false,"off_value":false,"rules":[
                {"id":"r1","kind":"conditions","value":true,"conditions":[{"target_type":"project","operator":"one_of","values":["project_1"]}]}
              ]}
            }}
            """;

        // Served values that are not booleans, as a multivariate server would send them.
        private const string VariantPayload = """
            {"version":2,"flags":{
              "boolean":{"slug":"boolean","enabled":true,"default_value":true,"off_value":false,"rules":[]},
              "variant_default":{"slug":"variant_default","enabled":true,"default_value":"variant-a","off_value":false,"rules":[]},
              "variant_off":{"slug":"variant_off","enabled":false,"default_value":true,"off_value":"variant-off","rules":[]},
              "variant_rule":{"slug":"variant_rule","enabled":true,"default_value":false,"off_value":false,"rules":[
                {"id":"r1","kind":"conditions","value":"variant-b","conditions":[{"target_type":"organization","operator":"one_of","values":["org_acme"]}]}
              ]},
              "missing_default":{"slug":"missing_default","enabled":true,"off_value":false,"rules":[]}
            }}
            """;

        // Configuration sequences for change detection. The reordered base differs only in flag,
        // property and target ID order, none of which carries meaning.
        private const string ChangeBase = """
            {"version":2,"flags":{
              "alpha":{"slug":"alpha","enabled":true,"default_value":false,"off_value":false,"rules":[
                {"id":"r1","kind":"conditions","value":true,"conditions":[{"target_type":"organization","operator":"one_of","values":["org_a","org_b"]}]},
                {"id":"r2","kind":"conditions","value":false,"conditions":[{"target_type":"user","operator":"one_of","values":["user_x"]}]}
              ]},
              "beta":{"slug":"beta","enabled":true,"default_value":true,"off_value":false,"rules":[]},
              "gamma":{"slug":"gamma","enabled":false,"default_value":false,"off_value":false,"rules":[]}
            }}
            """;

        private const string ChangeBaseReordered = """
            {"version":2,"flags":{
              "gamma":{"slug":"gamma","enabled":false,"default_value":false,"off_value":false,"rules":[]},
              "beta":{"rules":[],"slug":"beta","off_value":false,"default_value":true,"enabled":true},
              "alpha":{"slug":"alpha","enabled":true,"default_value":false,"off_value":false,"rules":[
                {"id":"r1","kind":"conditions","value":true,"conditions":[{"target_type":"organization","operator":"one_of","values":["org_b","org_a"]}]},
                {"id":"r2","kind":"conditions","value":false,"conditions":[{"target_type":"user","operator":"one_of","values":["user_x"]}]}
              ]}
            }}
            """;

        private const string ChangeEdited = """
            {"version":2,"flags":{
              "alpha":{"slug":"alpha","enabled":true,"default_value":false,"off_value":false,"rules":[
                {"id":"r1","kind":"conditions","value":true,"conditions":[{"target_type":"organization","operator":"one_of","values":["org_a","org_b"]}]},
                {"id":"r2","kind":"conditions","value":false,"conditions":[{"target_type":"user","operator":"one_of","values":["user_x"]}]}
              ]},
              "beta":{"slug":"beta","enabled":true,"default_value":false,"off_value":false,"rules":[]},
              "delta":{"slug":"delta","enabled":true,"default_value":true,"off_value":false,"rules":[]}
            }}
            """;

        private const string ChangeRulesReordered = """
            {"version":2,"flags":{
              "alpha":{"slug":"alpha","enabled":true,"default_value":false,"off_value":false,"rules":[
                {"id":"r2","kind":"conditions","value":false,"conditions":[{"target_type":"user","operator":"one_of","values":["user_x"]}]},
                {"id":"r1","kind":"conditions","value":true,"conditions":[{"target_type":"organization","operator":"one_of","values":["org_a","org_b"]}]}
              ]},
              "beta":{"slug":"beta","enabled":true,"default_value":false,"off_value":false,"rules":[]},
              "delta":{"slug":"delta","enabled":true,"default_value":true,"off_value":false,"rules":[]}
            }}
            """;

        [Fact]
        public async Task BootstrapEvaluatesOrderedRulesOffValuesDefaultsAndAllConditionTypesLocally()
        {
            using var client = new WorkOSClient(new WorkOSOptions { ApiKey = "sk_test" });
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                BootstrapConfiguration = FeatureFlagsConfiguration.FromJson(OrderedPayload),
            });
            var context = new FeatureFlagContext(userId: "user_jp", organizationId: "org_acme");

            Assert.True(runtime.IsReady);
            Assert.Null(runtime.Status.LastSuccessfulRefresh);
            Assert.False(runtime.IsEnabled("ordered", context)); // First false rule beats the later true rule and true default.
            Assert.True(runtime.IsEnabled("disabled", context)); // Disabled serves off_value even if a rule matches.
            Assert.True(runtime.IsEnabled("ordered")); // No context uses the flag default.
            Assert.True(runtime.IsEnabled("ordered", new FeatureFlagContext(userId: "user_jp")));
            Assert.True(runtime.IsEnabled("both", context));
            Assert.False(runtime.IsEnabled("both", new FeatureFlagContext(organizationId: "org_acme")));
            Assert.False(runtime.IsEnabled("both", new FeatureFlagContext(userId: "user_jp")));
            Assert.False(runtime.IsEnabled("both", new FeatureFlagContext(userId: "USER_JP", organizationId: "org_acme")));
            Assert.False(runtime.IsEnabled("missing"));
            Assert.True(runtime.IsEnabled("missing", defaultValue: true));
            Assert.Equal(
                new Dictionary<string, bool>
                {
                    ["ordered"] = false,
                    ["disabled"] = true,
                    ["both"] = true,
                    ["custom"] = false,
                },
                runtime.GetAllFlags(context));

            var custom = new Dictionary<string, string> { ["project"] = "project_1" };
            var customContext = new FeatureFlagContext(customTargets: custom);
            custom["project"] = "different";
            Assert.True(runtime.IsEnabled("custom", customContext));
            Assert.False(runtime.IsEnabled("custom", new FeatureFlagContext(customTargets: custom)));
        }

        [Fact]
        public async Task NonBooleanServedValuesFallThroughToCallerDefaultWithoutFailingTheRefresh()
        {
            using var handler = new ScriptedHandler((_, _, _) => Task.FromResult(Response(VariantPayload)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient();
            await runtime.RefreshAsync();
            var organization = new FeatureFlagContext(organizationId: "org_acme");

            Assert.Null(runtime.Status.LastError);
            Assert.Equal(5, runtime.Configuration!.Count);
            Assert.True(runtime.IsEnabled("boolean"));
            Assert.False(runtime.IsEnabled("variant_default"));
            Assert.True(runtime.IsEnabled("variant_default", defaultValue: true));
            Assert.False(runtime.IsEnabled("variant_off"));
            Assert.True(runtime.IsEnabled("variant_off", defaultValue: true));
            Assert.False(runtime.IsEnabled("missing_default"));
            Assert.True(runtime.IsEnabled("missing_default", defaultValue: true));

            // Only a served non-boolean has no answer; an unmatched variant rule leaves the flag default intact.
            Assert.True(runtime.IsEnabled("variant_rule", organization, defaultValue: true));
            Assert.False(runtime.IsEnabled("variant_rule", organization, defaultValue: false));
            Assert.False(runtime.IsEnabled("variant_rule", defaultValue: true));

            Assert.Equal(new Dictionary<string, bool> { ["boolean"] = true }, runtime.GetAllFlags(organization));
            Assert.Equal(new Dictionary<string, bool> { ["boolean"] = true, ["variant_rule"] = false }, runtime.GetAllFlags());
            await using var restored = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                BootstrapConfiguration = FeatureFlagsConfiguration.FromJson(runtime.Configuration!.ToJson()),
            });
            Assert.Equal(runtime.GetAllFlags(), restored.GetAllFlags());
        }

        [Theory]
        [InlineData("{\"id\":\"r1\",\"kind\":\"percentage\",\"value\":true}")]
        [InlineData("{\"id\":\"r1\",\"kind\":\"conditions\",\"value\":true,\"conditions\":[]}")]
        [InlineData("{\"id\":\"r1\",\"kind\":\"conditions\",\"value\":true,\"conditions\":[{\"operator\":\"future\"}]}")]
        [InlineData("{\"id\":\"r1\",\"kind\":\"conditions\",\"value\":true,\"conditions\":[{\"operator\":\"one_of\",\"target_type\":\"organization\",\"values\":[]}]}")]
        public async Task UnknownAndEmptyRulesNeverMatchEveryone(string rule)
        {
            using var client = new WorkOSClient(new WorkOSOptions { ApiKey = "sk_test" });
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                BootstrapConfiguration = FeatureFlagsConfiguration.FromJson(SingleFlag(rule)),
            });
            Assert.False(runtime.IsEnabled("test"));
            Assert.False(runtime.IsEnabled("test", new FeatureFlagContext(organizationId: "org_acme")));
        }

        [Theory]
        [InlineData("not json")]
        [InlineData("{\"version\":1,\"flags\":{}}")]
        [InlineData("{\"version\":3,\"flags\":{}}")]
        [InlineData("{\"version\":2}")]
        [InlineData("{\"version\":2,\"version\":2,\"flags\":{}}")]
        [InlineData("{\"version\":2,\"flags\":{}}{}")]
        [InlineData("{\"version\":2,\"flags\":{\"test\":{\"slug\":\"other\",\"enabled\":true,\"default_value\":false,\"off_value\":false,\"rules\":[]}}}")]
        [InlineData("{\"version\":2,\"flags\":{\"test\":{\"slug\":\"test\",\"enabled\":\"true\",\"default_value\":false,\"off_value\":false,\"rules\":[]}}}")]
        [InlineData("{\"version\":2,\"flags\":{\"test\":{\"slug\":\"test\",\"enabled\":true,\"default_value\":false,\"off_value\":false,\"rules\":null}}}")]
        public async Task InvalidRefreshPreservesWholeSnapshotAndNextValidEmptyRefreshRemovesFlags(string invalid)
        {
            using var handler = new ScriptedHandler((attempt, _, _) => Task.FromResult(Response(
                attempt == 1 ? OrderedPayload : attempt == 2 ? invalid : "{\"version\":2,\"flags\":{}}")));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient();
            await runtime.RefreshAsync();
            var previous = runtime.Status;

            await Assert.ThrowsAnyAsync<JsonException>(() => runtime.RefreshAsync());
            Assert.Same(previous.Configuration, runtime.Configuration);
            Assert.Equal(previous.LastSuccessfulRefresh, runtime.Status.LastSuccessfulRefresh);
            Assert.IsAssignableFrom<JsonException>(runtime.Status.LastError);
            Assert.True(runtime.IsEnabled("ordered"));

            await runtime.RefreshAsync();
            Assert.True(runtime.IsReady);
            Assert.Empty(runtime.GetAllFlags());
            Assert.Null(runtime.Status.LastError);
        }

        [Fact]
        public async Task PollUsesSdkAuthenticationVersionQueryAndRecoversFromTransientFailure()
        {
            var time = new TestTimeProvider();
            using var handler = new ScriptedHandler((attempt, request, _) =>
            {
                Assert.Equal(HttpMethod.Get, request.Method);
                Assert.Equal("https://custom.example.com/sdk/feature-flags?payload_version=2", request.RequestUri!.AbsoluteUri);
                Assert.Equal("Bearer sk_test", request.Headers.Authorization!.ToString());
                Assert.Contains("workos-dotnet/", request.Headers.UserAgent.ToString());
                return Task.FromResult(attempt == 1
                    ? Response("{\"message\":\"temporary\"}", HttpStatusCode.InternalServerError)
                    : Response(OrderedPayload));
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var options = PollingOptions(time);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(options);
            var start = runtime.StartAsync();

            // One failure backs off by max(polling interval, 1s) with ten percent jitter.
            var retryDelay = await time.NextTimerAsync(due => due != options.RequestTimeout);
            Assert.InRange(retryDelay, TimeSpan.FromSeconds(4.5), TimeSpan.FromSeconds(5.5));
            Assert.Equal(1, handler.RequestCount);
            Assert.IsType<ServerException>(runtime.Status.LastError);

            time.Advance(retryDelay);
            await start.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(2, handler.RequestCount);
            Assert.True(runtime.IsEnabled("ordered"));
            Assert.Null(runtime.Status.LastError);
            Assert.Equal(time.GetUtcNow(), runtime.Status.LastSuccessfulRefresh);
            Assert.Equal(1, handler.MaxConcurrentRequests);
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task AuthenticationFailuresStopPollingAndFailReadiness(HttpStatusCode status)
        {
            var time = new TestTimeProvider();
            using var handler = new ScriptedHandler((_, _, _) => Task.FromResult(Response("{}", status)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(PollingOptions(time));
            var error = await Assert.ThrowsAnyAsync<ApiException>(() => runtime.StartAsync());
            Assert.Equal(status, error.StatusCode);

            // No retry is ever scheduled, so advancing the clock sends nothing.
            time.Advance(TimeSpan.FromMinutes(5));
            Assert.Equal(1, handler.RequestCount);
            Assert.False(runtime.IsReady);
            Assert.Same(error, runtime.Status.LastError);
            await Assert.ThrowsAnyAsync<ApiException>(() => runtime.WaitUntilReadyAsync());
        }

        [Fact]
        public async Task HttpFailureRetainsBootstrapAndCallerOwnedCacheCanRoundTrip()
        {
            var bootstrap = FeatureFlagsConfiguration.FromJson(OrderedPayload);
            using var handler = new ScriptedHandler((_, _, _) => Task.FromResult(Response("{}", HttpStatusCode.InternalServerError)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions { BootstrapConfiguration = bootstrap });
            Assert.True(runtime.IsEnabled("ordered"));
            await runtime.WaitUntilReadyAsync();
            await Assert.ThrowsAsync<ServerException>(() => runtime.RefreshAsync());
            Assert.Same(bootstrap, runtime.Configuration);
            Assert.True(runtime.IsReady);
            await using var restored = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                BootstrapConfiguration = FeatureFlagsConfiguration.FromJson(runtime.Configuration!.ToJson()),
            });
            Assert.Equal(runtime.GetAllFlags(), restored.GetAllFlags());
        }

        [Fact]
        public async Task RefreshTimeoutRetainsSnapshotAndReadinessWaitCanBeCancelled()
        {
            var time = new TestTimeProvider();
            var entered = Signal();
            using var handler = new ScriptedHandler(async (_, _, token) =>
            {
                entered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return Response(OrderedPayload);
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var options = PollingOptions(time);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(options);
            var refresh = runtime.RefreshAsync();
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));

            time.Advance(options.RequestTimeout);
            await Assert.ThrowsAsync<TimeoutException>(() => refresh).WaitAsync(TimeSpan.FromSeconds(5));
            Assert.IsType<TimeoutException>(runtime.Status.LastError);
            Assert.False(runtime.IsReady);
            Assert.True(runtime.IsEnabled("missing", defaultValue: true));
            using var cancellation = new CancellationTokenSource();
            var wait = runtime.WaitUntilReadyAsync(cancellation.Token);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => wait);
        }

        [Fact]
        public async Task QueuedRefreshTimesOutWithoutSendingOrChangingActiveRefreshStatus()
        {
            var time = new TestTimeProvider();
            var entered = Signal();
            var release = Signal();
            using var handler = new ScriptedHandler(async (_, _, _) =>
            {
                entered.TrySetResult();

                // A transport can take time to finish after cancellation. Keep the gate occupied.
                await release.Task;
                return Response(OrderedPayload);
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var bootstrap = FeatureFlagsConfiguration.FromJson(OrderedPayload);
            var options = new FeatureFlagsRuntimeOptions
            {
                BootstrapConfiguration = bootstrap,
                RequestTimeout = TimeSpan.FromMilliseconds(100),
                TimeProvider = time,
            };
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(options);
            var active = runtime.RefreshAsync();
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var queued = runtime.RefreshAsync();
            time.Advance(options.RequestTimeout);

            try
            {
                await Assert.ThrowsAsync<TimeoutException>(() => queued).WaitAsync(TimeSpan.FromSeconds(5));
                Assert.Equal(1, handler.RequestCount);
                Assert.Equal(1, handler.ActiveRequests);
                Assert.Same(bootstrap, runtime.Configuration);
                Assert.Null(runtime.Status.LastError);
            }
            finally
            {
                release.TrySetResult();
                await Assert.ThrowsAsync<TimeoutException>(() => active).WaitAsync(TimeSpan.FromSeconds(5));
            }

            Assert.IsType<TimeoutException>(runtime.Status.LastError);
        }

        [Fact]
        public async Task ConcurrentRefreshIsSerializedAndWaitingCancellationDoesNotChangeSnapshot()
        {
            var entered = Signal();
            var release = Signal();
            using var handler = new ScriptedHandler(async (attempt, _, token) =>
            {
                if (attempt == 1)
                {
                    entered.TrySetResult();
                    await release.Task.WaitAsync(token);
                }

                return Response(OrderedPayload);
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient();
            var first = runtime.RefreshAsync();
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            using var cancellation = new CancellationTokenSource();
            var waiting = runtime.RefreshAsync(cancellation.Token);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => waiting);
            Assert.Equal(1, handler.RequestCount);
            release.TrySetResult();
            await first;
            await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => runtime.RefreshAsync()));
            Assert.Equal(1, handler.MaxConcurrentRequests);
            Assert.True(runtime.IsReady);
            Assert.Null(runtime.Status.LastError);
        }

        [Fact]
        public async Task ConcurrentReadersSeeOneCompleteSnapshotWhileRefreshesReplaceIt()
        {
            const string truePayload = "{\"version\":2,\"flags\":{\"a\":{\"slug\":\"a\",\"enabled\":true,\"default_value\":true,\"off_value\":false,\"rules\":[]},\"b\":{\"slug\":\"b\",\"enabled\":true,\"default_value\":true,\"off_value\":false,\"rules\":[]}}}";
            var falsePayload = truePayload.Replace("\"default_value\":true", "\"default_value\":false", StringComparison.Ordinal);
            using var handler = new ScriptedHandler((attempt, _, _) => Task.FromResult(Response(attempt % 2 == 0 ? truePayload : falsePayload)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                BootstrapConfiguration = FeatureFlagsConfiguration.FromJson(truePayload),
            });
            var readers = Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
            {
                for (var i = 0; i < 1000; i++)
                {
                    var values = runtime.GetAllFlags();
                    Assert.Equal(2, values.Count);
                    Assert.Equal(values["a"], values["b"]);
                }
            })).ToArray();
            for (var i = 0; i < 30; i++)
            {
                await runtime.RefreshAsync();
            }

            await Task.WhenAll(readers);
        }

        [Fact]
        public async Task StatusReportsArriveInRefreshOrderOneAtATime()
        {
            const int refreshes = 50;
            const string truePayload = "{\"version\":2,\"flags\":{\"a\":{\"slug\":\"a\",\"enabled\":true,\"default_value\":true,\"off_value\":false,\"rules\":[]}}}";
            var falsePayload = truePayload.Replace("\"default_value\":true", "\"default_value\":false", StringComparison.Ordinal);
            using var handler = new ScriptedHandler((attempt, _, _) => Task.FromResult(Response(attempt % 2 == 0 ? truePayload : falsePayload)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var reported = new List<FeatureFlagsConfiguration?>();
            var delivered = Signal();
            var active = 0;
            var maxActive = 0;
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                Progress = new InlineProgress(status =>
                {
                    var concurrent = Interlocked.Increment(ref active);
                    InterlockedMax(ref maxActive, concurrent);
                    Thread.Sleep(1); // Widen the window so overlapping deliveries would be observed.
                    lock (reported)
                    {
                        reported.Add(status.Configuration);
                        if (reported.Count == refreshes)
                        {
                            delivered.TrySetResult();
                        }
                    }

                    Interlocked.Decrement(ref active);
                }),
            });

            var expected = new List<FeatureFlagsConfiguration?>();
            for (var i = 0; i < refreshes; i++)
            {
                await runtime.RefreshAsync();
                expected.Add(runtime.Configuration);
            }

            await delivered.Task.WaitAsync(TimeSpan.FromSeconds(10));
            Assert.Equal(expected, reported);
            Assert.Equal(1, maxActive);
        }

        [Fact]
        public async Task DisposalJoinsQueuedManualRefreshes()
        {
            for (var iteration = 0; iteration < 20; iteration++)
            {
                var entered = Signal();
                using var handler = new ScriptedHandler(async (_, _, token) =>
                {
                    entered.TrySetResult();
                    await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    return Response(OrderedPayload);
                });
                using var http = new HttpClient(handler);
                using var client = Client(http);
                await using var runtime = client.CreateFeatureFlagsRuntimeClient();
                var active = runtime.RefreshAsync();
                await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
                var queued = Enumerable.Range(0, 256).Select(_ => runtime.RefreshAsync()).ToArray();

                await runtime.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));

                Assert.True(active.IsCompleted);
                Assert.All(queued, refresh => Assert.True(refresh.IsCompleted));
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => active);
                foreach (var refresh in queued)
                {
                    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => refresh);
                }

                Assert.Equal(0, handler.ActiveRequests);
            }
        }

        [Theory]
        [InlineData(60)]
        [InlineData(0)]
        public async Task ShortPollingIntervalsAreClampedToNodeMinimum(long ticks)
        {
            var time = new TestTimeProvider();
            var secondPoll = Signal();
            using var handler = new ScriptedHandler((attempt, _, _) =>
            {
                if (attempt == 2)
                {
                    secondPoll.TrySetResult();
                }

                return Task.FromResult(Response(OrderedPayload));
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var options = new FeatureFlagsRuntimeOptions
            {
                PollingInterval = TimeSpan.FromTicks(ticks),
                TimeProvider = time,
            };
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(options);
            await runtime.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));

            // Node's five-second minimum permits ten percent jitter in either direction.
            var interval = await time.NextTimerAsync(due => due != options.RequestTimeout);
            Assert.InRange(interval, TimeSpan.FromSeconds(4.5), TimeSpan.FromSeconds(5.5));
            Assert.Equal(1, handler.RequestCount);

            time.Advance(interval);
            await secondPoll.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(2, handler.RequestCount);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(9999)]
        public void SubMillisecondRequestTimeoutsAreRejected(long ticks)
        {
            using var client = new WorkOSClient(new WorkOSOptions { ApiKey = "sk_test" });
            var error = Assert.Throws<ArgumentOutOfRangeException>(() => client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                RequestTimeout = TimeSpan.FromTicks(ticks),
            }));
            Assert.Equal(nameof(FeatureFlagsRuntimeOptions.RequestTimeout), error.ParamName);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task DisposalCancelsAndJoinsActivePollOrManualRefreshWithoutDisposingHttpClient(bool background)
        {
            var entered = Signal();
            var cancelled = Signal();
            using var handler = new ScriptedHandler(async (attempt, _, token) =>
            {
                if (attempt == 1)
                {
                    entered.TrySetResult();
                    try
                    {
                        await Task.Delay(Timeout.InfiniteTimeSpan, token);
                    }
                    finally
                    {
                        cancelled.TrySetResult();
                    }
                }

                return Response(OrderedPayload);
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var runtime = client.CreateFeatureFlagsRuntimeClient();
            var operation = background ? runtime.StartAsync() : runtime.RefreshAsync();
            var readyWait = runtime.WaitUntilReadyAsync();
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await runtime.DisposeAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => operation);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => readyWait);
            Assert.True(cancelled.Task.IsCompleted);
            Assert.Equal(0, handler.ActiveRequests);
            await runtime.DisposeAsync();
            await Assert.ThrowsAsync<ObjectDisposedException>(() => runtime.StartAsync());
            await Assert.ThrowsAsync<ObjectDisposedException>(() => runtime.RefreshAsync());
            using var response = await http.GetAsync("https://custom.example.com/still-owned");
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async Task DisposedRuntimeServesCallerFallbacksButKeepsItsLastSnapshotReadable()
        {
            var bootstrap = FeatureFlagsConfiguration.FromJson(OrderedPayload);
            using var client = new WorkOSClient(new WorkOSOptions { ApiKey = "sk_test" });
            var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions { BootstrapConfiguration = bootstrap });
            Assert.True(runtime.IsEnabled("ordered"));

            await runtime.DisposeAsync();

            Assert.False(runtime.IsEnabled("ordered"));
            Assert.True(runtime.IsEnabled("ordered", defaultValue: true));
            Assert.Empty(runtime.GetAllFlags());
            Assert.Same(bootstrap, runtime.Configuration);
            Assert.True(runtime.Status.IsReady);
        }

        [Fact]
        public async Task CancellingStartWaitDoesNotStopBackgroundRecovery()
        {
            var entered = Signal();
            var release = Signal();
            using var handler = new ScriptedHandler(async (_, _, token) =>
            {
                entered.TrySetResult();
                await release.Task.WaitAsync(token);
                return Response(OrderedPayload);
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient();
            using var cancellation = new CancellationTokenSource();
            var start = runtime.StartAsync(cancellation.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => start);
            release.TrySetResult();
            await runtime.WaitUntilReadyAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(runtime.IsReady);
            await runtime.StartAsync();
            Assert.Equal(1, handler.RequestCount);
        }

        [Fact]
        public async Task StatusObserverCanBeginAsyncDisposalWithoutDeadlock()
        {
            var disposal = new TaskCompletionSource<Task>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var handler = new ScriptedHandler((_, _, _) => Task.FromResult(Response(OrderedPayload)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            FeatureFlagsRuntimeClient? runtime = null;
            runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                Progress = new InlineProgress(_ =>
                {
                    disposal.TrySetResult(runtime!.DisposeAsync().AsTask());
                    throw new InvalidOperationException("Observer exceptions must not escape polling.");
                }),
            });
            await runtime.StartAsync();
            var disposalTask = await disposal.Task.WaitAsync(TimeSpan.FromSeconds(5));
            await disposalTask.WaitAsync(TimeSpan.FromSeconds(5));
            await runtime.DisposeAsync();
            Assert.Equal(0, handler.ActiveRequests);
        }

        [Fact]
        public async Task StatusCountsPolledAndManualRefreshesAndTheirFailures()
        {
            var time = new TestTimeProvider();
            using var handler = new ScriptedHandler((attempt, _, _) => Task.FromResult(attempt == 2
                ? Response("{}", HttpStatusCode.InternalServerError)
                : Response(OrderedPayload)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(PollingOptions(time));
            Assert.Equal(0, runtime.Status.RefreshCount);
            Assert.Equal(0, runtime.Status.FailedRefreshCount);
            Assert.Null(runtime.Status.LastRefreshAttempt);

            await runtime.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(1, runtime.Status.RefreshCount);
            Assert.Equal(0, runtime.Status.FailedRefreshCount);
            Assert.Equal(time.GetUtcNow(), runtime.Status.LastRefreshAttempt);
            var succeeded = runtime.Status.LastSuccessfulRefresh;

            // Manual refreshes between polls are counted with the polls.
            time.Advance(TimeSpan.FromSeconds(1));
            await Assert.ThrowsAsync<ServerException>(() => runtime.RefreshAsync());
            Assert.Equal(2, runtime.Status.RefreshCount);
            Assert.Equal(1, runtime.Status.FailedRefreshCount);
            Assert.Equal(time.GetUtcNow(), runtime.Status.LastRefreshAttempt);
            Assert.Equal(succeeded, runtime.Status.LastSuccessfulRefresh);

            await runtime.RefreshAsync();
            Assert.Equal(3, runtime.Status.RefreshCount);
            Assert.Equal(1, runtime.Status.FailedRefreshCount);
            Assert.Null(runtime.Status.LastError);
        }

        [Fact]
        public async Task CancelledRefreshesAreNotCounted()
        {
            var entered = Signal();
            using var handler = new ScriptedHandler(async (_, _, token) =>
            {
                entered.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return Response(OrderedPayload);
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient();
            using var cancellation = new CancellationTokenSource();
            var refresh = runtime.RefreshAsync(cancellation.Token);
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => refresh);

            Assert.Equal(0, runtime.Status.RefreshCount);
            Assert.Equal(0, runtime.Status.FailedRefreshCount);
            Assert.Null(runtime.Status.LastRefreshAttempt);
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized, true)]
        [InlineData(HttpStatusCode.Forbidden, true)]
        [InlineData(HttpStatusCode.Unauthorized, false)]
        [InlineData(HttpStatusCode.Forbidden, false)]
        public async Task AuthenticationFailuresReportThatPollingStopped(HttpStatusCode status, bool afterReadiness)
        {
            var time = new TestTimeProvider();
            using var handler = new ScriptedHandler((attempt, _, _) => Task.FromResult(
                afterReadiness && attempt == 1 ? Response(OrderedPayload) : Response("{}", status)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var stopped = new TaskCompletionSource<FeatureFlagsRuntimeStatus>(TaskCreationOptions.RunContinuationsAsynchronously);
            var options = PollingOptions(time);
            options.Progress = new InlineProgress(report =>
            {
                if (report.IsPollingStopped)
                {
                    stopped.TrySetResult(report);
                }
            });
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(options);

            if (afterReadiness)
            {
                await runtime.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));
                Assert.False(runtime.Status.IsPollingStopped);
                time.Advance(await time.NextTimerAsync(due => due != options.RequestTimeout));
            }
            else
            {
                await Assert.ThrowsAnyAsync<ApiException>(() => runtime.StartAsync());
                Assert.True(runtime.Status.IsPollingStopped); // Visible as soon as readiness fails.
            }

            var report = await stopped.Task.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Equal(status, Assert.IsAssignableFrom<ApiException>(report.LastError).StatusCode);
            Assert.Equal(afterReadiness, report.IsReady); // The last good snapshot is kept.
            Assert.True(runtime.Status.IsPollingStopped);

            var requests = handler.RequestCount;
            time.Advance(TimeSpan.FromMinutes(5));
            Assert.Equal(requests, handler.RequestCount);
            Assert.Equal(afterReadiness, runtime.IsEnabled("ordered"));
        }

        [Fact]
        public async Task GetFlagReturnsConfigurationAsReceivedAndMissingFlagsAsNull()
        {
            using var client = new WorkOSClient(new WorkOSOptions { ApiKey = "sk_test" });
            var unready = client.CreateFeatureFlagsRuntimeClient();
            Assert.Null(unready.GetFlag("ordered"));
            await unready.DisposeAsync();

            var bootstrap = FeatureFlagsConfiguration.FromJson(OrderedPayload);
            var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions { BootstrapConfiguration = bootstrap });

            var flag = runtime.GetFlag("ordered");
            Assert.NotNull(flag);
            Assert.Equal("ordered", flag.Slug);
            Assert.True(flag.Enabled);
            Assert.True(flag.DefaultValue);
            Assert.False(flag.OffValue);
            Assert.Equal(new[] { "r1", "r2" }, flag.Rules.Select(rule => rule.Id));
            var first = flag.Rules[0];
            Assert.Equal("conditions", first.Kind);
            Assert.False(first.Value);
            var condition = Assert.Single(first.Conditions);
            Assert.Equal("organization", condition.TargetType);
            Assert.Equal("one_of", condition.Operator);
            Assert.Equal(new[] { "org_acme" }, condition.Values);
            Assert.Equal(2, runtime.GetFlag("both")!.Rules[0].Conditions.Count);

            Assert.Null(runtime.GetFlag("missing"));
            Assert.False(bootstrap.TryGetFlag("missing", out _));
            Assert.True(bootstrap.TryGetFlag("ordered", out var fromSnapshot));
            Assert.Same(flag, fromSnapshot);
            Assert.Throws<ArgumentException>(() => runtime.GetFlag(" "));

            // Inspection stays available for persisting or debugging after disposal, like Configuration.
            await runtime.DisposeAsync();
            Assert.Same(flag, runtime.GetFlag("ordered"));
        }

        [Fact]
        public async Task GetFlagShowsRulesTheSdkCannotEvaluateWithoutEvaluatingThem()
        {
            const string payload = """
                {"version":2,"flags":{"test":{"slug":"test","enabled":true,"default_value":false,"off_value":"variant-off","rules":[
                  {"id":"rollout","kind":"percentage","value":true,"percentage":5},
                  {"id":"pattern","kind":"conditions","value":true,"conditions":[
                    {"target_type":"organization","operator":"one_of","values":["org_acme"]},
                    {"target_type":"email","operator":"ends_with","values":["@acme.com",7]},
                    {"operator":"one_of"},
                    "not an object"
                  ]},
                  {"id":"variant","kind":"conditions","value":"variant-b","conditions":[{"target_type":"user","operator":"one_of","values":["user_jp"]}]}
                ]}}}
                """;
            using var client = new WorkOSClient(new WorkOSOptions { ApiKey = "sk_test" });
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                BootstrapConfiguration = FeatureFlagsConfiguration.FromJson(payload),
            });
            var flag = runtime.GetFlag("test")!;
            Assert.Null(flag.OffValue);

            var rollout = flag.Rules[0];
            Assert.Equal("percentage", rollout.Kind);
            Assert.True(rollout.Value);
            Assert.Empty(rollout.Conditions);

            // The unknown operator and everything after it are reported best effort, never validated.
            var pattern = flag.Rules[1];
            Assert.Equal(new string?[] { "one_of", "ends_with", "one_of" }, pattern.Conditions.Select(condition => condition.Operator));
            Assert.Equal("email", pattern.Conditions[1].TargetType);
            Assert.Equal(new[] { "@acme.com" }, pattern.Conditions[1].Values);
            Assert.Null(pattern.Conditions[2].TargetType);
            Assert.Empty(pattern.Conditions[2].Values);

            Assert.Null(flag.Rules[2].Value);

            // Evaluation is unchanged: neither unknown rule matches, and the variant rule has no answer.
            var context = new FeatureFlagContext(userId: "user_jp", organizationId: "org_acme");
            Assert.False(runtime.IsEnabled("test", new FeatureFlagContext(organizationId: "org_acme")));
            Assert.True(runtime.IsEnabled("test", context, defaultValue: true));
            Assert.False(runtime.IsEnabled("test", context, defaultValue: false));
        }

        [Fact]
        public async Task ConfigurationChangedFiresOnFirstLoadAndOnlyForRealChanges()
        {
            using var handler = new ScriptedHandler((attempt, _, _) => Task.FromResult(attempt switch
            {
                1 => Response(ChangeBase),
                2 => Response(ChangeBaseReordered),
                3 => Response("{}", HttpStatusCode.InternalServerError),
                4 => Response(ChangeEdited),
                _ => Response(ChangeRulesReordered),
            }));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient();
            var changes = RecordChanges(runtime);

            await runtime.RefreshAsync();
            var installed = runtime.Configuration;
            await runtime.RefreshAsync(); // Same content: flag order and target ID order differ.
            await Assert.ThrowsAsync<ServerException>(() => runtime.RefreshAsync());
            var beforeEdit = runtime.Configuration;
            await runtime.RefreshAsync();
            var edited = runtime.Configuration;
            await runtime.RefreshAsync();

            // Events arrive in refresh order, so the next event after the first load being the edit
            // proves the identical and failed refreshes raised nothing.
            var firstLoad = await NextChangeAsync(changes);
            Assert.Null(firstLoad.Previous);
            Assert.Same(installed, firstLoad.Current);
            Assert.Equal(new[] { "alpha", "beta", "gamma" }, firstLoad.ChangedFlags.Order());

            var edit = await NextChangeAsync(changes);
            Assert.Same(beforeEdit, edit.Previous);
            Assert.Same(edited, edit.Current);
            Assert.Equal(new[] { "beta", "delta", "gamma" }, edit.ChangedFlags.Order()); // Modified, added, removed.

            var reorder = await NextChangeAsync(changes);
            Assert.Equal(new[] { "alpha" }, reorder.ChangedFlags); // Rule order decides evaluation.
            Assert.Same(runtime.Configuration, reorder.Current);
            Assert.False(changes.Reader.TryRead(out _));
        }

        [Fact]
        public async Task ConfigurationChangedComparesTheFirstRefreshWithTheBootstrap()
        {
            var bootstrap = FeatureFlagsConfiguration.FromJson(ChangeBase);
            using var handler = new ScriptedHandler((attempt, _, _) => Task.FromResult(Response(attempt == 1 ? ChangeBaseReordered : ChangeEdited)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var current = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions { BootstrapConfiguration = bootstrap });
            var changes = RecordChanges(current);
            await current.RefreshAsync(); // Matches the bootstrap: no event.
            await current.RefreshAsync();
            var change = await NextChangeAsync(changes);
            Assert.Equal(new[] { "beta", "delta", "gamma" }, change.ChangedFlags.Order());

            // An outdated bootstrap is reported as the previous snapshot of the first refresh.
            using var outdatedHandler = new ScriptedHandler((_, _, _) => Task.FromResult(Response(ChangeEdited)));
            using var outdatedHttp = new HttpClient(outdatedHandler);
            using var outdatedClient = Client(outdatedHttp);
            await using var outdated = outdatedClient.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions { BootstrapConfiguration = bootstrap });
            var outdatedChanges = RecordChanges(outdated);
            await outdated.RefreshAsync();
            var refreshed = await NextChangeAsync(outdatedChanges);
            Assert.Same(bootstrap, refreshed.Previous);
            Assert.Equal(new[] { "beta", "delta", "gamma" }, refreshed.ChangedFlags.Order());
        }

        [Fact]
        public async Task ThrowingChangeHandlerDoesNotStopPollingOrLaterHandlers()
        {
            var time = new TestTimeProvider();
            using var handler = new ScriptedHandler((attempt, _, _) => Task.FromResult(Response(attempt == 1 ? ChangeBase : ChangeEdited)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var options = PollingOptions(time);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(options);
            runtime.ConfigurationChanged += (_, _) => throw new InvalidOperationException("Handler exceptions must not escape polling.");
            var changes = RecordChanges(runtime);

            await runtime.StartAsync().WaitAsync(TimeSpan.FromSeconds(5));
            Assert.Null((await NextChangeAsync(changes)).Previous);

            time.Advance(await time.NextTimerAsync(due => due != options.RequestTimeout));
            var polled = await NextChangeAsync(changes);
            Assert.Equal(new[] { "beta", "delta", "gamma" }, polled.ChangedFlags.Order());
            Assert.Equal(2, handler.RequestCount);
        }

        private static Channel<FeatureFlagsChangedEventArgs> RecordChanges(FeatureFlagsRuntimeClient runtime)
        {
            var changes = Channel.CreateUnbounded<FeatureFlagsChangedEventArgs>();
            runtime.ConfigurationChanged += (_, change) => changes.Writer.TryWrite(change);
            return changes;
        }

        private static async Task<FeatureFlagsChangedEventArgs> NextChangeAsync(Channel<FeatureFlagsChangedEventArgs> changes)
        {
            using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            return await changes.Reader.ReadAsync(deadline.Token);
        }

        private static string SingleFlag(string rule) =>
            "{\"version\":2,\"flags\":{\"test\":{\"slug\":\"test\",\"enabled\":true,\"default_value\":false,\"off_value\":false,\"rules\":[" + rule + "]}}}";

        private static FeatureFlagsRuntimeOptions PollingOptions(TestTimeProvider? time = null) => new FeatureFlagsRuntimeOptions
        {
            PollingInterval = TimeSpan.FromSeconds(5),
            TimeProvider = time,
        };

        private static WorkOSClient Client(HttpClient http) => new WorkOSClient(new WorkOSOptions
        {
            ApiKey = "sk_test",
            ApiBaseURL = "https://custom.example.com",
            HttpClient = http,
            MaxRetries = 0,
        });

        private static void InterlockedMax(ref int location, int value)
        {
            int current;
            while ((current = Volatile.Read(ref location)) < value && Interlocked.CompareExchange(ref location, value, current) != current)
            {
            }
        }

        private static TaskCompletionSource Signal() => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        private static HttpResponseMessage Response(string json, HttpStatusCode status = HttpStatusCode.OK) =>
            new HttpResponseMessage(status) { Content = new StringContent(json) };

        private sealed class InlineProgress : IProgress<FeatureFlagsRuntimeStatus>
        {
            private readonly Action<FeatureFlagsRuntimeStatus> callback;

            internal InlineProgress(Action<FeatureFlagsRuntimeStatus> callback) => this.callback = callback;

            public void Report(FeatureFlagsRuntimeStatus value) => this.callback(value);
        }

        /// <summary>A controllable clock that also exposes the due time of every timer the runtime schedules.</summary>
        private sealed class TestTimeProvider : FakeTimeProvider
        {
            private readonly Channel<TimeSpan> timers = Channel.CreateUnbounded<TimeSpan>();

            public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
            {
                var timer = base.CreateTimer(callback, state, dueTime, period);
                this.timers.Writer.TryWrite(dueTime);
                return timer;
            }

            internal async Task<TimeSpan> NextTimerAsync(Func<TimeSpan, bool> predicate)
            {
                using var deadline = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                while (true)
                {
                    var due = await this.timers.Reader.ReadAsync(deadline.Token);
                    if (predicate(due))
                    {
                        return due;
                    }
                }
            }
        }

        private sealed class ScriptedHandler : HttpMessageHandler
        {
            private readonly Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send;
            private int requestCount;
            private int activeRequests;
            private int maxConcurrentRequests;

            internal ScriptedHandler(Func<int, HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send) => this.send = send;

            internal int RequestCount => Volatile.Read(ref this.requestCount);

            internal int ActiveRequests => Volatile.Read(ref this.activeRequests);

            internal int MaxConcurrentRequests => Volatile.Read(ref this.maxConcurrentRequests);

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var attempt = Interlocked.Increment(ref this.requestCount);
                var active = Interlocked.Increment(ref this.activeRequests);
                if (active > this.maxConcurrentRequests)
                {
                    Interlocked.Exchange(ref this.maxConcurrentRequests, active);
                }

                try
                {
                    return await this.send(attempt, request, cancellationToken);
                }
                finally
                {
                    Interlocked.Decrement(ref this.activeRequests);
                }
            }
        }
    }
}
