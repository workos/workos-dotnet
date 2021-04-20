namespace WorkOS
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS Directory Sync.
    /// </summary>
    public class DirectorySyncService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DirectorySyncService"/> class.
        /// </summary>
        public DirectorySyncService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DirectorySyncService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public DirectorySyncService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Fetches a list of Directories.
        /// </summary>
        /// <param name="options">Filter options when searching for Directories.</param>
        /// <returns>A paginated list of Directories.</returns>
        public WorkOSList<Directory> ListDirectories(ListDirectoriesOptions options = null)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directories",
            };

            return this.Client.MakeAPIRequest<WorkOSList<Directory>>(request);
        }

        /// <summary>
        /// Asynchronously fetches a list of Directories.
        /// </summary>
        /// <param name="options">Filter options when searching for Directories.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a paginated list of Directories.</returns>
        public async Task<WorkOSList<Directory>> ListDirectoriesAsync(
            ListDirectoriesOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directories",
            };

            return await this.Client.MakeAPIRequestAsync<WorkOSList<Directory>>(request, cancellationToken);
        }

        /// <summary>
        /// Fetches a list of Directory Users.
        /// </summary>
        /// <param name="options">Filter options when searching for Directory Users.</param>
        /// <returns>A paginated list of Directory Users.</returns>
        public WorkOSList<User> ListUsers(ListUsersOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directory_users",
            };

            return this.Client.MakeAPIRequest<WorkOSList<User>>(request);
        }

        /// <summary>
        /// Asynchronously fetches a list of Directory Users.
        /// </summary>
        /// <param name="options">Filter options when searching for Directory Users.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a paginated list of Directory Users.</returns>
        public async Task<WorkOSList<User>> ListUsersAsync(
            ListUsersOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directory_users",
            };

            return await this.Client.MakeAPIRequestAsync<WorkOSList<User>>(request, cancellationToken);
        }

        /// <summary>
        /// Gets a provisioned User for a Directory.
        /// </summary>
        /// <param name="id">Directory User unique identifier.</param>
        /// <returns>A WorkOS Directory User record.</returns>
        public User GetUser(string id)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/directory_users/{id}",
            };

            return this.Client.MakeAPIRequest<User>(request);
        }

        /// <summary>
        /// Asynchronously gets a provisioned User for a Directory.
        /// </summary>
        /// <param name="id">Directory User unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a WorkOS Directory User record.</returns>
        public async Task<User> GetUserAsync(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/directory_users/{id}",
            };

            return await this.Client.MakeAPIRequestAsync<User>(request, cancellationToken);
        }

        /// <summary>
        /// Asynchronously deletes a provisioned User for a Directory.
        /// </summary>
        /// <param name="id">Directory User unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a WorkOS Directory User record.</returns>
        public async Task<User> DeleteUser(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/directory_users/{id}",
            };

            return await this.Client.MakeAPIRequestAsync<User>(request, cancellationToken);
        }

        /// <summary>
        /// Fetches a list of Directory Groups.
        /// </summary>
        /// <param name="options">Filter options when searching for Directory Groups.</param>
        /// <returns>A paginated list of Directory Groups.</returns>
        public WorkOSList<Group> ListGroups(ListGroupsOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directory_groups",
            };

            return this.Client.MakeAPIRequest<WorkOSList<Group>>(request);
        }

        /// <summary>
        /// Asynchronously fetches a list of Directory Groups.
        /// </summary>
        /// <param name="options">Filter options when searching for Directory Groups.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a paginated list of Directory Groups.</returns>
        public async Task<WorkOSList<Group>> ListGroupsAsync(
            ListGroupsOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directory_groups",
            };

            return await this.Client.MakeAPIRequestAsync<WorkOSList<Group>>(request, cancellationToken);
        }

        /// <summary>
        /// Gets a provisioned Group for a Directory.
        /// </summary>
        /// <param name="id">Directory Group unique identifier.</param>
        /// <returns>A WorkOS Directory Group record.</returns>
        public Group GetGroup(string id)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/directory_groups/{id}",
            };

            return this.Client.MakeAPIRequest<Group>(request);
        }

        /// <summary>
        /// Asynchronously gets a provisioned Group for a Directory.
        /// </summary>
        /// <param name="id">Directory Group unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a WorkOS Directory Group record.</returns>
        public async Task<Group> GetGroupAsync(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/directory_groups/{id}",
            };

            return await this.Client.MakeAPIRequestAsync<Group>(request, cancellationToken);
        }
    }
}
