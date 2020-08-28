namespace WorkOSTests
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;

    public class MockHttpClient
    {
        public MockHttpClient()
        {
            this.MockHandler = new Mock<HttpClientHandler>(MockBehavior.Strict)
            {
                CallBase = true,
            };
            this.HttpClient = new HttpClient(this.MockHandler.Object);
        }

        public Mock<HttpClientHandler> MockHandler { get; }

        public HttpClient HttpClient { get; }

        public void AssertRequestWasMade(HttpMethod method, string path)
        {
            this.MockHandler.Protected()
                .Verify(
                    "SendAsync",
                    Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(m =>
                        m.Method == method &&
                        m.RequestUri.AbsolutePath == path),
                    ItExpr.IsAny<CancellationToken>());
        }

        public void MockResponse(
            HttpMethod method,
            string path,
            HttpStatusCode status,
            string response)
        {
            var responseMessage = new HttpResponseMessage
            {
                Content = new StringContent(response),
                StatusCode = status,
            };

            this.MockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m =>
                        m.Method == method &&
                        m.RequestUri.AbsolutePath == path),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(responseMessage);
        }
    }
}
