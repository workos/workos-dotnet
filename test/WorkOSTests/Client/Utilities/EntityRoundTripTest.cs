// @oagen-ignore-file
namespace WorkOSTests
{
    using System.IO;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using WorkOS;
    using Xunit;
    using STJ = System.Text.Json;

    /// <summary>
    /// Round-trip tests for representative generated entities: load fixture →
    /// deserialize into the C# entity → re-serialize → compare structurally to
    /// the source. Catches drift in property naming, enum mapping, date
    /// handling, and nested-object shape under both Newtonsoft and STJ.
    ///
    /// Structural comparison ignores key order and whitespace but is strict
    /// about value equality and shape.
    /// </summary>
    public class EntityRoundTripTest
    {
        [Fact]
        public void Newtonsoft_Organization_RoundTrips()
        {
            AssertNewtonsoftRoundTrip<Organization>("testdata/organization.json");
        }

        [Fact]
        public void Newtonsoft_Connection_RoundTrips()
        {
            AssertNewtonsoftRoundTrip<Connection>("testdata/connection.json");
        }

        [Fact]
        public void Stj_Organization_RoundTrips()
        {
            AssertStjRoundTrip<Organization>("testdata/organization.json");
        }

        [Fact]
        public void Stj_Connection_RoundTrips()
        {
            AssertStjRoundTrip<Connection>("testdata/connection.json");
        }

        private static void AssertNewtonsoftRoundTrip<T>(string fixturePath)
        {
            var raw = File.ReadAllText(fixturePath);
            var entity = JsonConvert.DeserializeObject<T>(raw);
            Assert.NotNull(entity);
            var reserialized = JsonConvert.SerializeObject(entity);
            AssertStructurallyEqual(raw, reserialized, $"{typeof(T).Name} (Newtonsoft)");
        }

        private static void AssertStjRoundTrip<T>(string fixturePath)
        {
            var raw = File.ReadAllText(fixturePath);
            var entity = STJ.JsonSerializer.Deserialize<T>(raw);
            Assert.NotNull(entity);
            var reserialized = STJ.JsonSerializer.Serialize(entity);
            AssertStructurallyEqual(raw, reserialized, $"{typeof(T).Name} (STJ)");
        }

        /// <summary>
        /// Assert that <paramref name="actual"/> contains every key+value pair
        /// present in <paramref name="expected"/>. The reserialized output may
        /// add explicit nulls for fields the SDK models even when omitted from
        /// the fixture; that's fine. We're catching missing or mistyped data.
        /// </summary>
        private static void AssertStructurallyEqual(string expected, string actual, string label)
        {
            // DateParseHandling.None keeps date-shaped strings as strings, so a
            // value like "2026-01-15T12:00:00.000Z" stays string-equal across
            // the round-trip instead of being parsed into a DateTime that loses
            // timezone-display fidelity.
            var expectedJson = ParseRaw(expected);
            var actualJson = ParseRaw(actual);
            AssertSubset(expectedJson, actualJson, label, "$");
        }

        private static JToken ParseRaw(string json)
        {
            using var reader = new JsonTextReader(new StringReader(json))
            {
                DateParseHandling = DateParseHandling.None,
            };
            return JToken.ReadFrom(reader);
        }

        private static bool TryParseDate(JToken token, out System.DateTimeOffset value)
        {
            value = default;
            if (token.Type != JTokenType.String)
            {
                return false;
            }

            var s = token.Value<string>();
            return s != null
                && System.DateTimeOffset.TryParse(
                    s,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal,
                    out value);
        }

        private static void AssertSubset(JToken expected, JToken actual, string label, string path)
        {
            switch (expected.Type)
            {
                case JTokenType.Object:
                    Assert.True(
                        actual.Type == JTokenType.Object,
                        $"{label}: at {path} expected object, got {actual.Type}");
                    foreach (var prop in (JObject)expected)
                    {
                        var actualProp = ((JObject)actual)[prop.Key];
                        Assert.True(
                            actualProp != null,
                            $"{label}: at {path}.{prop.Key} expected to be present, was missing");
                        AssertSubset(prop.Value!, actualProp, label, $"{path}.{prop.Key}");
                    }

                    break;
                case JTokenType.Array:
                    Assert.True(
                        actual.Type == JTokenType.Array,
                        $"{label}: at {path} expected array, got {actual.Type}");
                    var expectedArr = (JArray)expected;
                    var actualArr = (JArray)actual;
                    Assert.True(
                        expectedArr.Count == actualArr.Count,
                        $"{label}: at {path} expected {expectedArr.Count} items, got {actualArr.Count}");
                    for (int i = 0; i < expectedArr.Count; i++)
                    {
                        AssertSubset(expectedArr[i], actualArr[i], label, $"{path}[{i}]");
                    }

                    break;
                default:
                    if (JToken.DeepEquals(expected, actual))
                    {
                        break;
                    }

                    // Date strings normalize across serializers (e.g. "...000Z"
                    // becomes "...+00:00"). Compare as instants instead.
                    if (TryParseDate(expected, out var expectedInstant)
                        && TryParseDate(actual, out var actualInstant)
                        && expectedInstant == actualInstant)
                    {
                        break;
                    }

                    Assert.Fail($"{label}: at {path} expected {expected} but got {actual}");
                    break;
            }
        }
    }
}
