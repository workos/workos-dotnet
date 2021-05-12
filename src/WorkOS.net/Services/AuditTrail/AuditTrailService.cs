namespace WorkOS
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS Audit Trail.
    /// </summary>
    public class AuditTrailService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditTrailService"/> class.
        /// </summary>
        public AuditTrailService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditTrailService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public AuditTrailService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Creates an Audit Trail event.
        /// </summary>
        /// <param name="options">Options representing an Event.</param>
        /// <param name="idempotencyKey">An optional idempotency key.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>True if successful.</returns>
        public async Task<bool> CreateEvent(
            CreateEventOptions options,
            string idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/events",
            };
            if (idempotencyKey != null)
            {
                request.WorkOSHeaders = new Dictionary<string, string>
                {
                    { "Idempotency-Key", idempotencyKey },
                };
            }

            await this.Client.MakeAPIRequest<object>(request, cancellationToken);
            return true;
        }

        /// <summary>
        /// Fetches a list of Audit Trail Events.
        /// </summary>
        /// <param name="options">Filter options when searching for events.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A paginated list of Audit Trail Events.</returns>
        public async Task<WorkOSList<Event>> ListEvents(
            ListEventsOptions options = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Get,
                Path = "/events",
            };

            return await this.Client.MakeAPIRequest<WorkOSList<Event>>(request, cancellationToken);
        }
    }
}
