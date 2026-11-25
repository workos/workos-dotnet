// @oagen-ignore-file
namespace WorkOS
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Returns the workload's identity token: the JWT that the platform the code runs on
    /// (GitHub Actions, Google Cloud, AWS, or your own source) issues for it.
    /// The SDK calls the provider once per exchange attempt and retries a failure, so a
    /// provider neither caches nor retries.
    /// </summary>
    public interface IIdentityTokenProvider
    {
        /// <summary>
        /// Returns the identity token.
        /// </summary>
        /// <param name="cancellationToken">Canceled when the exchange runs out of time.</param>
        /// <returns>The JWT. An empty value counts as a failure.</returns>
        Task<string> GetTokenAsync(CancellationToken cancellationToken);
    }
}
