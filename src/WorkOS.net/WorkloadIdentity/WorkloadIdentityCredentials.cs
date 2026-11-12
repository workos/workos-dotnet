// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Authenticates a <see cref="WorkOSClient"/> with workload identity federation: the
    /// identity token the workload's platform issues is exchanged for a short-lived WorkOS
    /// access token, so no API key is stored.
    /// <para>
    /// The access token is cached in memory on this instance, refreshed on demand shortly
    /// before it expires, and replaced once when the API rejects it. Concurrent requests
    /// share one exchange. Pass an instance as <see cref="WorkOSOptions.Credentials"/>.
    /// </para>
    /// </summary>
    public sealed class WorkloadIdentityCredentials : IWorkOSCredentials, IDisposable
    {
        private readonly object gate = new object();
        private readonly string clientId;
        private readonly Uri tokenUrl;
        private readonly string environmentId;
        private readonly string resource;
        private readonly string? scope;
        private readonly IIdentityTokenProvider provider;
        private readonly HttpClient httpClient;
        private readonly TimeProvider clock;
        private readonly Func<TimeSpan, CancellationToken, Task> sleeper;

        // The cache, guarded by gate. Generation 0 has no token. Nothing here is static:
        // two instances never share a token, even with identical settings.
        private CredentialSnapshot? snapshot;
        private DateTimeOffset expiresAt;
        private TimeSpan lifetime;
        private long generation;

        // The one exchange in progress, or null. Guarded by gate.
        private Task<CredentialSnapshot>? inflight;
        private bool disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadIdentityCredentials"/> class.
        /// </summary>
        /// <param name="options">The settings. Omitted values are read from the environment.</param>
        /// <exception cref="WorkloadIdentityException">A required setting is missing, with code <c>invalid_configuration</c>.</exception>
        public WorkloadIdentityCredentials(WorkloadIdentityCredentialsOptions options)
            : this(options, handler: null, clock: null, sleeper: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadIdentityCredentials"/> class
        /// with the test seams filled in.
        /// </summary>
        /// <param name="options">The settings. Omitted values are read from the environment.</param>
        /// <param name="handler">Answers the exchange request in place of the network.</param>
        /// <param name="clock">The clock the cache and the exchange deadline run on.</param>
        /// <param name="sleeper">Waits between exchange attempts.</param>
        internal WorkloadIdentityCredentials(
            WorkloadIdentityCredentialsOptions options,
            HttpMessageHandler? handler,
            TimeProvider? clock,
            Func<TimeSpan, CancellationToken, Task>? sleeper)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            this.clientId = Require(options.ClientId, WorkloadIdentityConstants.ClientIdVariable, nameof(options.ClientId));
            var url = Require(options.TokenUrl, WorkloadIdentityConstants.TokenUrlVariable, nameof(options.TokenUrl));
            this.environmentId = Require(options.EnvironmentId, WorkloadIdentityConstants.EnvironmentIdVariable, nameof(options.EnvironmentId));
            this.resource = FirstSet(options.Resource) ?? WorkloadIdentityConstants.DefaultResource;
            this.scope = FirstSet(options.Scope, Environment.GetEnvironmentVariable(WorkloadIdentityConstants.ScopeVariable));

            if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl)
                || (parsedUrl.Scheme != Uri.UriSchemeHttps && parsedUrl.Scheme != Uri.UriSchemeHttp))
            {
                throw new WorkloadIdentityException(
                    WorkloadIdentityException.InvalidConfiguration,
                    $"{nameof(options.TokenUrl)} must be an absolute http or https URL");
            }

            this.tokenUrl = parsedUrl;

            if (options.IdentityTokenProvider != null && options.IdentityTokenCallback != null)
            {
                throw new WorkloadIdentityException(
                    WorkloadIdentityException.InvalidConfiguration,
                    $"Set {nameof(options.IdentityTokenProvider)} or {nameof(options.IdentityTokenCallback)}, not both");
            }

            if (options.IdentityTokenProvider != null)
            {
                this.provider = options.IdentityTokenProvider;
            }
            else if (options.IdentityTokenCallback != null)
            {
                this.provider = IdentityTokenProvider.FromCallback(options.IdentityTokenCallback);
            }
            else
            {
                throw new WorkloadIdentityException(
                    WorkloadIdentityException.InvalidConfiguration,
                    $"{nameof(options.IdentityTokenProvider)} is required");
            }

            this.clock = clock ?? TimeProvider.System;
            this.sleeper = sleeper ?? ((delay, token) => Task.Delay(delay, this.clock, token));

            // The exchange has its own HTTP client. Redirects can only be switched off on the
            // handler, and the client's own HttpClient may carry the caller's handlers, which
            // must never see the identity token.
            this.httpClient = new HttpClient(handler ?? new HttpClientHandler { AllowAutoRedirect = false }, disposeHandler: true)
            {
                Timeout = WorkOSClient.DefaultTimeout,
            };
        }

        /// <summary>
        /// Returns the current access token as request headers. Reuses the cached token while
        /// it is fresh; otherwise waits for the one shared exchange.
        /// </summary>
        /// <param name="cancellationToken">Stops this caller's wait. The shared exchange keeps running for the others.</param>
        /// <returns>The <c>Authorization</c> and environment headers, and their generation.</returns>
        /// <exception cref="WorkloadIdentityException">The exchange failed. A failure is not cached; the next call tries again.</exception>
        public Task<CredentialSnapshot> ResolveAsync(CancellationToken cancellationToken)
        {
            Task<CredentialSnapshot> call;
            lock (this.gate)
            {
                ObjectDisposedException.ThrowIf(this.disposed, this);

                if (this.snapshot != null && this.IsFresh())
                {
                    return Task.FromResult(this.snapshot);
                }

                if (this.inflight == null)
                {
                    // Detached from this caller: it runs on its own deadline, so a caller
                    // that gives up does not cancel it for everyone else.
                    this.inflight = this.StartExchange();
                    Observe(this.inflight);
                }

                call = this.inflight;
            }

            return call.WaitAsync(cancellationToken);
        }

        /// <summary>
        /// Marks the token of the given generation as expired, so the next resolve exchanges
        /// again. Does nothing when a newer token is already cached, which keeps a late 401
        /// for an old token from discarding a fresh one.
        /// </summary>
        /// <param name="generation">The generation of the snapshot the API rejected.</param>
        public void Invalidate(long generation)
        {
            lock (this.gate)
            {
                if (this.generation == generation)
                {
                    this.expiresAt = DateTimeOffset.MinValue;
                }
            }
        }

        /// <summary>
        /// Releases the HTTP client used for the exchange.
        /// </summary>
        public void Dispose()
        {
            lock (this.gate)
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;
            }

            this.httpClient.Dispose();
        }

        private static string Require(string? value, string variable, string field)
        {
            return FirstSet(value, Environment.GetEnvironmentVariable(variable))
                ?? throw new WorkloadIdentityException(
                    WorkloadIdentityException.InvalidConfiguration,
                    $"{field} is required (or set {variable})");
        }

        private static string? FirstSet(params string?[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return null;
        }

        private static TimeSpan RefreshThreshold(TimeSpan lifetime)
        {
            var share = TimeSpan.FromTicks((long)Math.Round(lifetime.Ticks * WorkloadIdentityConstants.RefreshThresholdFraction));
            return share < WorkloadIdentityConstants.RefreshThresholdCap ? share : WorkloadIdentityConstants.RefreshThresholdCap;
        }

        // A server could echo the assertion in its error description. It must not reach an error message.
        private static string? Scrub(string? description, string assertion)
        {
            return description?.Replace(assertion, "[redacted]", StringComparison.Ordinal);
        }

        // Keeps the failure of a task that nobody waits for any more (every caller gave up,
        // or the deadline passed) from surfacing as an unobserved task exception.
        private static void Observe(Task task)
        {
            task.ContinueWith(
                faulted => _ = faulted.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        // Called with the gate held.
        private bool IsFresh()
        {
            return this.expiresAt - this.clock.GetUtcNow() >= RefreshThreshold(this.lifetime);
        }

        // Starts the shared exchange on the thread pool without the starting caller's
        // execution context, so it carries no one request's ambient state or trace.
        private Task<CredentialSnapshot> StartExchange()
        {
            if (ExecutionContext.IsFlowSuppressed())
            {
                return Task.Run(this.RunExchangeAsync);
            }

            using (ExecutionContext.SuppressFlow())
            {
                return Task.Run(this.RunExchangeAsync);
            }
        }

        private async Task<CredentialSnapshot> RunExchangeAsync()
        {
            try
            {
                TokenBody token;
                using (var deadline = new CancellationTokenSource(WorkloadIdentityConstants.ExchangeDeadline, this.clock))
                {
                    token = await this.ExchangeAsync(deadline.Token).ConfigureAwait(false);
                }

                var headers = new Dictionary<string, string>
                {
                    ["Authorization"] = "Bearer " + token.AccessToken,
                    [WorkloadIdentityConstants.EnvironmentHeader] = this.environmentId,
                };

                // Store the token and clear the in-flight exchange under one lock, before the
                // waiters are woken (they wake when this method returns).
                lock (this.gate)
                {
                    this.generation++;
                    this.lifetime = token.Lifetime;
                    this.expiresAt = this.clock.GetUtcNow() + token.Lifetime;
                    this.snapshot = new CredentialSnapshot(headers, this.generation, recoverable: true);
                    this.inflight = null;
                    return this.snapshot;
                }
            }
            catch (Exception ex)
            {
                // Clear first, so a caller that sees the error and calls again at once starts
                // a new exchange and never joins this finished one.
                lock (this.gate)
                {
                    this.inflight = null;
                }

                if (ex is WorkloadIdentityException)
                {
                    throw;
                }

                throw new WorkloadIdentityException(WorkloadIdentityException.ExchangeFailed, description: null, innerException: ex);
            }
        }

        // One exchange: the provider call and the POST, retried on provider failures, transport
        // failures, 429 and 5xx with the client's own retry count and backoff.
        private async Task<TokenBody> ExchangeAsync(CancellationToken deadline)
        {
            // What to report if the exchange stops now: the most recent failure.
            var failureCode = WorkloadIdentityException.ProviderFailed;
            string? failureDescription = null;
            Exception? failureCause = null;

            // The last 429 or 5xx response, kept until the next attempt for its Retry-After.
            HttpResponseMessage? retryResponse = null;

            try
            {
                for (var attempt = 0; attempt <= WorkOSOptions.DefaultMaxRetries; attempt++)
                {
                    if (attempt > 0)
                    {
                        var delay = WorkOSClient.ComputeRetryDelay(attempt, retryResponse);
                        await this.sleeper(delay, deadline).ConfigureAwait(false);
                    }

                    retryResponse?.Dispose();
                    retryResponse = null;

                    // The provider is called again on every attempt: the source may have rotated.
                    failureCode = WorkloadIdentityException.ProviderFailed;
                    failureDescription = null;
                    failureCause = null;

                    string assertion;
                    try
                    {
                        // The provider is the caller's code. Run it off this thread and stop
                        // waiting at the deadline, so a provider that ignores its token, or
                        // blocks, cannot keep the exchange (and every waiter) from ending.
                        var providerCall = Task.Run(() => this.provider.GetTokenAsync(deadline), deadline);
                        Observe(providerCall);
                        assertion = await providerCall.WaitAsync(deadline).ConfigureAwait(false);
                    }
                    catch (Exception ex) when (!(ex is OperationCanceledException) || !deadline.IsCancellationRequested)
                    {
                        failureCause = ex;
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(assertion))
                    {
                        failureDescription = "The identity token provider returned an empty token.";
                        continue;
                    }

                    failureCode = WorkloadIdentityException.ExchangeFailed;

                    HttpResponseMessage response;
                    try
                    {
                        using var request = this.BuildExchangeRequest(assertion);
                        response = await this.httpClient.SendAsync(request, deadline).ConfigureAwait(false);
                    }
                    catch (HttpRequestException ex)
                    {
                        failureCause = ex;
                        continue;
                    }
                    catch (OperationCanceledException ex) when (!deadline.IsCancellationRequested)
                    {
                        // The HTTP client's own timeout, not the deadline: a transport failure.
                        failureCause = ex;
                        continue;
                    }

                    // The status is checked before the body: a 429 or 5xx is retried even
                    // when its body carries an OAuth error.
                    var status = (int)response.StatusCode;
                    var body = TokenBody.Parse(await response.Content.ReadAsStringAsync(deadline).ConfigureAwait(false));

                    if (status == 429 || status >= 500)
                    {
                        failureCode = body.Error ?? WorkloadIdentityException.ExchangeFailed;
                        failureDescription = Scrub(body.ErrorDescription, assertion) ?? $"The token endpoint returned HTTP {status}.";
                        retryResponse = response;
                        continue;
                    }

                    response.Dispose();

                    if (body.Error != null)
                    {
                        // An OAuth error is the server's final answer. It is not retried.
                        throw new WorkloadIdentityException(body.Error, Scrub(body.ErrorDescription, assertion));
                    }

                    if (status >= 300 && status < 400)
                    {
                        throw new WorkloadIdentityException(
                            WorkloadIdentityException.ExchangeFailed,
                            $"The token endpoint answered with a redirect (HTTP {status}), which is not followed.");
                    }

                    if (status != 200)
                    {
                        throw new WorkloadIdentityException(
                            WorkloadIdentityException.InvalidResponse,
                            $"The token endpoint returned HTTP {status}.");
                    }

                    if (!body.IsObject)
                    {
                        throw new WorkloadIdentityException(
                            WorkloadIdentityException.InvalidResponse,
                            "The token endpoint's response was not a JSON object.");
                    }

                    if (string.IsNullOrEmpty(body.AccessToken))
                    {
                        throw new WorkloadIdentityException(
                            WorkloadIdentityException.InvalidResponse,
                            "The token endpoint's response has no access_token.");
                    }

                    return body;
                }
            }
            catch (OperationCanceledException ex) when (deadline.IsCancellationRequested)
            {
                var seconds = (int)WorkloadIdentityConstants.ExchangeDeadline.TotalSeconds;
                throw new WorkloadIdentityException(
                    failureCode,
                    failureDescription ?? $"The exchange did not finish within {seconds} seconds.",
                    failureCause ?? ex);
            }
            finally
            {
                retryResponse?.Dispose();
            }

            throw new WorkloadIdentityException(failureCode, failureDescription, failureCause);
        }

        // No client secret, no Authorization header and no API key: the assertion is the credential.
        private HttpRequestMessage BuildExchangeRequest(string assertion)
        {
            var form = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", WorkloadIdentityConstants.GrantType),
                new KeyValuePair<string, string>("assertion", assertion),
                new KeyValuePair<string, string>("client_id", this.clientId),
                new KeyValuePair<string, string>("resource", this.resource),
            };

            if (this.scope != null)
            {
                form.Add(new KeyValuePair<string, string>("scope", this.scope));
            }

            var request = new HttpRequestMessage(HttpMethod.Post, this.tokenUrl)
            {
                Content = new FormUrlEncodedContent(form),
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.TryAddWithoutValidation("User-Agent", WorkOSClient.UserAgent);
            return request;
        }

        /// <summary>
        /// The fields of a token endpoint response that the exchange reads.
        /// </summary>
        private readonly struct TokenBody
        {
            private TokenBody(string? error, string? errorDescription, string? accessToken, TimeSpan lifetime)
            {
                this.IsObject = true;
                this.Error = error;
                this.ErrorDescription = errorDescription;
                this.AccessToken = accessToken;
                this.Lifetime = lifetime;
            }

            internal bool IsObject { get; }

            internal string? Error { get; }

            internal string? ErrorDescription { get; }

            internal string? AccessToken { get; }

            internal TimeSpan Lifetime { get; }

            internal static TokenBody Parse(string body)
            {
                try
                {
                    using var document = JsonDocument.Parse(body);
                    var root = document.RootElement;
                    if (root.ValueKind != JsonValueKind.Object)
                    {
                        return default;
                    }

                    return new TokenBody(
                        StringField(root, "error"),
                        StringField(root, "error_description"),
                        StringField(root, "access_token"),
                        LifetimeOf(root));
                }
                catch (JsonException)
                {
                    return default;
                }
            }

            private static string? StringField(JsonElement root, string name)
            {
                if (root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String)
                {
                    var text = value.GetString();
                    return string.IsNullOrEmpty(text) ? null : text;
                }

                return null;
            }

            // Absent, not a number, not positive, or too large to be a real lifetime: the default.
            private static TimeSpan LifetimeOf(JsonElement root)
            {
                if (root.TryGetProperty("expires_in", out var value)
                    && value.ValueKind == JsonValueKind.Number
                    && value.TryGetDouble(out var seconds)
                    && seconds > 0
                    && seconds <= int.MaxValue)
                {
                    return TimeSpan.FromSeconds(seconds);
                }

                return WorkloadIdentityConstants.DefaultLifetime;
            }
        }
    }
}
