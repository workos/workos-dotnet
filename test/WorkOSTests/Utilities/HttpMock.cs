// @oagen-ignore-file
#nullable enable
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
        public async Task<string?> GetLastRequestBodyAsync()
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

        /// <summary>
        /// Asserts that the last captured request body, parsed as JSON, contains
        /// every key+value pair present in <paramref name="expectedJson"/>.
        /// Strict on values; ignores extra keys in the actual body.
        /// </summary>
        public async Task AssertRequestBodyJsonContainsAsync(string expectedJson)
        {
            var body = await this.GetLastRequestBodyAsync();
            if (body == null)
            {
                throw new Xunit.Sdk.XunitException("No request body was captured.");
            }

            var expected = Newtonsoft.Json.Linq.JToken.Parse(expectedJson);
            var actual = Newtonsoft.Json.Linq.JToken.Parse(body);
            AssertJsonSubset(expected, actual, "$");
        }

        /// <summary>
        /// Returns the query string of the last captured request as a
        /// <see cref="System.Collections.Specialized.NameValueCollection"/>.
        /// </summary>
        public System.Collections.Specialized.NameValueCollection GetLastRequestQuery()
        {
            var last = this.CapturedRequests.LastOrDefault();
            return System.Web.HttpUtility.ParseQueryString(last?.RequestUri?.Query ?? string.Empty);
        }

        /// <summary>
        /// Asserts the last captured request URL contains the given query
        /// param with the expected value (URL-decoded).
        /// </summary>
        public void AssertQueryParam(string key, string expectedValue)
        {
            var actual = this.GetLastRequestQuery()[key];
            if (actual != expectedValue)
            {
                throw new Xunit.Sdk.XunitException(
                    $"Query param '{key}': expected '{expectedValue}', got '{actual}'");
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
                        m.RequestUri!.AbsolutePath == path),
                    ItExpr.IsAny<CancellationToken>());
        }

        /// <summary>
        /// Assert that every key/value in <paramref name="expectedQuery"/>
        /// appears on the last captured request's URL query string with the
        /// exact expected value. Extra keys on the captured request are
        /// tolerated; mismatches throw.
        /// </summary>
        public void AssertQueryParams(IDictionary<string, string> expectedQuery)
        {
            var actual = this.GetLastRequestQuery();
            foreach (var kv in expectedQuery)
            {
                var got = actual[kv.Key];
                if (got != kv.Value)
                {
                    throw new Xunit.Sdk.XunitException(
                        $"Query param '{kv.Key}': expected '{kv.Value}', got '{got ?? "<missing>"}'");
                }
            }
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
                        m.RequestUri!.AbsolutePath == path),
                    ItExpr.IsAny<CancellationToken>())
                .Callback<HttpRequestMessage, CancellationToken>((req, _) => this.CapturedRequests.Add(req))
                .ReturnsAsync(responseMessage);
        }

        /// <summary>
        /// Strict variant of <see cref="MockResponse(HttpMethod,string,HttpStatusCode,string)"/>
        /// that also requires exact match on query params and, when
        /// <paramref name="bodyPredicate"/> is supplied, the request body.
        /// A request that matches path but not query (or fails the body
        /// predicate) will NOT match this setup, so Moq.Strict will fail
        /// the test on SendAsync. Lets generated tests opt into full
        /// request-shape matching without rewriting mock wiring.
        /// </summary>
        public void MockResponseExact(
            HttpMethod method,
            string path,
            IDictionary<string, string>? expectedQuery,
            System.Func<string, bool>? bodyPredicate,
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
                        m.RequestUri!.AbsolutePath == path &&
                        QueryMatches(m.RequestUri!.Query, expectedQuery) &&
                        BodyMatches(m.Content, bodyPredicate)),
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
                        m.RequestUri!.AbsolutePath == path),
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

        /// <summary>
        /// Mocks sequential responses for the same method + path where each
        /// response can have its own status code and optional headers.
        /// </summary>
        public void MockSequentialResponseMessages(
            HttpMethod method,
            string path,
            (HttpStatusCode Status, string Body, IDictionary<string, string>? Headers)[] responses)
        {
            var setup = this.MockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(m =>
                        m.Method == method &&
                        m.RequestUri!.AbsolutePath == path),
                    ItExpr.IsAny<CancellationToken>());

            foreach (var (status, body, headers) in responses)
            {
                var msg = new HttpResponseMessage
                {
                    Content = new StringContent(body),
                    StatusCode = status,
                };
                if (headers != null)
                {
                    foreach (var kv in headers)
                    {
                        msg.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }

                // Capture via a local copy to avoid closure issues.
                var captured = msg;
                setup.ReturnsAsync(() =>
                {
                    // We can't capture in the callback of SetupSequence,
                    // so we track via the ReturnsAsync factory.
                    return captured;
                });
            }
        }

        /// <summary>
        /// Mocks sequential responses for ANY request method + path where each
        /// response can have its own status code and optional headers.
        /// </summary>
        public void MockSequentialResponseMessagesForAnyRequest(
            (HttpStatusCode Status, string Body, IDictionary<string, string>? Headers)[] responses)
        {
            var setup = this.MockHandler.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());

            foreach (var (status, body, headers) in responses)
            {
                var msg = new HttpResponseMessage
                {
                    Content = new StringContent(body),
                    StatusCode = status,
                };
                if (headers != null)
                {
                    foreach (var kv in headers)
                    {
                        msg.Headers.TryAddWithoutValidation(kv.Key, kv.Value);
                    }
                }

                var captured = msg;
                setup.ReturnsAsync(() => captured);
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

        private static bool QueryMatches(string actualQuery, IDictionary<string, string>? expected)
        {
            if (expected == null)
            {
                return true;
            }

            var parsed = System.Web.HttpUtility.ParseQueryString(actualQuery ?? string.Empty);
            foreach (var kv in expected)
            {
                if (parsed[kv.Key] != kv.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool BodyMatches(HttpContent? content, System.Func<string, bool>? predicate)
        {
            if (predicate == null)
            {
                return true;
            }

            // Matchers run synchronously under Moq; block briefly on content read.
            var body = content == null ? string.Empty : content.ReadAsStringAsync().GetAwaiter().GetResult();
            return predicate(body);
        }

        private static void AssertJsonSubset(
            Newtonsoft.Json.Linq.JToken expected,
            Newtonsoft.Json.Linq.JToken actual,
            string path)
        {
            switch (expected.Type)
            {
                case Newtonsoft.Json.Linq.JTokenType.Object:
                    if (actual.Type != Newtonsoft.Json.Linq.JTokenType.Object)
                    {
                        throw new Xunit.Sdk.XunitException(
                            $"At {path}: expected object, got {actual.Type}");
                    }

                    foreach (var prop in (Newtonsoft.Json.Linq.JObject)expected)
                    {
                        var actualProp = ((Newtonsoft.Json.Linq.JObject)actual)[prop.Key];
                        if (actualProp == null)
                        {
                            throw new Xunit.Sdk.XunitException(
                                $"At {path}.{prop.Key}: expected to be present, was missing. Body: {actual}");
                        }

                        AssertJsonSubset(prop.Value!, actualProp, $"{path}.{prop.Key}");
                    }

                    break;
                case Newtonsoft.Json.Linq.JTokenType.Array:
                    if (actual.Type != Newtonsoft.Json.Linq.JTokenType.Array
                        || ((Newtonsoft.Json.Linq.JArray)expected).Count != ((Newtonsoft.Json.Linq.JArray)actual).Count)
                    {
                        throw new Xunit.Sdk.XunitException(
                            $"At {path}: array shape mismatch. Expected {expected}, got {actual}");
                    }

                    var ea = (Newtonsoft.Json.Linq.JArray)expected;
                    var aa = (Newtonsoft.Json.Linq.JArray)actual;
                    for (int i = 0; i < ea.Count; i++)
                    {
                        AssertJsonSubset(ea[i], aa[i], $"{path}[{i}]");
                    }

                    break;
                default:
                    if (!Newtonsoft.Json.Linq.JToken.DeepEquals(expected, actual))
                    {
                        throw new Xunit.Sdk.XunitException(
                            $"At {path}: expected {expected} ({expected.Type}), got {actual} ({actual.Type})");
                    }

                    break;
            }
        }
    }
}
