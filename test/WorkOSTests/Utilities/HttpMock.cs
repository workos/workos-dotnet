// @oagen-ignore-file
namespace WorkOSTests
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Moq;
    using Moq.Protected;

    public class HttpMock
    {
        public HttpMock()
        {
            this.MockHandler = new Mock<HttpClientHandler>(MockBehavior.Strict)
            {
                CallBase = true,
            };
            this.HttpClient = new HttpClient(this.MockHandler.Object);
            this.CapturedRequests = new List<HttpRequestMessage>();
        }

        public Mock<HttpClientHandler> MockHandler { get; }

        public HttpClient HttpClient { get; }

        /// <summary>
        /// All captured HTTP requests, in order.
        /// </summary>
        public List<HttpRequestMessage> CapturedRequests { get; }

        /// <summary>
        /// Returns the body of the last captured request as a string.
        /// </summary>
        public async Task<string> GetLastRequestBodyAsync()
        {
            var last = this.CapturedRequests.LastOrDefault();
            if (last?.Content == null)
            {
                return null;
            }

            return await last.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Asserts that the last captured request body contains the given key-value pair.
        /// </summary>
        public async Task AssertRequestBodyContainsAsync(string key, string expectedValue)
        {
            var body = await this.GetLastRequestBodyAsync();
            if (body == null)
            {
                throw new Xunit.Sdk.XunitException("No request body was captured.");
            }

            if (!body.Contains($"\"{key}\"") || !body.Contains(expectedValue))
            {
                throw new Xunit.Sdk.XunitException(
                    $"Request body does not contain \"{key}\": \"{expectedValue}\". Body: {body}");
            }
        }

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
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => this.CapturedRequests.Add(req))
                .ReturnsAsync(responseMessage);
        }

        /// <summary>
        /// Mocks sequential responses for the same method + path.
        /// Each successive call returns the next response in the array.
        /// </summary>
        public void MockSequentialResponses(
            HttpMethod method,
            string path,
            HttpStatusCode status,
            string[] responses)
        {
            var setup = this.MockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m =>
                        m.Method == method &&
                        m.RequestUri.AbsolutePath == path),
                    ItExpr.IsAny<CancellationToken>());

            foreach (var response in responses)
            {
                setup.ReturnsAsync(new HttpResponseMessage
                {
                    Content = new StringContent(response),
                    StatusCode = status,
                });
            }
        }

        public void MockResponseForAnyRequest(
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
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => this.CapturedRequests.Add(req))
                .ReturnsAsync(responseMessage);
        }
    }
}
