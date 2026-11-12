// @oagen-ignore-file
namespace WorkOS
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Supplies the authorization headers for requests to the WorkOS API. A client resolves
    /// a snapshot before every HTTP attempt, so short-lived credentials are never sent stale.
    /// </summary>
    public interface IWorkOSCredentials
    {
        /// <summary>
        /// Returns the credential to use for one HTTP attempt. May wait for a new token.
        /// </summary>
        /// <param name="cancellationToken">Stops this caller's wait. It never cancels work shared with other callers.</param>
        /// <returns>The headers to send, and the generation they came from.</returns>
        Task<CredentialSnapshot> ResolveAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Reports that the API rejected the credential of the given generation. Has no effect
        /// when a newer credential is already in place. Never waits on the network.
        /// </summary>
        /// <param name="generation">The <see cref="CredentialSnapshot.Generation"/> of the rejected snapshot.</param>
        void Invalidate(long generation);
    }
}
