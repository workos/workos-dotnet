// @oagen-ignore-file
namespace WorkOS
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Credentials that never change: a static API key, or a per-request key or access token.
    /// A 401 on them is final, because resolving again would return the same value.
    /// </summary>
    internal sealed class StaticKeyCredentials : IWorkOSCredentials
    {
        private readonly Task<CredentialSnapshot> resolved;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticKeyCredentials"/> class.
        /// </summary>
        /// <param name="key">The API key or access token to send as a bearer token.</param>
        internal StaticKeyCredentials(string key)
        {
            this.resolved = Task.FromResult(Snapshot(key));
        }

        /// <inheritdoc/>
        public Task<CredentialSnapshot> ResolveAsync(CancellationToken cancellationToken)
        {
            return this.resolved;
        }

        /// <inheritdoc/>
        public void Invalidate(long generation)
        {
        }

        /// <summary>
        /// Builds the snapshot for a fixed bearer value.
        /// </summary>
        /// <param name="key">The API key or access token.</param>
        /// <returns>A snapshot that is not recoverable.</returns>
        internal static CredentialSnapshot Snapshot(string key)
        {
            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer " + key,
            };
            return new CredentialSnapshot(headers, generation: 0, recoverable: false);
        }
    }
}
