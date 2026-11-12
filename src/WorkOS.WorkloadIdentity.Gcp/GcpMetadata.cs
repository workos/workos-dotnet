// @oagen-ignore-file
namespace WorkOS.WorkloadIdentity
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Identity tokens from the Google Cloud metadata server, for
    /// <see cref="WorkloadIdentityCredentialsOptions.IdentityTokenProvider"/>.
    /// Works wherever the metadata server is reachable: Compute Engine, GKE, Cloud Run,
    /// Cloud Functions and App Engine. The token is for the default service account.
    /// </summary>
    public static class GcpMetadata
    {
        /// <summary>The metadata server. Only tests point the provider anywhere else.</summary>
        internal const string DefaultBaseUrl = "http://metadata.google.internal";

        private const string IdentityPath = "/computeMetadata/v1/instance/service-accounts/default/identity";

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
        /// Creates a provider that asks the metadata server for an identity token with the
        /// given audience.
        /// </summary>
        /// <param name="audience">
        /// The <c>aud</c> claim to request. It must match the audience of the trust rule;
        /// the conventional value is <c>https://api.workos.com</c>.
        /// </param>
        /// <returns>A provider that fetches a token on every call. It neither caches nor retries; the SDK does both.</returns>
        public static IIdentityTokenProvider Create(string audience)
        {
            return Create(audience, DefaultBaseUrl);
        }

        /// <summary>
        /// Creates a provider against another base URL. For tests only: this is not a public
        /// option and is not read from the environment.
        /// </summary>
        /// <param name="audience">The <c>aud</c> claim to request.</param>
        /// <param name="baseUrl">The origin that stands in for the metadata server.</param>
        /// <returns>A provider that fetches a token on every call.</returns>
        internal static IIdentityTokenProvider Create(string audience, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new ArgumentException("An audience is required.", nameof(audience));
            }

            return new Provider($"{baseUrl}{IdentityPath}?audience={Uri.EscapeDataString(audience)}&format=full");
        }

        private sealed class Provider : IIdentityTokenProvider
        {
            private readonly string url;

            internal Provider(string url) => this.url = url;

            public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, this.url);
                request.Headers.TryAddWithoutValidation("Metadata-Flavor", "Google");

                using var response = await Http.SendAsync(request, cancellationToken).ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new HttpRequestException(
                        $"The Google Cloud metadata server returned HTTP {(int)response.StatusCode}.",
                        inner: null,
                        statusCode: response.StatusCode);
                }

                // The body is the token, so it never goes into an error.
                var token = (await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false)).Trim();
                if (token.Length == 0)
                {
                    throw new InvalidOperationException("The Google Cloud metadata server returned an empty identity token.");
                }

                return token;
            }
        }
    }
}
