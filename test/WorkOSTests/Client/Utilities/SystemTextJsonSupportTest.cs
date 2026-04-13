// @oagen-ignore-file
namespace WorkOSTests
{
    using System.IO;
    using System.Text.Json;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Exercises <c>System.Text.Json</c> (STJ) against the generated entity
    /// surface. The SDK advertises STJ as a supported serializer, so these
    /// tests lock in the property-name mapping, enum handling, and nested
    /// object/list deserialization for a representative slice of DTOs.
    /// </summary>
    public class SystemTextJsonSupportTest
    {
        [Fact]
        public void Stj_Organization_DeserializesSnakeCasedProperties()
        {
            var json = File.ReadAllText("testdata/organization.json");
            var org = JsonSerializer.Deserialize<Organization>(json);

            Assert.NotNull(org);
            Assert.Equal("org_01EHWNCE74X7JSDV0X3SZ3KJNY", org!.Id);
            Assert.Equal("Acme Inc.", org.Name);
            Assert.Equal("2fe01467-f7ea-4dd2-8b79-c2b4f56d0191", org.ExternalId);
            Assert.Equal("cus_R9qWAGMQ6nGE7V", org.StripeCustomerId);
#pragma warning disable CS0618 // testing the deprecated wire field
            Assert.False(org.AllowProfilesOutsideOrganization);
#pragma warning restore CS0618
        }

        [Fact]
        public void Stj_Organization_DeserializesNestedListAndDictionary()
        {
            var json = File.ReadAllText("testdata/organization.json");
            var org = JsonSerializer.Deserialize<Organization>(json)!;

            Assert.NotNull(org.Domains);
            Assert.Single(org.Domains);
            Assert.Equal("foo-corp.com", org.Domains[0].Domain);

            Assert.NotNull(org.Metadata);
            Assert.Equal("diamond", org.Metadata!["tier"]);
        }

        [Fact]
        public void Stj_Organization_RoundTripPreservesSnakeCase()
        {
            var json = File.ReadAllText("testdata/organization.json");
            var org = JsonSerializer.Deserialize<Organization>(json)!;
            var reserialized = JsonSerializer.Serialize(org);

            Assert.Contains("\"external_id\"", reserialized);
            Assert.Contains("\"stripe_customer_id\"", reserialized);
            Assert.Contains("\"allow_profiles_outside_organization\"", reserialized);
            Assert.DoesNotContain("\"ExternalId\"", reserialized);
            Assert.DoesNotContain("\"StripeCustomerId\"", reserialized);
        }

        [Fact]
        public void Stj_Connection_DeserializesEnumViaEnumMember()
        {
            var json = File.ReadAllText("testdata/connection.json");
            var connection = JsonSerializer.Deserialize<Connection>(json)!;

            Assert.Equal(ConnectionState.Active, connection.State);
            var reserialized = JsonSerializer.Serialize(connection);
            Assert.Contains("\"state\":\"active\"", reserialized);
        }

        [Fact]
        public void Stj_Connection_UnknownEnumValueFallsBackToUnknown()
        {
            var json = File.ReadAllText("testdata/connection.json");
            var rewritten = json.Replace(
                "\"state\": \"active\"",
                "\"state\": \"some_new_state_from_server\"");
            var connection = JsonSerializer.Deserialize<Connection>(rewritten)!;
            Assert.Equal(ConnectionState.Unknown, connection.State);
        }

        [Fact]
        public void Stj_WorkOSList_DeserializesPaginatedShape()
        {
            var json =
                "{\"data\":[{\"object\":\"organization\",\"id\":\"org_1\",\"name\":\"Acme\"," +
                "\"domains\":[],\"created_at\":\"2026-01-01T00:00:00.000Z\"," +
                "\"updated_at\":\"2026-01-01T00:00:00.000Z\"," +
                "\"allow_profiles_outside_organization\":false}]," +
                "\"list_metadata\":{\"before\":null,\"after\":\"cursor_xyz\"}}";

            var page = JsonSerializer.Deserialize<WorkOSList<Organization>>(json)!;
            Assert.NotNull(page.Data);
            Assert.Single(page.Data!);
            Assert.Equal("org_1", page.Data![0].Id);
            Assert.NotNull(page.ListMetadata);
            Assert.Equal("cursor_xyz", page.ListMetadata!.After);
        }

        [Fact]
        public void Stj_ListOptions_RoundTripPaginationOrder()
        {
            var opts = new TestListOptions { Limit = 25, Order = PaginationOrder.Asc };
            var json = JsonSerializer.Serialize(opts);
            Assert.Contains("\"limit\":25", json);
            Assert.Contains("\"order\":\"asc\"", json);

            var parsed = JsonSerializer.Deserialize<TestListOptions>(json)!;
            Assert.Equal(25, parsed.Limit);
            Assert.Equal(PaginationOrder.Asc, parsed.Order);
        }

        [Fact]
        public void Stj_Webhook_DeserializesEnvelope()
        {
            var payload = "{\"id\":\"wh_01\",\"event\":\"dsync.user.created\"," +
                "\"data\":{\"first_name\":\"Lela\"}," +
                "\"created_at\":\"2026-01-15T12:00:00.000Z\"}";

            var webhook = JsonSerializer.Deserialize<Webhook>(payload)!;
            Assert.Equal("wh_01", webhook.Id);
            Assert.Equal("dsync.user.created", webhook.Event);
            Assert.Equal("2026-01-15T12:00:00.000Z", webhook.CreatedAt);
            Assert.NotNull(webhook.Data);
        }

#pragma warning disable SA1402 // companion fixture type kept beside its tests
        public class TestListOptions : ListOptions
        {
        }
#pragma warning restore SA1402
    }
}
