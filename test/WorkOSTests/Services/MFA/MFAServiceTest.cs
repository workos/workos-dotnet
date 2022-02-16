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
        public void TestGenericEnroll()
        {
            var options = new EnrollFactorOptions("generic_oidc");
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.service.EnrollFactor(options));
        }

        [Fact]
        public void TestSmsEnroll()
        {
            var options = new EnrollSmsFactorOptions("+15555555555");
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.service.EnrollFactor(options));
        }

        [Fact]
        public void TestTotpEnroll()
        {
            var options = new EnrollTotpFactorOptions("WorkOS", "some_user");
            var exception = Assert.ThrowsAsync<ArgumentNullException>(() =>
                this.service.EnrollFactor(options));
        }
    }
}