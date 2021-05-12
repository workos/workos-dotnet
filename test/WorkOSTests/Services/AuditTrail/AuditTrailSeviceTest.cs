namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class AuditTrailSeviceTest
    {
        private readonly HttpMock httpMock;

        private readonly AuditTrailService service;

        private readonly CreateEventOptions createEventOptions;

        private readonly ListEventsOptions listEventsOptions;

        private readonly WorkOSList<Event> mockListEventsResponse;

        public AuditTrailSeviceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new AuditTrailService(client);

            this.createEventOptions = new CreateEventOptions
            {
                Action = "document.updated",
                ActionType = "u",
                ActorName = "foo@corp.com",
                ActorId = "user_0",
                Group = "corp.com",
                Location = "192.0.0.8",
                Metadata = new Dictionary<string, string>
                {
                    { "key", "value" },
                },
                OccurredAt = DateTime.Now,
                TargetId = "document_39127",
                TargetName = "Security Audit 2018",
            };
            this.listEventsOptions = new ListEventsOptions
            {
                ActorName = new List<string> { "foo@corp.com" },
                OccurredAtGt = DateTime.Now,
            };
            this.mockListEventsResponse = new WorkOSList<Event>
            {
                Data = new List<Event>
                {
                    new Event
                    {
                        Action = new EventAction
                        {
                            Id = "event_action_id",
                            Name = "user.updated_directory",
                            EnvironmentId = "environment_id",
                        },
                        ActorId = "user_id",
                        ActorName = "demo@foo-corp.com",
                        Group = "workos.com",
                        Id = "event_id",
                        Location = "::1",
                        Metadata = new Dictionary<string, string>
                        {
                            { "key", "value" },
                        },
                        OccurredAt = DateTime.Now,
                        TargetId = "target_id",
                        TargetName = "target_name",
                    },
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
                "/events",
                HttpStatusCode.Created,
                RequestUtilities.ToJsonString(mockResponse));

            var success = await this.service.CreateEvent(this.createEventOptions);
            this.httpMock.AssertRequestWasMade(HttpMethod.Post, "/events");
            Assert.True(success);
        }

        [Fact]
        public async void TestListEvents()
        {
            this.httpMock.MockResponse(
                HttpMethod.Get,
                "/events",
                HttpStatusCode.OK,
                RequestUtilities.ToJsonString(this.mockListEventsResponse));

            var response = await this.service.ListEvents(this.listEventsOptions);

            this.httpMock.AssertRequestWasMade(HttpMethod.Get, "/events");
            Assert.Equal(
                JsonConvert.SerializeObject(this.mockListEventsResponse),
                JsonConvert.SerializeObject(response));
        }
    }
}
