namespace WorkOS
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS UserManagment API.
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
        /// <returns> A User record.</returns>
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

        /// <summary>
        /// Create a new User.
        /// </summary>
        /// <param name="options"> Parameters used to create the user.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A User record.</returns>
        public async Task<User> CreateUser(
            CreateUserOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users",
            };
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Authenticate user session with a password.
        /// </summary>
        /// <param name="options"> Parameters used to create the user.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A User and Session record.</returns>
        public async Task<(User, Session)> AuthenticateUserWithPassword(
            AuthenticateUserWithPasswordOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/sessions/token",
            };
            return await this.Client.MakeAPIRequest<(User, Session)>(request, cancellationToken);
        }

        /// <summary>
        /// Adds an unmanaged User as a member of the given Organization.
        /// </summary>
        /// <param name="id">The unique ID of the User.</param>
        /// <param name="options"> Parameters used to add the user to an organization.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A User and Session record.</returns>
        public async Task<User> AddUserToOrganziation(
            string id,
            AddUserToOrganizationOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/{id}/organizations",
            };

            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Remove an unmanaged User as a member of the given Organization.
        /// </summary>
        /// <param name="id">The unique ID of the User.</param>
        /// <param name="organizationId"> Unique identifier of the Organization.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A Task.</returns>
        public async Task RemoveUserFromOrganziation(
            string id,
            string organizationId,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/users/{id}/{organizationId}",
            };

            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }
    }
}
