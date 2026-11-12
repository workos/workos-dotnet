// @oagen-ignore-file
namespace WorkOS
{
    using System;

    /// <summary>
    /// The fixed values of workload identity federation. Each one is defined here and nowhere else.
    /// </summary>
    internal static class WorkloadIdentityConstants
    {
        /// <summary>The header that carries the environment ID on every API request.</summary>
        internal const string EnvironmentHeader = "x-url-environment-id";

        /// <summary>The OAuth grant type of the exchange.</summary>
        internal const string GrantType = "urn:ietf:params:oauth:grant-type:jwt-bearer";

        /// <summary>The <c>resource</c> form field when none is configured.</summary>
        internal const string DefaultResource = "https://api.workos.com";

        /// <summary>The share of a token's lifetime that counts as "about to expire".</summary>
        internal const double RefreshThresholdFraction = 0.2;

        /// <summary>The longest <see cref="WorkloadIdentityException.Description"/> kept from a server response.</summary>
        internal const int DescriptionLimit = 500;

        /// <summary>Environment variable that supplies the default client ID.</summary>
        internal const string ClientIdVariable = "WORKOS_CLIENT_ID";

        /// <summary>Environment variable that supplies the default token URL.</summary>
        internal const string TokenUrlVariable = "WORKOS_WIF_TOKEN_URL";

        /// <summary>Environment variable that supplies the default scope.</summary>
        internal const string ScopeVariable = "WORKOS_WIF_SCOPE";

        /// <summary>Environment variable that supplies the default environment ID.</summary>
        internal const string EnvironmentIdVariable = "WORKOS_ENVIRONMENT_ID";

        /// <summary>The refresh threshold never exceeds this, however long the token lives.</summary>
        internal static readonly TimeSpan RefreshThresholdCap = TimeSpan.FromSeconds(60);

        /// <summary>The lifetime assumed when the token response has no usable <c>expires_in</c>.</summary>
        internal static readonly TimeSpan DefaultLifetime = TimeSpan.FromSeconds(300);

        /// <summary>The time allowed for one whole exchange, including its retries and backoff.</summary>
        internal static readonly TimeSpan ExchangeDeadline = TimeSpan.FromSeconds(30);
    }
}
