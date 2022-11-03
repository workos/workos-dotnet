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
        /// Creates an Audit Log Event.
        /// </summary>
        /// <param name="options">Audit log event options.</param>
        /// <param name="idempotencyKey">An optional idempotency key.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        public async void CreateEvent(
            CreateAuditLogEventOptions options,
            string idempotencyKey = null,
            CancellationToken cancellationToken = default)
        {
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
        }

        /// <summary>
        /// Creates an Audit Log Export.
        /// </summary>
        /// <param name="options">Date range and query options for creating an export.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>AuditLogExport entity.</returns>
        public Task<AuditLogExport> CreateExport(
            CreateAuditLogExportOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/audit_logs/exports",
            };

            return this.Client.MakeAPIRequest<AuditLogExport>(request, cancellationToken);
        }

        /// <summary>
        /// Retrieves an Audit Log Export.
        /// </summary>
        /// <param name="auditLogExportId">Unique identifier of the Audit Log Export.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>AuditLogExport entity.</returns>
        public Task<AuditLogExport> GetExport(
            string auditLogExportId,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/audit_logs/exports/{auditLogExportId}",
            };

            return this.Client.MakeAPIRequest<AuditLogExport>(request, cancellationToken);
        }
    }
}
