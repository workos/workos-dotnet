// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Settings for <see cref="WorkloadIdentityCredentials"/>. They are read once, when the
    /// credentials are constructed; to change one, build new credentials.
    /// </summary>
    public class WorkloadIdentityCredentialsOptions
    {
        /// <summary>
        /// The client ID of the M2M application that holds the trust rule.
        /// Defaults to the <c>WORKOS_CLIENT_ID</c> environment variable. Required.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// The token endpoint that exchanges the identity token for an access token.
        /// Defaults to the <c>WORKOS_WIF_TOKEN_URL</c> environment variable. Required; there
        /// is no built-in default.
        /// </summary>
        public string? TokenUrl { get; set; }

        /// <summary>
        /// The environment the requests are for. Optional. When set, it is sent as the
        /// <c>x-url-environment-id</c> header on every API request; when not set, the header
        /// is not sent. Defaults to the <c>WORKOS_ENVIRONMENT_ID</c> environment variable.
        /// An empty string counts as not set.
        /// <para>
        /// The server binds a workload identity client to exactly one environment and takes
        /// the environment from that binding, so the header selects nothing. Setting it is a
        /// safety check: a value that names a different environment is refused with
        /// <c>403 environment_mismatch</c>.
        /// </para>
        /// </summary>
        public string? EnvironmentId { get; set; }

        /// <summary>
        /// The resource the access token is requested for. Defaults to <c>https://api.workos.com</c>.
        /// </summary>
        public string? Resource { get; set; }

        /// <summary>
        /// The scope to request. Defaults to the <c>WORKOS_WIF_SCOPE</c> environment variable;
        /// left out of the exchange when neither is set.
        /// </summary>
        public string? Scope { get; set; }

        /// <summary>
        /// Returns the workload's identity token. Use a provider package
        /// (<c>WorkOS.WorkloadIdentity.GitHubActions</c>, <c>.Gcp</c>, <c>.Aws</c>),
        /// <see cref="WorkOS.IdentityTokenProvider.TokenFile"/>,
        /// <see cref="WorkOS.IdentityTokenProvider.TokenEnv"/>, or your own implementation.
        /// Set this or <see cref="IdentityTokenCallback"/>, not both.
        /// </summary>
        public IIdentityTokenProvider? IdentityTokenProvider { get; set; }

        /// <summary>
        /// A function that returns the workload's identity token, for sources that do not
        /// need a class of their own. Set this or <see cref="IdentityTokenProvider"/>, not both.
        /// </summary>
        public Func<CancellationToken, Task<string>>? IdentityTokenCallback { get; set; }
    }
}
