namespace WorkOS
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS Portal.
    /// </summary>
    public class PortalService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PortalService"/> class.
        /// </summary>
        public PortalService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public PortalService(WorkOSClient client)
            : base(client)
        {
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

        /// <summary>
        /// Generates a link to the Admin Portal.
        /// </summary>
        /// <param name="options">Parameters to create an Admin Portal link.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>The Admin Portal URL.</returns>
        public async Task<string> GenerateLink(
            GenerateLinkOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/portal/generate_link",
            };

            var response = await this.Client.MakeAPIRequest<GenerateLinkResponse>(request, cancellationToken);
            return response.Link;
        }
    }
}
