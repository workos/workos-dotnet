namespace WorkOS
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS Audit Logs API.
    /// </summary>
    public class AuditLogsService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogsService"/> class.
        /// </summary>
        public AuditLogsService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditLogsService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public AuditLogsService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Creates an Audit Trail event.
        /// </summary>
        /// <param name="organizationId">Organization the Event belongs to.</param>
        /// <param name="auditLogEvent">The Audit Log Event payload.</param>
        /// <param name="idempotencyKey">An optional idempotency key.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>True if successful.</returns>
        public async Task<bool> CreateEvent(
            string organizationId,
            AuditLogEvent auditLogEvent,
            string idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
            var options = new CreateAuditLogEventOptions
            {
                OrganizationId = organizationId,
                Event = auditLogEvent,
            };

            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/audit_logs/events",
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
    }
}
