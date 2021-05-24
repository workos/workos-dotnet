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
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A paginated list of Directories.</returns>
        public async Task<WorkOSList<Directory>> ListDirectories(
            ListDirectoriesOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directories",
            };

            return await this.Client.MakeAPIRequest<WorkOSList<Directory>>(request, cancellationToken);
        }

        /// <summary>
        /// Fetches a list of Directory Users.
        /// </summary>
        /// <param name="options">Filter options when searching for Directory Users.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A paginated list of Directory Users.</returns>
        public async Task<WorkOSList<User>> ListUsers(
            ListUsersOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directory_users",
            };

            return await this.Client.MakeAPIRequest<WorkOSList<User>>(request, cancellationToken);
        }

        /// <summary>
        /// Gets a provisioned User for a Directory.
        /// </summary>
        /// <param name="id">Directory User unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A WorkOS Directory User record.</returns>
        public async Task<User> GetUser(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/directory_users/{id}",
            };

            return await this.Client.MakeAPIRequest<User>(request, cancellationToken);
        }

        /// <summary>
        /// Deletes a Directory.
        /// </summary>
        /// <param name="id">Directory unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task.</returns>
        public async Task DeleteDirectory(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/directories/{id}",
            };

            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }

        /// <summary>
        /// Fetches a list of Directory Groups.
        /// </summary>
        /// <param name="options">Filter options when searching for Directory Groups.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A paginated list of Directory Groups.</returns>
        public async Task<WorkOSList<Group>> ListGroups(
            ListGroupsOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/directory_groups",
            };

            return await this.Client.MakeAPIRequest<WorkOSList<Group>>(request, cancellationToken);
        }

        /// <summary>
        /// Gets a provisioned Group for a Directory.
        /// </summary>
        /// <param name="id">Directory Group unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A WorkOS Directory Group record.</returns>
        public async Task<Group> GetGroup(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/directory_groups/{id}",
            };

            return await this.Client.MakeAPIRequest<Group>(request, cancellationToken);
        }
    }
}
