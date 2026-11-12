// @oagen-ignore-file
namespace WorkOS.WorkloadIdentity
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Identity tokens from GitHub Actions' OIDC provider, for
    /// <see cref="WorkloadIdentityCredentialsOptions.IdentityTokenProvider"/>.
    /// The workflow or job needs <c>permissions: id-token: write</c>.
    /// </summary>
    public static class GitHubActions
    {
        /// <summary>The variable GitHub Actions sets to the URL that issues the job's OIDC token.</summary>
        internal const string RequestUrlVariable = "ACTIONS_ID_TOKEN_REQUEST_URL";

        /// <summary>The variable GitHub Actions sets to the bearer token for that URL.</summary>
        internal const string RequestTokenVariable = "ACTIONS_ID_TOKEN_REQUEST_TOKEN";

        /// <summary>How long one request for a token may take.</summary>
        private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(10);

        /// <summary>
        /// The one HTTP client of this package: it does not follow redirects and gives up
        /// after 10 seconds. It holds no token.
        /// </summary>
        internal static HttpClient Http { get; } = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            Timeout = HttpTimeout,
        };

        /// <summary>
        /// Creates a provider that asks GitHub Actions for an OIDC token with the given audience.
        /// </summary>
        /// <param name="audience">
        /// The <c>aud</c> claim to request. It must match the audience of the trust rule;
        /// the conventional value is <c>https://api.workos.com</c>.
        /// </param>
        /// <returns>A provider that fetches a token on every call. It neither caches nor retries; the SDK does both.</returns>
        public static IIdentityTokenProvider Create(string audience)
        {
            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new ArgumentException("An audience is required.", nameof(audience));
            }

            return new Provider(audience);
        }

        private sealed class Provider : IIdentityTokenProvider
        {
            private readonly string audience;

            internal Provider(string audience) => this.audience = audience;

            public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
            {
                // Read on every call: both are set per job, and a test may change them.
                var url = Environment.GetEnvironmentVariable(RequestUrlVariable);
                var requestToken = Environment.GetEnvironmentVariable(RequestTokenVariable);
                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(requestToken))
                {
                    throw new InvalidOperationException(
                        "GitHub Actions did not provide an OIDC token endpoint. Add `permissions: id-token: write` to the workflow or job.");
                }

                // The URL already has a query string. Add to it; do not replace it.
                var separator = url.Contains('?') ? "&" : "?";
                var requestUrl = $"{url}{separator}audience={Uri.EscapeDataString(this.audience)}";

                using var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", requestToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                using var response = await Http.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException(
                        $"GitHub Actions' OIDC token endpoint returned HTTP {(int)response.StatusCode}.",
                        inner: null,
                        statusCode: response.StatusCode);
                }

                // The body holds the token, so it never goes into an error.
                var token = ReadValue(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("GitHub Actions' OIDC token response has no token in its \"value\" field.");
                }

                return token;
            }

            private static string? ReadValue(string body)
            {
                try
                {
                    using var document = JsonDocument.Parse(body);
                    return document.RootElement.ValueKind == JsonValueKind.Object
                        && document.RootElement.TryGetProperty("value", out var value)
                        && value.ValueKind == JsonValueKind.String
                            ? value.GetString()
                            : null;
                }
                catch (JsonException)
                {
                    return null;
                }
            }
        }
    }
}
