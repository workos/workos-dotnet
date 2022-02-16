namespace WorkOSTests
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WorkOS;
    using Xunit;

    public class MFAServiceTest
    {
        private readonly HttpMock httpMock;
        private readonly MFAService service;

        // private readonly Factor mockFactor;
        public MFAServiceTest()
        {
            this.httpMock = new HttpMock();
            var client = new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = "test",
                    HttpClient = this.httpMock.HttpClient,
                });
            this.service = new MFAService(client);
        }

        [Fact]
        public void TestGetAuthorizationURLWithNoConnectionDomainOrProvider()
        {
            var options = new EnrollFactorOptions()
            {
                Type = "generic_oidc",
            };
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.service.EnrollFactor(options));
        }
    }
}