namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class MfaServiceTest
    {
        private readonly HttpMock httpMock;
        private readonly MfaService service;

        // private readonly Factor mockFactor;
        public MfaServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new MfaService(client);
        }

        [Fact]
        public void TestGetAuthorizationURLWithNoConnectionDomainOrProvider()
        {
            var options = new EnrollFactorOptions("generic_oidc");
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.service.EnrollFactor(options));
        }
    }
}