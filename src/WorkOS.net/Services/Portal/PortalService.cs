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
    }
}
