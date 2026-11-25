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
        /// Required. It is set here only: no environment variable is read for it.
        /// </summary>
        public string? ClientId { get; set; }

        /// <summary>
        /// The token endpoint that exchanges the identity token for an access token.
        /// Required. It is set here only: there is no built-in default, and no environment
        /// variable is read for it.
        /// <para>
        /// It must be an absolute <c>https</c> URL. The request to it carries the workload's
        /// identity token and the response carries the access token, so any other URL,
        /// <c>http://localhost</c> included, is rejected when the credentials are constructed,
        /// with code <c>invalid_configuration</c>.
        /// </para>
        /// </summary>
        public string? TokenUrl { get; set; }

        /// <summary>
        /// The environment the requests are for. Optional. When set, it is sent as the
        /// <c>x-url-environment-id</c> header on every API request; when not set, the header
        /// is not sent. It is set here only: no environment variable is read for it.
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
        /// The scope to request. Optional; left out of the exchange when not set or empty.
        /// It is set here only: no environment variable is read for it.
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
