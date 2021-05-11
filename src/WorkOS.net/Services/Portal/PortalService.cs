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
        /// <returns>A paginated list of Organizations.</returns>
        public WorkOSList<Organization> ListOrganizations(ListOrganizationsOptions options = null)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/organizations",
            };

            return this.Client.MakeAPIRequest<WorkOSList<Organization>>(request);
        }

        /// <summary>
        /// Asynchronously fetches a list of Organizations.
        /// </summary>
        /// <param name="options">Filter options when searching for Organizations.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a paginated list of Organizations.</returns>
        public async Task<WorkOSList<Organization>> ListOrganizationsAsync(
            ListOrganizationsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/organizations",
            };

            return await this.Client.MakeAPIRequestAsync<WorkOSList<Organization>>(request, cancellationToken);
        }

        /// <summary>
        /// Creates an Organization.
        /// </summary>
        /// <param name="options">Parameters to create an Organization.</param>
        /// <returns>The created Organization.</returns>
        public Organization CreateOrganization(CreateOrganizationOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/organizations",
            };

            return this.Client.MakeAPIRequest<Organization>(request);
        }

        /// <summary>
        /// Asynchronously creates an Organization.
        /// </summary>
        /// <param name="options">Parameters to create an Organization.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping the created Organization.</returns>
        public async Task<Organization> CreateOrganizationAsync(
            CreateOrganizationOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/organizations",
            };

            return await this.Client.MakeAPIRequestAsync<Organization>(request, cancellationToken);
        }

        /// <summary>
        /// Updates an Organization.
        /// </summary>
        /// <param name="options">Parameters to update an Organization.</param>
        /// <returns>The updated Organization.</returns>
        public Organization UpdateOrganization(UpdateOrganizationOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Put,
                Path = $"/organizations/{options.Organization}",
            };

            return this.Client.MakeAPIRequest<Organization>(request);
        }

        /// <summary>
        /// Asynchronously updates an Organization.
        /// </summary>
        /// <param name="options">Parameters to update an Organization.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping the updated Organization.</returns>
        public async Task<Organization> UpdateOrganizationAsync(
          UpdateOrganizationOptions options,
          CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Put,
                Path = $"/organizations/{options.Organization}",
            };

            return await this.Client.MakeAPIRequestAsync<Organization>(request, cancellationToken);
        }

        /// <summary>
        /// Generates a link to the Admin Portal.
        /// </summary>
        /// <param name="options">Parameters to create an Admin Portal link.</param>
        /// <returns>The Admin Portal URL.</returns>
        public string GenerateLink(GenerateLinkOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/portal/generate_link",
            };

            var response = this.Client.MakeAPIRequest<GenerateLinkResponse>(request);
            return response.Link;
        }

        /// <summary>
        /// Asynchronously generates a link to the Admin Portal.
        /// </summary>
        /// <param name="options">Parameters to create an Admin Portal link.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping the Admin Portal URL.</returns>
        public async Task<string> GenerateLinkAsync(
            GenerateLinkOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/portal/generate_link",
            };

            var response = await this.Client.MakeAPIRequestAsync<GenerateLinkResponse>(request, cancellationToken);
            return response.Link;
        }
    }
}
