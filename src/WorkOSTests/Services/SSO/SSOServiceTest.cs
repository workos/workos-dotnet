namespace WorkOSTests.Services.SSO
{
    using System.Collections.Generic;
    using System.Net;
    using System.Linq;
    using Xunit;

    using WorkOS;

    public class SSOServiceTest
    {
        private readonly SSOService service;

        public SSOServiceTest()
        {
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                }
            );
            this.service = new SSOService(client);
        }

        [Fact]
        public void TestGetAuthorizationURLWithDomain()
        {
            var options = new GetAuthorizationURLOptions
            {
                ClientId = "project_123",
                Domain = "foo-corp.com",
                RedirectURI = "https://example.com/sso/callback"
            };
            string url = this.service.GetAuthorizationURL(options);

            Dictionary<string, string> parameters = this.ParseURLParameters(url);
            Assert.Equal(options.ClientId, parameters["client_id"]);
            Assert.Equal(options.Domain, parameters["domain"]);
            Assert.Equal(options.RedirectURI, parameters["redirect_uri"]);
            Assert.Equal("code", parameters["response_type"]);
        }

        private Dictionary<string, string> ParseURLParameters(string url)
        {
            int startIndex = url.IndexOf('?') + 1;
            return url.Substring(startIndex).Split('&')
                .Select(x => x.Split(new[] { '=' }, 2))
                .ToDictionary(x => WebUtility.UrlDecode(x[0]), x => WebUtility.UrlDecode(x[1]));
        }
    }
}
