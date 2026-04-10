// @oagen-ignore-file
namespace WorkOS
{
    using System;

    /// <summary>Options for creating a public (PKCE-only) WorkOS client.</summary>
    public class PublicWorkOSOptions
    {
        /// <summary>Gets or sets the client ID (required).</summary>
        public string ClientId { get; set; } = default!;

        /// <summary>Gets or sets the base URL (optional, defaults to https://api.workos.com).</summary>
        public string? ApiBaseURL { get; set; }
    }

    /// <summary>
    /// A restricted WorkOS client for public/PKCE-only flows (H19).
    /// Does not require an API key. Only exposes helpers safe for
    /// browser, mobile, CLI, and desktop applications.
    /// </summary>
    public class PublicWorkOSClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PublicWorkOSClient"/> class.
        /// </summary>
        /// <param name="options">Public client options (requires client_id, no API key).</param>
        public PublicWorkOSClient(PublicWorkOSOptions options)
        {
            if (string.IsNullOrEmpty(options.ClientId))
            {
                throw new ArgumentException("Client ID is required.", nameof(options.ClientId));
            }

            this.ClientId = options.ClientId;
            this.ApiBaseURL = options.ApiBaseURL ?? WorkOSClient.DefaultApiBaseURL;
        }

        /// <summary>Gets the client ID.</summary>
        public string ClientId { get; }

        /// <summary>Gets the API base URL.</summary>
        public string ApiBaseURL { get; }

        /// <summary>Build an AuthKit authorization URL.</summary>
        public string GetAuthorizationUrl(AuthKitAuthorizationUrlOptions options)
        {
            return AuthorizationUrlBuilder.BuildAuthKitAuthorizationUrl(
                this.ApiBaseURL,
                this.ClientId,
                options);
        }

        /// <summary>Build an AuthKit authorization URL with automatic PKCE.</summary>
        public PkceAuthorizationUrlResult GetAuthorizationUrlWithPkce(AuthKitAuthorizationUrlOptions options)
        {
            return AuthorizationUrlBuilder.BuildAuthKitAuthorizationUrlWithPkce(
                this.ApiBaseURL,
                this.ClientId,
                options);
        }

        /// <summary>Build an SSO authorization URL.</summary>
        public string GetSSOAuthorizationUrl(SSOAuthorizationUrlOptions options)
        {
            return AuthorizationUrlBuilder.BuildSSOAuthorizationUrl(
                this.ApiBaseURL,
                this.ClientId,
                options);
        }

        /// <summary>Build an SSO authorization URL with automatic PKCE.</summary>
        public PkceAuthorizationUrlResult GetSSOAuthorizationUrlWithPkce(SSOAuthorizationUrlOptions options)
        {
            return AuthorizationUrlBuilder.BuildSSOAuthorizationUrlWithPkce(
                this.ApiBaseURL,
                this.ClientId,
                options);
        }

        /// <summary>Build a logout URL.</summary>
        public string GetLogoutUrl(string sessionId, string? returnTo = null)
        {
            return AuthorizationUrlBuilder.BuildLogoutUrl(this.ApiBaseURL, sessionId, returnTo);
        }

        /// <summary>Get the JWKS URL for the current client.</summary>
        public string GetJwksUrl()
        {
            return $"{this.ApiBaseURL}/sso/jwks/{this.ClientId}";
        }
    }
}
