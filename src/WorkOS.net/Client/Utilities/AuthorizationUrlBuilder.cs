// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Web;

    /// <summary>Options for building an AuthKit authorization URL.</summary>
    public class AuthKitAuthorizationUrlOptions
    {
        public string RedirectUri { get; set; } = default!;

        public string? ConnectionId { get; set; }

        public string? OrganizationId { get; set; }

        public string? Provider { get; set; }

        public string? State { get; set; }

        public string? DomainHint { get; set; }

        public string? LoginHint { get; set; }

        public string? CodeChallenge { get; set; }

        public string? CodeChallengeMethod { get; set; }

        public string? ScreenHint { get; set; }

        public string? Prompt { get; set; }

        public IEnumerable<string>? ProviderScopes { get; set; }

        public Dictionary<string, string>? ProviderQueryParams { get; set; }
    }

    /// <summary>Options for building an SSO authorization URL.</summary>
    public class SSOAuthorizationUrlOptions
    {
        public string RedirectUri { get; set; } = default!;

        public string? Connection { get; set; }

        public string? Organization { get; set; }

        public string? Provider { get; set; }

        public string? State { get; set; }

        public string? DomainHint { get; set; }

        public string? LoginHint { get; set; }

        public string? Nonce { get; set; }

        public string? CodeChallenge { get; set; }

        public string? CodeChallengeMethod { get; set; }

        public IEnumerable<string>? ProviderScopes { get; set; }

        public Dictionary<string, string>? ProviderQueryParams { get; set; }
    }

    /// <summary>Result of generating an authorization URL with PKCE.</summary>
    public class PkceAuthorizationUrlResult
    {
        public string Url { get; set; } = default!;

        public string State { get; set; } = default!;

        public string CodeVerifier { get; set; } = default!;
    }

    /// <summary>
    /// URL builder helpers for AuthKit and SSO authorization flows (H09-H12, H14-H17).
    /// </summary>
    public static class AuthorizationUrlBuilder
    {
        /// <summary>Build an AuthKit authorization URL (H09).</summary>
        /// <param name="baseUrl">The WorkOS API base URL.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="options">Authorization URL options.</param>
        /// <returns>The authorization URL string.</returns>
        public static string BuildAuthKitAuthorizationUrl(
            string baseUrl,
            string clientId,
            AuthKitAuthorizationUrlOptions options)
        {
            if (string.IsNullOrEmpty(options.Provider) &&
                string.IsNullOrEmpty(options.ConnectionId) &&
                string.IsNullOrEmpty(options.OrganizationId))
            {
                // AuthKit allows omitting these (provider defaults to authkit)
            }

            var queryParams = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "redirect_uri", options.RedirectUri },
                { "response_type", "code" },
            };

            AddIfNotNull(queryParams, "connection_id", options.ConnectionId);
            AddIfNotNull(queryParams, "organization_id", options.OrganizationId);
            AddIfNotNull(queryParams, "provider", options.Provider);
            AddIfNotNull(queryParams, "state", options.State);
            AddIfNotNull(queryParams, "domain_hint", options.DomainHint);
            AddIfNotNull(queryParams, "login_hint", options.LoginHint);
            AddIfNotNull(queryParams, "code_challenge", options.CodeChallenge);
            AddIfNotNull(queryParams, "code_challenge_method", options.CodeChallengeMethod);
            AddIfNotNull(queryParams, "screen_hint", options.ScreenHint);
            AddIfNotNull(queryParams, "prompt", options.Prompt);

            if (options.ProviderScopes != null)
            {
                queryParams["provider_scopes"] = string.Join(" ", options.ProviderScopes);
            }

            var url = $"{baseUrl}/user_management/authorize?{BuildQueryString(queryParams)}";

            if (options.ProviderQueryParams != null)
            {
                foreach (var kv in options.ProviderQueryParams)
                {
                    url += $"&provider_query_params[{Uri.EscapeDataString(kv.Key)}]={Uri.EscapeDataString(kv.Value)}";
                }
            }

            return url;
        }

        /// <summary>Build an AuthKit authorization URL with automatic PKCE (H10).</summary>
        /// <param name="baseUrl">The WorkOS API base URL.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="options">Authorization URL options (code_challenge/method are auto-generated).</param>
        /// <returns>The URL, state, and code verifier for the PKCE flow.</returns>
        public static PkceAuthorizationUrlResult BuildAuthKitAuthorizationUrlWithPkce(
            string baseUrl,
            string clientId,
            AuthKitAuthorizationUrlOptions options)
        {
            var pkce = PkceUtilities.Generate();
            var state = PkceUtilities.GenerateState();

            options.CodeChallenge = pkce.CodeChallenge;
            options.CodeChallengeMethod = pkce.CodeChallengeMethod;
            options.State = state;

            var url = BuildAuthKitAuthorizationUrl(baseUrl, clientId, options);

            return new PkceAuthorizationUrlResult
            {
                Url = url,
                State = state,
                CodeVerifier = pkce.CodeVerifier,
            };
        }

        /// <summary>Build an SSO authorization URL (H14).</summary>
        /// <param name="baseUrl">The WorkOS API base URL.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="options">SSO authorization URL options.</param>
        /// <returns>The authorization URL string.</returns>
        public static string BuildSSOAuthorizationUrl(
            string baseUrl,
            string clientId,
            SSOAuthorizationUrlOptions options)
        {
            if (string.IsNullOrEmpty(options.Connection) &&
                string.IsNullOrEmpty(options.Organization) &&
                string.IsNullOrEmpty(options.Provider))
            {
                throw new ArgumentException("One of connection, organization, or provider is required.");
            }

            var queryParams = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "redirect_uri", options.RedirectUri },
                { "response_type", "code" },
            };

            AddIfNotNull(queryParams, "connection", options.Connection);
            AddIfNotNull(queryParams, "organization", options.Organization);
            AddIfNotNull(queryParams, "provider", options.Provider);
            AddIfNotNull(queryParams, "state", options.State);
            AddIfNotNull(queryParams, "domain_hint", options.DomainHint);
            AddIfNotNull(queryParams, "login_hint", options.LoginHint);
            AddIfNotNull(queryParams, "nonce", options.Nonce);
            AddIfNotNull(queryParams, "code_challenge", options.CodeChallenge);
            AddIfNotNull(queryParams, "code_challenge_method", options.CodeChallengeMethod);

            if (options.ProviderScopes != null)
            {
                queryParams["provider_scopes"] = string.Join(" ", options.ProviderScopes);
            }

            var url = $"{baseUrl}/sso/authorize?{BuildQueryString(queryParams)}";

            if (options.ProviderQueryParams != null)
            {
                foreach (var kv in options.ProviderQueryParams)
                {
                    url += $"&provider_query_params[{Uri.EscapeDataString(kv.Key)}]={Uri.EscapeDataString(kv.Value)}";
                }
            }

            return url;
        }

        /// <summary>Build an SSO authorization URL with automatic PKCE (H15).</summary>
        /// <param name="baseUrl">The WorkOS API base URL.</param>
        /// <param name="clientId">The client ID.</param>
        /// <param name="options">SSO authorization URL options (code_challenge/method are auto-generated).</param>
        /// <returns>The URL, state, and code verifier for the PKCE flow.</returns>
        public static PkceAuthorizationUrlResult BuildSSOAuthorizationUrlWithPkce(
            string baseUrl,
            string clientId,
            SSOAuthorizationUrlOptions options)
        {
            var pkce = PkceUtilities.Generate();
            var state = PkceUtilities.GenerateState();

            options.CodeChallenge = pkce.CodeChallenge;
            options.CodeChallengeMethod = pkce.CodeChallengeMethod;
            options.State = state;

            var url = BuildSSOAuthorizationUrl(baseUrl, clientId, options);

            return new PkceAuthorizationUrlResult
            {
                Url = url,
                State = state,
                CodeVerifier = pkce.CodeVerifier,
            };
        }

        /// <summary>Build a logout URL for UserManagement.</summary>
        /// <param name="baseUrl">The WorkOS API base URL.</param>
        /// <param name="sessionId">The session ID.</param>
        /// <param name="returnTo">Optional URL to redirect to after logout.</param>
        /// <returns>The logout URL string.</returns>
        public static string BuildLogoutUrl(string baseUrl, string sessionId, string? returnTo = null)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentException("Session ID is required.", nameof(sessionId));
            }

            var url = $"{baseUrl}/user_management/sessions/logout?session_id={Uri.EscapeDataString(sessionId)}";
            if (!string.IsNullOrEmpty(returnTo))
            {
                url += $"&return_to={Uri.EscapeDataString(returnTo)}";
            }

            return url;
        }

        /// <summary>Build an SSO logout URL.</summary>
        /// <param name="baseUrl">The WorkOS API base URL.</param>
        /// <param name="token">The logout token.</param>
        /// <param name="returnTo">Optional URL to redirect to after logout.</param>
        /// <returns>The SSO logout URL string.</returns>
        public static string BuildSSOLogoutUrl(string baseUrl, string token, string? returnTo = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Token is required.", nameof(token));
            }

            var url = $"{baseUrl}/sso/logout?token={Uri.EscapeDataString(token)}";
            if (!string.IsNullOrEmpty(returnTo))
            {
                url += $"&return_to={Uri.EscapeDataString(returnTo)}";
            }

            return url;
        }

        private static void AddIfNotNull(Dictionary<string, string> dict, string key, string? value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                dict[key] = value;
            }
        }

        private static string BuildQueryString(Dictionary<string, string> queryParams)
        {
            var sb = new StringBuilder();
            bool first = true;
            foreach (var kv in queryParams)
            {
                if (!first)
                {
                    sb.Append('&');
                }

                sb.Append(Uri.EscapeDataString(kv.Key));
                sb.Append('=');
                sb.Append(Uri.EscapeDataString(kv.Value));
                first = false;
            }

            return sb.ToString();
        }
    }
}
