namespace WorkOS
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS Organizations.
    /// </summary>
    public class OrganizationsService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationsService"/> class.
        /// </summary>
        public OrganizationsService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrganizationsService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public OrganizationsService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Gets an Organization.
        /// </summary>
        /// <param name="id">Organization unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A WorkOS Organization record.</returns>
        public async Task<Organization> GetOrganization(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/organizations/{id}",
            };

            return await this.Client.MakeAPIRequest<Organization>(request, cancellationToken);
        }

        /// <summary>
        /// Fetches a list of Organizations.
        /// </summary>
        /// <param name="options">Filter options when searching for Organizations.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A paginated list of Organizations.</returns>
        public async Task<WorkOSList<Organization>> ListOrganizations(
            ListOrganizationsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/organizations",
            };

            return await this.Client.MakeAPIRequest<WorkOSList<Organization>>(request, cancellationToken);
        }

        /// <summary>
        /// Creates an Organization.
        /// </summary>
        /// <param name="options">Parameters to create an Organization.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The created Organization.</returns>
        public async Task<Organization> CreateOrganization(
            CreateOrganizationOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/organizations",
            };

            return await this.Client.MakeAPIRequest<Organization>(request, cancellationToken);
        }

        /// <summary>
        /// Deletes an Organization.
        /// </summary>
        /// <param name="id">Organization unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A deleted WorkOS Organization record.</returns>
        public async Task<Organization> DeleteOrganization(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/organizations/{id}",
            };

            return await this.Client.MakeAPIRequest<Organization>(request, cancellationToken);
        }

        /// <summary>
        /// Updates an Organization.
        /// </summary>
        /// <param name="options">Parameters to update an Organization.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The updated Organization.</returns>
        public async Task<Organization> UpdateOrganization(
          UpdateOrganizationOptions options,
          CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Put,
                Path = $"/organizations/{options.Organization}",
            };

            return await this.Client.MakeAPIRequest<Organization>(request, cancellationToken);
        }
    }
}
