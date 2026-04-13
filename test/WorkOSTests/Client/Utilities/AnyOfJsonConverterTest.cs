// @oagen-ignore-file
namespace WorkOSTests
{
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using WorkOS;
    using Xunit;
    using STJ = System.Text.Json;

#pragma warning disable SA1201 // nested test fixture types placed beside the tests that use them
    public class AnyOfJsonConverterTest
    {
        public class MetadataEnvelope
        {
            [JsonProperty("metadata")]
            [STJ.Serialization.JsonPropertyName("metadata")]
            public Dictionary<string, AnyOf<string, double, bool>> Metadata { get; set; } = default!;
        }

        [Fact]
        public void Newtonsoft_SerializesInnerValueDirectly_String()
        {
            AnyOf<string, double, bool> v = "hello";
            var json = JsonConvert.SerializeObject(v);
            Assert.Equal("\"hello\"", json);
        }

        [Fact]
        public void Newtonsoft_SerializesInnerValueDirectly_Double()
        {
            AnyOf<string, double, bool> v = 3.14;
            var json = JsonConvert.SerializeObject(v);
            Assert.Equal("3.14", json);
        }

        [Fact]
        public void Newtonsoft_SerializesInnerValueDirectly_Bool()
        {
            AnyOf<string, double, bool> v = true;
            var json = JsonConvert.SerializeObject(v);
            Assert.Equal("true", json);
        }

        [Fact]
        public void Newtonsoft_DoesNotWrapInValueProperty()
        {
            AnyOf<string, double, bool> v = "x";
            var json = JsonConvert.SerializeObject(v);
            Assert.DoesNotContain("\"Value\"", json);
        }

        [Fact]
        public void Newtonsoft_AuditLogMetadataShapeSerializesCorrectly()
        {
            var env = new MetadataEnvelope
            {
                Metadata = new Dictionary<string, AnyOf<string, double, bool>>
                {
                    { "actor_type", "user" },
                    { "actor_id", "user_123" },
                    { "score", 0.95 },
                    { "reviewed", true },
                },
            };

            var json = JsonConvert.SerializeObject(env);
            var parsed = JToken.Parse(json);
            Assert.Equal(JTokenType.String, parsed.SelectToken("metadata.actor_type").Type);
            Assert.Equal("user", parsed.SelectToken("metadata.actor_type").Value<string>());
            Assert.Equal(JTokenType.Float, parsed.SelectToken("metadata.score").Type);
            Assert.Equal(0.95, parsed.SelectToken("metadata.score").Value<double>());
            Assert.Equal(JTokenType.Boolean, parsed.SelectToken("metadata.reviewed").Type);
            Assert.True(parsed.SelectToken("metadata.reviewed").Value<bool>());
        }

        [Fact]
        public void Newtonsoft_DeserializesEachTypeArgumentCorrectly()
        {
            var json = "{\"metadata\":{\"a\":\"x\",\"b\":1.5,\"c\":false}}";
            var env = JsonConvert.DeserializeObject<MetadataEnvelope>(json)!;
            Assert.NotNull(env.Metadata);
            Assert.Equal("x", env.Metadata["a"].Value);
            Assert.Equal(1.5, env.Metadata["b"].Value);
            Assert.Equal(false, env.Metadata["c"].Value);
        }

        [Fact]
        public void Stj_SerializesInnerValueDirectly()
        {
            AnyOf<string, double, bool> v = "hello";
            var json = STJ.JsonSerializer.Serialize(v);
            Assert.Equal("\"hello\"", json);
        }

        [Fact]
        public void Stj_DoesNotWrapInValueProperty()
        {
            AnyOf<string, double, bool> v = 42.0;
            var json = STJ.JsonSerializer.Serialize(v);
            Assert.DoesNotContain("\"Value\"", json);
        }

        [Fact]
        public void Stj_AuditLogMetadataShapeSerializesCorrectly()
        {
            var env = new MetadataEnvelope
            {
                Metadata = new Dictionary<string, AnyOf<string, double, bool>>
                {
                    { "actor_type", "user" },
                    { "score", 0.5 },
                    { "reviewed", true },
                },
            };

            var json = STJ.JsonSerializer.Serialize(env);
            Assert.Contains("\"actor_type\":\"user\"", json);
            Assert.Contains("\"score\":0.5", json);
            Assert.Contains("\"reviewed\":true", json);
            Assert.DoesNotContain("\"Value\"", json);
        }
    }
#pragma warning restore SA1201
}
