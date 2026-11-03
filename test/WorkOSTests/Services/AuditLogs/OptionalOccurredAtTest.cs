// @oagen-ignore-file
namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;
    using WorkOS;
    using Xunit;

    public class OptionalOccurredAtTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("2026-11-03T12:34:56.789Z")]
        public async Task CreateEvent_OmitsAbsentTimestampAndPreservesExplicitValue(string timestamp)
        {
            var mock = new HttpMock();
            var client = new WorkOSClient(new WorkOSOptions
            {
                ApiKey = "sk_test",
                HttpClient = mock.HttpClient,
            });
            mock.MockResponse(HttpMethod.Post, "/audit_logs/events", HttpStatusCode.OK, "{\"success\":true}");
            var auditEvent = new AuditLogEvent
            {
                Action = "user.signed_in",
                Actor = new AuditLogEventActor { Id = "user_1", Type = "user" },
                Targets = new List<AuditLogEventActor>(),
                Context = new AuditLogEventContext { Location = "127.0.0.1" },
            };
            if (timestamp != null)
            {
                auditEvent.OccurredAt = DateTimeOffset.Parse(timestamp, CultureInfo.InvariantCulture);
            }

            await client.AuditLogs.CreateEventAsync(new AuditLogsCreateEventOptions
            {
                OrganizationId = "org_123",
                Event = auditEvent,
            });

            var body = JObject.Parse(await mock.GetLastRequestBodyAsync());
            var sentEvent = (JObject)body["event"];
            if (timestamp == null)
            {
                Assert.Null(sentEvent.Property("occurred_at"));
            }
            else
            {
                Assert.Equal(auditEvent.OccurredAt, sentEvent["occurred_at"].ToObject<DateTimeOffset>());
            }
        }
    }
}
