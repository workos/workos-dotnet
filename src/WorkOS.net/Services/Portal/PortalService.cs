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
