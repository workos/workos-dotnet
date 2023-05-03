namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class AuditLogsSeviceTest
    {
        private readonly HttpMock httpMock;

        private readonly AuditLogsService service;

        private readonly AuditLogEvent auditLogEventPayload;

        public AuditLogsSeviceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new AuditLogsService(client);

            this.auditLogEventPayload = new AuditLogEvent
            {
                Action = "document.updated",
                OccurredAt = DateTime.Now,
                Version = 1,
                Actor = new AuditLogEventActor
                {
                    Id = "user_123",
                    Type = "user",
                    Name = "User",
                    Metadata = new Dictionary<string, string>
                    {
                        { "key", "value" },
                    },
                },
                Targets = new List<AuditLogEventTarget>()
                {
                    new AuditLogEventTarget
                    {
                        Id = "team_123",
                        Type = "team",
                        Name = "Team",
                        Metadata = new Dictionary<string, string>
                        {
                            { "key", "value" },
                        },
                    },
                },
                Context = new AuditLogEventContext
                {
                    Location = "0.0.0.0",
                    UserAgent = "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36",
                },
                Metadata = new Dictionary<string, string>
                {
                    { "key", "value" },
                },
            };
        }

        [Fact]
        public void TestCreateEvent()
        {
            var mockResponse = new Dictionary<string, bool>
            {
                { "success", true },
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/audit_logs/events",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(mockResponse));

            var options = new CreateAuditLogEventOptions()
            {
                OrganizationId = "org_123",
                Event = this.auditLogEventPayload,
            };

            this.service.CreateEvent(options);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/audit_logs/events");
        }

        [Fact]
        public async void TestCreateExport()
        {
            var mockResponse = new Dictionary<string, string>
            {
                { "object", "audit_log_export" },
                { "id", "audit_log_export_123" },
                { "state", "ready" },
                { "url", "https://audit-logs.com/download.csv" },
                { "created_at", "2022-08-18T18:07:10.822Z" },
                { "updated_at", "2022-08-18T18:07:10.822Z" },
            };

            this.httpMock.MockResponse(
                HttpMethod.Post,
                "/audit_logs/exports",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(mockResponse));

            var options = new CreateAuditLogExportOptions()
            {
                OrganizationId = "org_123",
                RangeStart = DateTime.Now,
                RangeEnd = DateTime.Now,
                Actions = new List<string>()
                { "user.signed_in" },
#pragma warning disable CS0618 // CreateAuditLogExportOptions.Actors' is obsolete: 'Please use ActorNames instead.
                Actors = new List<string>()
#pragma warning restore CS0618 // CreateAuditLogExportOptions.Actors' is obsolete: 'Please use ActorNames instead.
                { "Actor" },
                ActorNames = new List<string>()
                { "Actor" },
                ActorIds = new List<string>()
                { "user_foo" },
                Targets = new List<string>()
                { "user" },
            };

            var response = await this.service.CreateExport(options);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/audit_logs/exports");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }

        [Fact]
        public async void TestGetExport()
        {
            var mockResponse = new Dictionary<string, string>
            {
                { "object", "audit_log_export" },
                { "id", "audit_log_export_123" },
                { "state", "ready" },
                { "url", "https://audit-logs.com/download.csv" },
                { "created_at", "2022-08-18T18:07:10.822Z" },
                { "updated_at", "2022-08-18T18:07:10.822Z" },
            };

            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/audit_logs/exports/audit_log_export_123",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(mockResponse));

            var response = await this.service.GetExport("audit_log_export_123");
            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/audit_logs/exports/audit_log_export_123");
            Assert.Equal(
                JsonConvert.SerializeObject(mockResponse),
                JsonConvert.SerializeObject(response));
        }
    }
}
