// @oagen-ignore-file
namespace WorkOSTests
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;
    using STJ = System.Text.Json;
    using STJS = System.Text.Json.Serialization;

#pragma warning disable SA1201 // nested test fixture types placed beside the tests that use them
    public class EnumConverterTest
    {
        [JsonConverter(typeof(WorkOSNewtonsoftStringEnumConverter))]
        [STJS.JsonConverter(typeof(WorkOSStringEnumConverterFactory))]
        public enum SampleEnum
        {
            [EnumMember(Value = "unknown")]
            Unknown,

            [EnumMember(Value = "first_value")]
            FirstValue,
            [EnumMember(Value = "second_value")]
            SecondValue,
        }

        public class Envelope
        {
            [JsonProperty("value")]
            [STJS.JsonPropertyName("value")]
            public SampleEnum Value { get; set; }
        }

        [Fact]
        public void Newtonsoft_RoundTripsKnownEnumMemberValue()
        {
            var json = "{\"value\":\"first_value\"}";
            var parsed = JsonConvert.DeserializeObject<Envelope>(json)!;
            Assert.Equal(SampleEnum.FirstValue, parsed.Value);

            var reserialized = JsonConvert.SerializeObject(parsed);
            Assert.Contains("\"first_value\"", reserialized);
        }

        [Fact]
        public void Newtonsoft_ForwardCompatUnknownValueDoesNotThrow()
        {
            var json = "{\"value\":\"some_future_value_not_yet_in_sdk\"}";
            var parsed = JsonConvert.DeserializeObject<Envelope>(json)!;
            Assert.Equal(SampleEnum.Unknown, parsed.Value);
        }

        [Fact]
        public void Newtonsoft_WritesUnknownUsingEnumMemberValue()
        {
            var env = new Envelope { Value = SampleEnum.Unknown };
            var json = JsonConvert.SerializeObject(env);
            Assert.Contains("\"unknown\"", json);
        }

        [Fact]
        public void Stj_RoundTripsKnownEnumMemberValue()
        {
            var json = "{\"value\":\"second_value\"}";
            var parsed = STJ.JsonSerializer.Deserialize<Envelope>(json)!;
            Assert.Equal(SampleEnum.SecondValue, parsed.Value);

            var reserialized = STJ.JsonSerializer.Serialize(parsed);
            Assert.Contains("\"second_value\"", reserialized);
        }

        [Fact]
        public void Stj_ForwardCompatUnknownValueDoesNotThrow()
        {
            var json = "{\"value\":\"some_future_value_not_yet_in_sdk\"}";
            var parsed = STJ.JsonSerializer.Deserialize<Envelope>(json)!;
            Assert.Equal(SampleEnum.Unknown, parsed.Value);
        }
    }
#pragma warning restore SA1201
}
