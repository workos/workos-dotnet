// @oagen-ignore-file
namespace WorkOSTests
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

#pragma warning disable SA1201 // nested fixture type placed beside the tests that use it
    public class QuerySerializationTest
    {
        private class NumericOptions : BaseOptions
        {
            [JsonProperty("flag")]
            public bool Flag { get; set; }

            [JsonProperty("score")]
            public double Score { get; set; }

            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("count_short")]
            public short CountShort { get; set; }

            [JsonProperty("count_long")]
            public long CountLong { get; set; }

            [JsonProperty("occurred_at")]
            [JsonConverter(typeof(Newtonsoft.Json.Converters.IsoDateTimeConverter))]
            public DateTimeOffset OccurredAt { get; set; }
        }

        [Fact]
        public void CreateQueryString_SerializesBooleans()
        {
            var q = RequestUtilities.CreateQueryString(new NumericOptions
            {
                Flag = true,
                Score = 0,
                Amount = 0,
                CountShort = 0,
                CountLong = 0,
                OccurredAt = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
            });
            Assert.Contains("flag=true", q);
        }

        [Fact]
        public void CreateQueryString_SerializesNumerics()
        {
            var q = RequestUtilities.CreateQueryString(new NumericOptions
            {
                Flag = false,
                Score = 3.14,
                Amount = 12.34m,
                CountShort = 7,
                CountLong = 9000000000L,
                OccurredAt = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
            });
            Assert.Contains("score=3.14", q);
            Assert.Contains("amount=12.34", q);
            Assert.Contains("count_short=7", q);
            Assert.Contains("count_long=9000000000", q);
        }

        [Fact]
        public async Task CreateHttpContent_FormEncodedDoesNotThrowOnNonStringValues()
        {
            var content = RequestUtilities.CreateHttpContent(new WorkOSRequest
            {
                IsJsonContentType = false,
                Options = new NumericOptions
                {
                    Flag = true,
                    Score = 2.5,
                    Amount = 10,
                    CountShort = 3,
                    CountLong = 100,
                    OccurredAt = new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero),
                },
            });

            var body = await content.ReadAsStringAsync();
            Assert.Contains("flag=true", body);
            Assert.Contains("score=2.5", body);
            Assert.Contains("amount=10", body);
            Assert.Contains("count_short=3", body);
            Assert.Contains("count_long=100", body);
        }
    }
#pragma warning restore SA1201
}
