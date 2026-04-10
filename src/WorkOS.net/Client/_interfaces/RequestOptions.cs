// @oagen-ignore-file
namespace WorkOS
{
    /// <summary>
    /// Per-request configuration overrides.
    /// </summary>
    public class RequestOptions
    {
        /// <summary>
        /// Override the API key for this request.
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// Idempotency key to ensure safe retries.
        /// </summary>
        public string? IdempotencyKey { get; set; }
    }
}
