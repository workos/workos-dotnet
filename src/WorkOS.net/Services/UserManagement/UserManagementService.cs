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
<<<<<<< HEAD
        /// <returns> A User and Session record.</returns>
        public async Task<User> AuthenticateUserWithPassword(
=======
        /// <returns> A AuthenticateUserWithPasswordResponse record.</returns>
        public async Task<AuthenticateUserResponse> AuthenticateUserWithPassword(
>>>>>>> main
            AuthenticateUserWithPasswordOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/authenticate",
            };
<<<<<<< HEAD
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
=======
            return await this.Client.MakeAPIRequest<AuthenticateUserResponse>(request, cancellationToken);
>>>>>>> main
        }

        /// <summary>
        /// Authenticate user session with a code.
        /// </summary>
        /// <param name="options"> Parameters used to authenticate user with a code.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
<<<<<<< HEAD
        /// <returns> A User and Session record.</returns>
        public async Task<User> AuthenticateUserWithCode(
=======
        /// <returns> A AuthenticateUserWithCodeResponse record.</returns>
        public async Task<AuthenticateUserResponse> AuthenticateUserWithCode(
>>>>>>> main
            AuthenticateUserWithCodeOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/authenticate",
            };
<<<<<<< HEAD
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
=======
            return await this.Client.MakeAPIRequest<AuthenticateUserResponse>(request, cancellationToken);
>>>>>>> main
        }

        /// <summary>
        /// Authenticate user session with Magic Auth.
        /// </summary>
        /// <param name="options"> Parameters used to authenticate user with Magic Auth.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
<<<<<<< HEAD
        /// <returns> A User and Session record.</returns>
        public async Task<User> AuthenticateUserWithMagicAuth(
=======
        /// <returns> A AuthenticateUserWithMagicAuthResponse record.</returns>
        public async Task<AuthenticateUserResponse> AuthenticateUserWithMagicAuth(
>>>>>>> main
            AuthenticateUserWithMagicAuthOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/authenticate",
            };
<<<<<<< HEAD
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
=======
            return await this.Client.MakeAPIRequest<AuthenticateUserResponse>(request, cancellationToken);
        }

        /// <summary>
        /// Creates a one-time Magic Auth code and emails it to the user.
        /// </summary>
        /// <param name="options"> Parameters used to send magic auth code.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A Magic Auth Challenge.</returns>
        public async Task<SendMagicAuthCodeResponse> SendMagicAuthCode(
            SendMagicAuthCodeOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/magic_auth/send",
            };
            return await this.Client.MakeAPIRequest<SendMagicAuthCodeResponse>(request, cancellationToken);
>>>>>>> main
        }

        /// <summary>
        /// Creates an email verification challenge and emails verification token to user.
        /// </summary>
        /// <param name="id">The unique ID of the User.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A token and the corresponding User object.</returns>
        public async Task<SendVerificationEmailResponse> SendVerificationEmail(
            string id,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = $"/users/{id}/send_verification_email",
            };
            return await this.Client.MakeAPIRequest<SendVerificationEmailResponse>(request, cancellationToken);
        }

        /// <summary>
        /// Verifies email challenge.
        /// </summary>
        /// <param name="options"> Parameters used to complete email verification.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> The corresponding User object.</returns>
        public async Task<User> VerifyEmail(
            VerifyEmailOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = $"/users/{options.UserId}/verify_email",
            };
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Adds an User as a member of the given Organization.
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

        /// <summary>
        /// Update the user's information.
        /// </summary>
        /// <param name="options"> Parameters used to update a user.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> The corresponding User object.</returns>
        public async Task<User> UpdateUser(
            UpdateUserOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Put,
                Path = $"/users/{options.Id}",
            };
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Update the user's information.
        /// </summary>
        /// <param name="options"> Parameters used to update a user.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> The corresponding User object.</returns>
        public async Task<User> UpdateUserPassword(
            UpdateUserPasswordOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Put,
                Path = $"/users/{options.Id}/password",
            };
            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Delete a User.
        /// </summary>
        /// <param name="id">The unique ID of the User.</param>
        /// <param name="cancellationToken">An optional token to cancel the request.</param>
        /// <returns> A Task.</returns>
        public async Task DeleteUser(
            string id,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/users/{id}",
            };
            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }
    }
}
