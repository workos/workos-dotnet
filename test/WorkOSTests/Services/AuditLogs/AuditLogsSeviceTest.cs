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
        public async void TestCreateEvent()
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

            var success = await this.service.CreateEvent("org_123", this.auditLogEventPayload);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/audit_logs/events");
            Assert.True(success);
        }
    }
}
