namespace WorkOS
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS Audit Logs API.
    /// </summary>
    public class UserManagementService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementService"/> class.
        /// </summary>
        public UserManagementService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserManagementService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public UserManagementService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Get the details of an existing user.
        /// </summary>
        /// <param name="id">The unique ID of the User.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A WorkOS User record.</returns>
        public async Task<User> GetUser(
            string id,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/users/{id}",
            };
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }
    }
}