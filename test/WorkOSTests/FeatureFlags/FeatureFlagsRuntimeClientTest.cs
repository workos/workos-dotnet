// @oagen-ignore-file
#nullable enable
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
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
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(PollingOptions());
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            await runtime.StartAsync(timeout.Token);
            Assert.True(handler.RequestCount >= 2);
            Assert.True(runtime.IsEnabled("ordered"));
            Assert.Null(runtime.Status.LastError);
            Assert.Equal(1, handler.MaxConcurrentRequests);
        }

        [Theory]
        [InlineData(HttpStatusCode.Unauthorized)]
        [InlineData(HttpStatusCode.Forbidden)]
        public async Task AuthenticationFailuresStopPollingAndFailReadiness(HttpStatusCode status)
        {
            using var handler = new ScriptedHandler((_, _, _) => Task.FromResult(Response("{}", status)));
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(PollingOptions());
            var error = await Assert.ThrowsAnyAsync<ApiException>(() => runtime.StartAsync());
            Assert.Equal(status, error.StatusCode);
            await Task.Delay(TimeSpan.FromSeconds(6));
            Assert.Equal(1, handler.RequestCount);
            Assert.False(runtime.IsReady);
            Assert.Same(error, runtime.Status.LastError);
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
            using var handler = new ScriptedHandler(async (_, _, token) =>
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, token);
                return Response(OrderedPayload);
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            var options = PollingOptions();
            options.RequestTimeout = TimeSpan.FromMilliseconds(40);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(options);
            await Assert.ThrowsAsync<TimeoutException>(() => runtime.RefreshAsync());
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
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                BootstrapConfiguration = bootstrap,
                RequestTimeout = TimeSpan.FromMilliseconds(100),
            });
            var active = runtime.RefreshAsync();
            await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
            var queued = runtime.RefreshAsync();

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
            var elapsed = new Stopwatch();
            var nextPoll = new TaskCompletionSource<TimeSpan>(TaskCreationOptions.RunContinuationsAsynchronously);
            using var handler = new ScriptedHandler((attempt, _, _) =>
            {
                if (attempt == 1)
                {
                    elapsed.Start();
                }
                else
                {
                    nextPoll.TrySetResult(elapsed.Elapsed);
                }

                return Task.FromResult(Response(OrderedPayload));
            });
            using var http = new HttpClient(handler);
            using var client = Client(http);
            await using var runtime = client.CreateFeatureFlagsRuntimeClient(new FeatureFlagsRuntimeOptions
            {
                PollingInterval = TimeSpan.FromTicks(ticks),
            });
            await runtime.StartAsync();

            var interval = await nextPoll.Task.WaitAsync(TimeSpan.FromSeconds(15));

            // Node's five-second minimum permits ten percent jitter in either direction.
            Assert.True(interval >= TimeSpan.FromSeconds(4.4), $"Polling interval was {interval}.");
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
            Assert.Throws<ObjectDisposedException>(() => runtime.IsEnabled("ordered"));
            using var response = await http.GetAsync("https://custom.example.com/still-owned");
            Assert.True(response.IsSuccessStatusCode);
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

        private static string SingleFlag(string rule) =>
            "{\"version\":2,\"flags\":{\"test\":{\"slug\":\"test\",\"enabled\":true,\"default_value\":false,\"off_value\":false,\"rules\":[" + rule + "]}}}";

        private static FeatureFlagsRuntimeOptions PollingOptions() => new FeatureFlagsRuntimeOptions
        {
            PollingInterval = TimeSpan.FromSeconds(5),
        };

        private static WorkOSClient Client(HttpClient http) => new WorkOSClient(new WorkOSOptions
        {
            ApiKey = "sk_test",
            ApiBaseURL = "https://custom.example.com",
            HttpClient = http,
            MaxRetries = 0,
        });

        private static TaskCompletionSource Signal() => new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        private static HttpResponseMessage Response(string json, HttpStatusCode status = HttpStatusCode.OK) =>
            new HttpResponseMessage(status) { Content = new StringContent(json) };

        private sealed class InlineProgress : IProgress<FeatureFlagsRuntimeStatus>
        {
            private readonly Action<FeatureFlagsRuntimeStatus> callback;

            internal InlineProgress(Action<FeatureFlagsRuntimeStatus> callback) => this.callback = callback;

            public void Report(FeatureFlagsRuntimeStatus value) => this.callback(value);
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
