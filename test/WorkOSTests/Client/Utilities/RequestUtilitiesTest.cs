namespace WorkOSTests
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class RequestUtilitiesTest
    {
        [Fact]
        public void TestCreateQueryString()
        {
            var options = new FakeOptions
            {
                Id = "some_id",
                Name = "some_name",
            };

            var query = RequestUtilities.CreateQueryString(options);
            Assert.Equal("id=some_id&name=some_name", query);
        }

        [Fact]
        public void TestCreateHttpContent()
        {
            var options = new FakeOptions
            {
                Id = "some_id",
                Name = "some_name",
            };

            var content = RequestUtilities.CreateHttpContent(
                new WorkOSRequest
                {
                    Options = options,
                });
            var jsonContent = content.ReadAsStringAsync().Result;
            var dictionaryContent = JsonConvert.DeserializeObject<IDictionary<string, string>>(jsonContent);
            var expectedDictionary = new Dictionary<string, string>
            {
                { "id", "some_id" },
                { "name", "some_name" },
            };

            Assert.Equal("application/json", content.Headers.ContentType.ToString());
            Assert.Equal(expectedDictionary, dictionaryContent);
        }

        [Fact]
        public async void TestCreateHttpContentUrlEncoded()
        {
            var options = new FakeOptions
            {
                Id = "some_id",
                Name = "some_name",
            };

            var content = RequestUtilities.CreateHttpContent(
                new WorkOSRequest
                {
                    IsJsonContentType = false,
                    Options = options,
                });
            var stream = await content.ReadAsStreamAsync();
            var parameters = new StreamReader(stream).ReadToEnd();
            var expectedParameters = "id=some_id&name=some_name";

            Assert.Equal("application/x-www-form-urlencoded", content.Headers.ContentType.MediaType);
            Assert.Equal(expectedParameters, parameters);
        }

        [Fact]
        public void TestToJsonString()
        {
            var options = new FakeOptions
            {
                Id = "some_id",
                Name = "some_name",
            };

            var jsonString = RequestUtilities.ToJsonString(options);
            var dictionaryContent = JsonConvert.DeserializeObject<IDictionary<string, string>>(jsonString);
            var expectedDictionary = new Dictionary<string, string>
            {
                { "id", "some_id" },
                { "name", "some_name" },
            };

            Assert.Equal(expectedDictionary, dictionaryContent);
        }

        [Fact]
        public void TestFromJson()
        {
            var jsonString = "{\"id\": \"some_id\", \"name\": \"some_name\"}";
            var result = RequestUtilities.FromJson<FakeOptions>(jsonString);
            var expectedResult = new FakeOptions
            {
                Id = "some_id",
                Name = "some_name",
            };

            Assert.Equal(expectedResult.Id, result.Id);
            Assert.Equal(expectedResult.Name, result.Name);
        }

        [Fact]
        public void TestParseURLParameters()
        {
            var url = "https://api.workos.com/sso/authorize?domain=foo&state=bar";
            var parsedUrl = RequestUtilities.ParseURLParameters(url);
            var expectedDictionary = new Dictionary<string, string>
            {
                { "domain", "foo" },
                { "state", "bar" },
            };

            Assert.Equal(expectedDictionary, parsedUrl);
        }

        private class FakeOptions : BaseOptions
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }
        }
    }
}
