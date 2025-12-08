namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS User Management.
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
        /// Gets a User.
        /// </summary>
        /// <param name="id">User unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A WorkOS User record.</returns>
        public async Task<User> GetUser(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/user_management/users/{id}",
            };

            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Gets a User by external ID.
        /// </summary>
        /// <param name="externalId">User external identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A WorkOS User record.</returns>
        public async Task<User> GetUserByExternalId(string externalId, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/user_management/users/external_id/{externalId}",
            };

            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Fetches a list of Users.
        /// </summary>
        /// <param name="options">Filter options when searching for Users.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A paginated list of Users.</returns>
        public async Task<WorkOSList<User>> ListUsers(
            ListUsersOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/user_management/users",
            };

            return await this.Client.MakeAPIRequest<WorkOSList<User>>(request, cancellationToken);
        }

        /// <summary>
        /// Creates a User.
        /// </summary>
        /// <param name="options">Parameters to create a User.</param>
        /// <param name="idempotencyKey">An optional idempotency key.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The created User.</returns>
        public async Task<User> CreateUser(
            CreateUserOptions options,
            string idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/user_management/users",
            };
            if (idempotencyKey != null)
            {
                request.WorkOSHeaders = new Dictionary<string, string>
                {
                    { "Idempotency-Key", idempotencyKey },
                };
            }

            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Updates a User.
        /// </summary>
        /// <param name="options">Parameters to update a User.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The updated User.</returns>
        public async Task<User> UpdateUser(
            UpdateUserOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Put,
                Path = $"/user_management/users/{options.Id}",
            };

            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Deletes a User.
        /// </summary>
        /// <param name="id">User unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task.</returns>
        public async Task DeleteUser(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/user_management/users/{id}",
            };

            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }

        /// <summary>
        /// Gets a password reset token.
        /// </summary>
        /// <param name="id">Password reset token unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A WorkOS PasswordReset record.</returns>
        public async Task<PasswordReset> GetPasswordReset(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/user_management/password_reset/{id}",
            };

            return await this.Client.MakeAPIRequest<PasswordReset>(request, cancellationToken);
        }

        /// <summary>
        /// Creates a password reset token for a user.
        /// </summary>
        /// <param name="options">Parameters to create a password reset token.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The created password reset token.</returns>
        public async Task<PasswordReset> CreatePasswordReset(
            CreatePasswordResetOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/user_management/password_reset",
            };

            return await this.Client.MakeAPIRequest<PasswordReset>(request, cancellationToken);
        }

        /// <summary>
        /// Resets a user's password.
        /// </summary>
        /// <param name="options">Parameters to reset the password.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The updated User after password reset.</returns>
        public async Task<User> ResetPassword(
            ResetPasswordOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/user_management/password_reset/confirm",
            };

            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Gets an organization membership.
        /// </summary>
        /// <param name="id">Organization membership unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A WorkOS OrganizationMembership record.</returns>
        public async Task<OrganizationMembership> GetOrganizationMembership(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/user_management/organization_memberships/{id}",
            };

            return await this.Client.MakeAPIRequest<OrganizationMembership>(request, cancellationToken);
        }

        /// <summary>
        /// Fetches a list of organization memberships.
        /// </summary>
        /// <param name="options">Filter options when searching for organization memberships.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A paginated list of organization memberships.</returns>
        public async Task<WorkOSList<OrganizationMembership>> ListOrganizationMemberships(
            ListOrganizationMembershipsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/user_management/organization_memberships",
            };

            return await this.Client.MakeAPIRequest<WorkOSList<OrganizationMembership>>(request, cancellationToken);
        }

        /// <summary>
        /// Creates an organization membership.
        /// </summary>
        /// <param name="options">Parameters to create an organization membership.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The created organization membership.</returns>
        public async Task<OrganizationMembership> CreateOrganizationMembership(
            CreateOrganizationMembershipOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/user_management/organization_memberships",
            };

            return await this.Client.MakeAPIRequest<OrganizationMembership>(request, cancellationToken);
        }

        /// <summary>
        /// Updates an organization membership.
        /// </summary>
        /// <param name="options">Parameters to update an organization membership.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The updated organization membership.</returns>
        public async Task<OrganizationMembership> UpdateOrganizationMembership(
            UpdateOrganizationMembershipOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Put,
                Path = $"/user_management/organization_memberships/{options.Id}",
            };

            return await this.Client.MakeAPIRequest<OrganizationMembership>(request, cancellationToken);
        }

        /// <summary>
        /// Deletes an organization membership.
        /// </summary>
        /// <param name="id">Organization membership unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task.</returns>
        public async Task DeleteOrganizationMembership(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/user_management/organization_memberships/{id}",
            };

            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }

        /// <summary>
        /// Deactivates an organization membership.
        /// </summary>
        /// <param name="id">Organization membership unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The deactivated organization membership.</returns>
        public async Task<OrganizationMembership> DeactivateOrganizationMembership(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Put,
                Path = $"/user_management/organization_memberships/{id}/deactivate",
            };

            return await this.Client.MakeAPIRequest<OrganizationMembership>(request, cancellationToken);
        }

        /// <summary>
        /// Reactivates an organization membership.
        /// </summary>
        /// <param name="id">Organization membership unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The reactivated organization membership.</returns>
        public async Task<OrganizationMembership> ReactivateOrganizationMembership(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Put,
                Path = $"/user_management/organization_memberships/{id}/reactivate",
            };

            return await this.Client.MakeAPIRequest<OrganizationMembership>(request, cancellationToken);
        }

        /// <summary>
        /// Gets a logout URL for a session.
        /// </summary>
        /// <param name="sessionId">The session identifier.</param>
        /// <param name="returnTo">The URL to return to after logout. Optional.</param>
        /// <returns>The logout URL where the user's browser should be redirected.</returns>
        public string GetLogoutUrl(string sessionId, string returnTo = null)
        {
            if (string.IsNullOrEmpty(sessionId))
            {
                throw new ArgumentNullException(nameof(sessionId));
            }

            var url = $"{this.Client.ApiBaseURL}/user_management/sessions/logout?session_id={System.Uri.EscapeDataString(sessionId)}";

            if (!string.IsNullOrEmpty(returnTo))
            {
                url += $"&return_to={System.Uri.EscapeDataString(returnTo)}";
            }

            return url;
        }

        /// <summary>
        /// Gets the JWKS (JSON Web Key Set) URL for validating session tokens.
        /// </summary>
        /// <param name="clientId">The client identifier for the WorkOS environment.</param>
        /// <returns>The JWKS URL for the organization.</returns>
        public string GetJwksUrl(string clientId)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentNullException(nameof(clientId));
            }

            return $"{this.Client.ApiBaseURL}/sso/jwks/{System.Uri.EscapeDataString(clientId)}";
        }

        /// <summary>
        /// Authenticates a user using an authorization code.
        /// </summary>
        /// <param name="options">Parameters to authenticate a user with an authorization code.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The authentication response containing the user and session tokens.</returns>
        public async Task<AuthenticationResponse> AuthenticateWithCode(
            AuthenticateWithCodeOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/user_management/authenticate",
            };

            return await this.Client.MakeAPIRequest<AuthenticationResponse>(request, cancellationToken);
        }
    }
}
