// @oagen-ignore-file
#nullable enable
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using WorkOS;

    /// <summary>
    /// Fixtures and builders for the workload identity conformance tests. The values come
    /// from conformance-cases.md.
    /// </summary>
    internal static class Wif
    {
        internal const string ClientId = "client_wif_test";
        internal const string EnvironmentId = "environment_wif_test";
        internal const string TokenUrl = "https://auth.workos.test/oauth2/token";
        internal const string EnvironmentHeader = "x-url-environment-id";

        /// <summary>The environment variables the SDK reads for workload identity, plus the API key variable.</summary>
        internal static readonly string[] EnvironmentVariables =
        {
            "WORKOS_CLIENT_ID",
            "WORKOS_WIF_TOKEN_URL",
            "WORKOS_WIF_SCOPE",
            "WORKOS_ENVIRONMENT_ID",
            "WORKOS_API_KEY",
        };

        /// <summary>Options with the fixture client ID, token URL and environment ID.</summary>
        internal static WorkloadIdentityCredentialsOptions Options(IIdentityTokenProvider provider)
        {
            return new WorkloadIdentityCredentialsOptions
            {
                ClientId = ClientId,
                TokenUrl = TokenUrl,
                EnvironmentId = EnvironmentId,
                IdentityTokenProvider = provider,
            };
        }

        /// <summary>Credentials whose exchange goes to the mock token endpoint.</summary>
        internal static WorkloadIdentityCredentials Credentials(
            WorkloadIdentityCredentialsOptions options,
            TokenEndpoint token,
            TimeProvider? clock = null,
            SleepRecorder? sleeper = null)
        {
            // Backoff never really sleeps in these tests; a recorder is always in place.
            sleeper ??= new SleepRecorder();
            return new WorkloadIdentityCredentials(options, token.Handler, clock, sleeper.SleepAsync);
        }

        /// <summary>A client that authenticates with the given credentials against the mock API.</summary>
        internal static WorkOSClient Client(WorkOS.ICredentials credentials, RecordingHandler api, int? maxRetries = null)
        {
            var options = new WorkOSOptions
            {
                Credentials = credentials,
                HttpClient = new HttpClient(api),
            };
            if (maxRetries != null)
            {
                options.MaxRetries = maxRetries.Value;
            }

            return new WorkOSClient(options);
        }

        /// <summary>A mock API that answers every request with 200.</summary>
        internal static RecordingHandler Api()
        {
            return new RecordingHandler((_, _, _) => Task.FromResult(Json(HttpStatusCode.OK, "{}")));
        }

        /// <summary>A mock API that answers from a script; the first request is number 1.</summary>
        internal static RecordingHandler Api(Func<int, HttpResponseMessage> script)
        {
            return new RecordingHandler((number, _, _) => Task.FromResult(script(number)));
        }

        /// <summary>A plain GET, the request most cases make.</summary>
        internal static WorkOSRequest Get()
        {
            return new WorkOSRequest { Method = HttpMethod.Get, Path = "/test" };
        }

        internal static HttpResponseMessage Json(HttpStatusCode status, string json)
        {
            return new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
        }

        /// <summary>The OAuth error response of the fixtures: HTTP 400 with <c>error</c> and <c>error_description</c>.</summary>
        internal static HttpResponseMessage OAuthError(string code, string description, HttpStatusCode status = HttpStatusCode.BadRequest)
        {
            var body = new Newtonsoft.Json.Linq.JObject
            {
                ["error"] = code,
                ["error_description"] = description,
            };
            return Json(status, body.ToString());
        }

        internal static HttpResponseMessage Status(HttpStatusCode status, string? retryAfter = null)
        {
            var response = new HttpResponseMessage(status) { Content = new StringContent(string.Empty) };
            if (retryAfter != null)
            {
                response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
            }

            return response;
        }
    }

    /// <summary>One HTTP request a mock received. Captured when it was sent, so the body outlives the request.</summary>
    internal sealed class RecordedRequest
    {
        private RecordedRequest(HttpMethod method, Uri uri, Dictionary<string, string> headers, string? contentType, byte[] bodyBytes)
        {
            this.Method = method;
            this.Uri = uri;
            this.Headers = headers;
            this.ContentType = contentType;
            this.BodyBytes = bodyBytes;
        }

        internal HttpMethod Method { get; }

        internal Uri Uri { get; }

        /// <summary>Request headers by case-insensitive name; several values are joined with a comma.</summary>
        internal IReadOnlyDictionary<string, string> Headers { get; }

        internal string? ContentType { get; }

        internal byte[] BodyBytes { get; }

        internal string Body => Encoding.UTF8.GetString(this.BodyBytes);

        /// <summary>The body parsed as a URL-encoded form.</summary>
        internal NameValueCollection Form => System.Web.HttpUtility.ParseQueryString(this.Body);

        internal static async Task<RecordedRequest> FromAsync(HttpRequestMessage request)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in request.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }

            var body = request.Content == null ? Array.Empty<byte>() : await request.Content.ReadAsByteArrayAsync();
            return new RecordedRequest(
                request.Method,
                request.RequestUri!,
                headers,
                request.Content?.Headers.ContentType?.ToString(),
                body);
        }
    }

    /// <summary>
    /// A handler that records every request and answers from a script. The script gets the
    /// request's number (the first is 1), the request, and the caller's cancellation token.
    /// </summary>
    internal sealed class RecordingHandler : HttpMessageHandler
    {
        private readonly Func<int, RecordedRequest, CancellationToken, Task<HttpResponseMessage>> respond;
        private readonly List<RecordedRequest> requests = new List<RecordedRequest>();

        internal RecordingHandler(Func<int, RecordedRequest, CancellationToken, Task<HttpResponseMessage>> respond)
        {
            this.respond = respond;
        }

        /// <summary>The number of requests received.</summary>
        internal int Count
        {
            get
            {
                lock (this.requests)
                {
                    return this.requests.Count;
                }
            }
        }

        /// <summary>Every request received so far, in order.</summary>
        internal IReadOnlyList<RecordedRequest> Requests
        {
            get
            {
                lock (this.requests)
                {
                    return this.requests.ToArray();
                }
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var recorded = await RecordedRequest.FromAsync(request);
            int number;
            lock (this.requests)
            {
                this.requests.Add(recorded);
                number = this.requests.Count;
            }

            return await this.respond(number, recorded, cancellationToken);
        }
    }

    /// <summary>
    /// The mock token endpoint. Its exchange count is the number of POSTs it received.
    /// By default every exchange succeeds with <c>access-1</c>, <c>access-2</c> and so on.
    /// </summary>
    internal sealed class TokenEndpoint
    {
        private int issued;

        // An endpoint where every exchange succeeds.
        internal TokenEndpoint()
            : this((endpoint, _, _) => endpoint.Success())
        {
        }

        // An endpoint that answers from a script; the first exchange is number 1.
        internal TokenEndpoint(Func<TokenEndpoint, int, RecordedRequest, HttpResponseMessage> script)
        {
            this.Handler = new RecordingHandler((number, request, _) => Task.FromResult(script(this, number, request)));
        }

        private TokenEndpoint(Func<TokenEndpoint, int, RecordedRequest, CancellationToken, Task<HttpResponseMessage>> script)
        {
            this.Handler = new RecordingHandler((number, request, token) => script(this, number, request, token));
        }

        internal RecordingHandler Handler { get; }

        internal int ExchangeCount => this.Handler.Count;

        internal IReadOnlyList<RecordedRequest> Requests => this.Handler.Requests;

        /// <summary>An endpoint whose script can wait, for the cases that hold a response.</summary>
        internal static TokenEndpoint Async(Func<TokenEndpoint, int, RecordedRequest, CancellationToken, Task<HttpResponseMessage>> script)
        {
            return new TokenEndpoint(script);
        }

        /// <summary>
        /// The success response of the fixtures, with the next access token. Pass
        /// <c>null</c> for <paramref name="expiresIn"/> to leave the field out.
        /// </summary>
        internal HttpResponseMessage Success(int? expiresIn = 300, string? accessToken = null)
        {
            var number = Interlocked.Increment(ref this.issued);
            var body = new Newtonsoft.Json.Linq.JObject
            {
                ["access_token"] = accessToken ?? $"access-{number}",
                ["token_type"] = "Bearer",
            };
            if (expiresIn != null)
            {
                body["expires_in"] = expiresIn.Value;
            }

            return Wif.Json(HttpStatusCode.OK, body.ToString());
        }
    }

    /// <summary>An identity token provider that counts its invocations. The first call is number 1.</summary>
    internal sealed class CountingProvider : IIdentityTokenProvider
    {
        private readonly Func<int, string> next;
        private int count;

        internal CountingProvider(string assertion)
            : this(_ => assertion)
        {
        }

        internal CountingProvider(Func<int, string> next)
        {
            this.next = next;
        }

        /// <summary>The provider count: how many times the provider was invoked.</summary>
        internal int Count => Volatile.Read(ref this.count);

        public Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(this.next(Interlocked.Increment(ref this.count)));
        }
    }

    /// <summary>The injected sleeper: records each requested delay and returns at once.</summary>
    internal sealed class SleepRecorder
    {
        private readonly List<TimeSpan> delays = new List<TimeSpan>();

        internal IReadOnlyList<TimeSpan> Delays
        {
            get
            {
                lock (this.delays)
                {
                    return this.delays.ToArray();
                }
            }
        }

        internal Task SleepAsync(TimeSpan delay, CancellationToken cancellationToken)
        {
            lock (this.delays)
            {
                this.delays.Add(delay);
            }

            return Task.CompletedTask;
        }
    }

    /// <summary>Sets environment variables for one test and puts the old values back when disposed.</summary>
    internal sealed class EnvironmentScope : IDisposable
    {
        private readonly Dictionary<string, string?> original = new Dictionary<string, string?>();

        public void Dispose()
        {
            foreach (var entry in this.original)
            {
                Environment.SetEnvironmentVariable(entry.Key, entry.Value);
            }

            this.original.Clear();
        }

        /// <summary>Sets a variable, or removes it when <paramref name="value"/> is null.</summary>
        internal EnvironmentScope Set(string name, string? value)
        {
            if (!this.original.ContainsKey(name))
            {
                this.original[name] = Environment.GetEnvironmentVariable(name);
            }

            Environment.SetEnvironmentVariable(name, value);
            return this;
        }
    }
}
