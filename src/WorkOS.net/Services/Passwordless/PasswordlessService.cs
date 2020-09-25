namespace WorkOS
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS Passwordless.
    /// </summary>
    public class PasswordlessService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordlessService"/> class.
        /// </summary>
        public PasswordlessService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordlessService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public PasswordlessService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Creates a Passwordless Session for authenticating a user.
        /// </summary>
        /// <param name="options">Parameters to create the Passwordless Session.</param>
        /// <returns>The created Passwordless Session.</returns>
        public PasswordlessSession CreateSession(CreatePasswordlessSessionOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/passwordless/sessions",
            };

            return this.Client.MakeAPIRequest<PasswordlessSession>(request);
        }

        /// <summary>
        /// Asynchronously creates a Passwordless Session for authenticating a user.
        /// </summary>
        /// <param name="options">Parameters to create the Passwordless Session.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping the created Passwordless Session.</returns>
        public async Task<PasswordlessSession> CreateSessionAsync(
            CreatePasswordlessSessionOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/passwordless/sessions",
            };

            return await this.Client.MakeAPIRequestAsync<PasswordlessSession>(request, cancellationToken);
        }

        /// <summary>
        /// Sends the link to confirm a Passwordless Session.
        /// </summary>
        /// <param name="id">Passwordless Session identifier.</param>
        /// <returns>True if successful.</returns>
        public bool SendSession(string id)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = $"/passwordless/sessions/{id}/send",
            };

            this.Client.MakeAPIRequest<object>(request);
            return true;
        }

        /// <summary>
        /// Asynchronously sends the link to confirm a Passwordless Session.
        /// </summary>
        /// <param name="id">Passwordless Session identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping True if successful.</returns>
        public async Task<bool> SendSessionAsync(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = $"/passwordless/sessions/{id}/send",
            };

            await this.Client.MakeAPIRequestAsync<object>(request, cancellationToken);
            return true;
        }
    }
}
