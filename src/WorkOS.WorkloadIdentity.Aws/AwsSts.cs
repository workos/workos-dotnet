// @oagen-ignore-file
namespace WorkOS.WorkloadIdentity
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Amazon;
    using Amazon.Runtime;
    using Amazon.SecurityToken;
    using Amazon.SecurityToken.Model;

    /// <summary>
    /// Identity tokens from AWS STS (<c>GetWebIdentityToken</c>), for
    /// <see cref="WorkloadIdentityCredentialsOptions.IdentityTokenProvider"/>.
    /// The caller's IAM principal needs the <c>sts:GetWebIdentityToken</c> permission.
    /// </summary>
    public static class AwsSts
    {
        /// <summary>The signing algorithm requested for the token.</summary>
        internal const string SigningAlgorithm = "RS256";

        // The pseudo-region whose STS endpoint is the global one.
        private const string GlobalRegion = "aws-global";

        /// <summary>
        /// Creates a provider that asks STS for a web identity token with the given audience.
        /// The STS client is built once, here, from the AWS SDK's default credential chain.
        /// </summary>
        /// <param name="audience">
        /// The <c>aud</c> claim to request. It must match the audience of the trust rule;
        /// the conventional value is <c>https://api.workos.com</c>.
        /// </param>
        /// <param name="region">
        /// The AWS region whose STS endpoint to call, for example <c>us-east-1</c>. When
        /// omitted, the region the AWS SDK resolves (<c>AWS_REGION</c>, the shared config
        /// file, instance metadata) is used.
        /// </param>
        /// <returns>A provider that fetches a token on every call. It neither caches nor retries; the SDK does both.</returns>
        /// <exception cref="InvalidOperationException">No region was given and the AWS SDK could not resolve one.</exception>
        public static IIdentityTokenProvider Create(string audience, string? region = null)
        {
            RequireAudience(audience);
            var config = BuildConfig(region, FallbackRegionFactory.GetRegionEndpoint);
            return new Provider(audience, new AmazonSecurityTokenServiceClient(config));
        }

        /// <summary>
        /// Creates a provider on an existing STS client. The seam tests use to stub STS.
        /// </summary>
        /// <param name="audience">The <c>aud</c> claim to request.</param>
        /// <param name="client">The STS client to call.</param>
        /// <returns>A provider that fetches a token on every call.</returns>
        internal static IIdentityTokenProvider Create(string audience, IAmazonSecurityTokenService client)
        {
            RequireAudience(audience);
            return new Provider(audience, client);
        }

        /// <summary>
        /// Builds the STS client configuration for a region. Always regional: STS is never
        /// called on its global endpoint.
        /// </summary>
        /// <param name="region">The region the caller asked for, if any.</param>
        /// <param name="resolveRegion">Finds a region when the caller gave none; returns null when there is none.</param>
        /// <returns>The configuration to build the STS client from.</returns>
        internal static AmazonSecurityTokenServiceConfig BuildConfig(string? region, Func<RegionEndpoint?> resolveRegion)
        {
            var endpoint = string.IsNullOrWhiteSpace(region)
                ? resolveRegion()
                : RegionEndpoint.GetBySystemName(region);

            if (endpoint == null)
            {
                throw new InvalidOperationException(
                    "No AWS region is configured. Pass a region to AwsSts.Create, or set AWS_REGION.");
            }

            if (string.Equals(endpoint.SystemName, GlobalRegion, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The STS global endpoint is not supported. Use a regional endpoint, for example us-east-1.");
            }

            return new AmazonSecurityTokenServiceConfig { RegionEndpoint = endpoint };
        }

        private static void RequireAudience(string audience)
        {
            if (string.IsNullOrWhiteSpace(audience))
            {
                throw new ArgumentException("An audience is required.", nameof(audience));
            }
        }

        private sealed class Provider : IIdentityTokenProvider
        {
            private readonly string audience;
            private readonly IAmazonSecurityTokenService client;

            internal Provider(string audience, IAmazonSecurityTokenService client)
            {
                this.audience = audience;
                this.client = client;
            }

            public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
            {
                // DurationSeconds is left unset, so STS applies its default lifetime.
                var request = new GetWebIdentityTokenRequest
                {
                    Audience = new List<string> { this.audience },
                    SigningAlgorithm = SigningAlgorithm,
                };

                GetWebIdentityTokenResponse response;
                try
                {
                    response = await this.client.GetWebIdentityTokenAsync(request, cancellationToken).ConfigureAwait(false);
                }
                catch (AmazonServiceException ex)
                {
                    throw new InvalidOperationException($"sts:GetWebIdentityToken failed with {ex.ErrorCode}.", ex);
                }
                catch (AmazonClientException ex)
                {
                    throw new InvalidOperationException("sts:GetWebIdentityToken could not be called.", ex);
                }

                if (string.IsNullOrEmpty(response?.WebIdentityToken))
                {
                    throw new InvalidOperationException("sts:GetWebIdentityToken returned no token.");
                }

                return response.WebIdentityToken;
            }
        }
    }
}
