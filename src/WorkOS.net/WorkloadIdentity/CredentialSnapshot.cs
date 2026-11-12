// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// The credential one HTTP attempt is sent with. Immutable.
    /// </summary>
    public sealed class CredentialSnapshot
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CredentialSnapshot"/> class.
        /// </summary>
        /// <param name="headers">The authorization headers to set on the attempt.</param>
        /// <param name="generation">Identifies the credential the headers came from.</param>
        /// <param name="recoverable">Whether a 401 on this snapshot is worth resolving again once.</param>
        public CredentialSnapshot(IReadOnlyDictionary<string, string> headers, long generation, bool recoverable)
        {
            if (headers == null)
            {
                throw new ArgumentNullException(nameof(headers));
            }

            // Copied, so a caller that keeps the dictionary cannot change a snapshot in use.
            var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var header in headers)
            {
                copy[header.Key] = header.Value;
            }

            this.Headers = new ReadOnlyDictionary<string, string>(copy);
            this.Generation = generation;
            this.Recoverable = recoverable;
        }

        /// <summary>
        /// The authorization headers to set on this HTTP attempt. They replace any header of
        /// the same name already on the request.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>
        /// Identifies the credential these headers came from. Pass it to
        /// <see cref="ICredentials.Invalidate"/> when the API rejects this snapshot.
        /// </summary>
        public long Generation { get; }

        /// <summary>
        /// Whether a 401 response to a request sent with this snapshot is worth one more
        /// resolve and one resend. False for a static API key.
        /// </summary>
        public bool Recoverable { get; }
    }
}
