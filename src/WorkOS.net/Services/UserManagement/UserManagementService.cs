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
        /// List Users.
        /// </summary>
        /// <param name="options"> Parameters to filter list of users.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A list of Users.</returns>
        public async Task<WorkOSList<User>> ListUsers(
            ListUsersOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = $"/users",
            };
            return await this.Client.MakeAPIRequest<WorkOSList<User>>(request, cancellationToken);
        }

        /// <summary>
        /// Authenticate user session with a password.
        /// </summary>
        /// <param name="options"> Parameters used to authenticate user with password.</param>
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
        /// Authenticate user session with a code.
        /// </summary>
        /// <param name="options"> Parameters used to authenticate user with a code.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A User and Session record.</returns>
        public async Task<(User, Session)> AuthenticateUserWithCode(
            AuthenticateUserWithCodeOptions options,
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
        /// Authenticate user session with Magic Auth.
        /// </summary>
        /// <param name="options"> Parameters used to authenticate user with Magic Auth.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A User and Session record.</returns>
        public async Task<(User, Session)> AuthenticateUserWithMagicAuth(
            AuthenticateUserWithMagicAuthOptions options,
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
        /// Create a Password Reset challenge.
        /// </summary>
        /// <param name="options"> Parameters used to create a password reset challenge.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> The token string and corresponding User object.</returns>
        public async Task<(string, User)> CreatePasswordResetChallenge(
            CreatePasswordResetChallengeOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/password_reset_challenge",
            };
            return await this.Client.MakeAPIRequest<(string, User)>(request, cancellationToken);
        }

        /// <summary>
        /// Complete a Password Reset.
        /// </summary>
        /// <param name="options"> Parameters used to complete a password reset.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> The corresponding User object.</returns>
        public async Task<User> CompletePasswordReset(
            CompletePasswordResetOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/password_reset",
            };
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }
    }
}
