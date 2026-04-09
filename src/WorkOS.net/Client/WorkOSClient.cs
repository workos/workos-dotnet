// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A client to manage requests to the WorkOS API.
    /// </summary>
    public partial class WorkOSClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorkOSClient"/> class.
        /// </summary>
        /// <param name="options">Parameters to create the client with.</param>
        public WorkOSClient(WorkOSOptions options)
        {
            if (options.ApiKey == null || options.ApiKey.Length == 0)
            {
                throw new ArgumentException("API Key is required", nameof(options.ApiKey));
            }

            this.ApiBaseURL = options.ApiBaseURL ?? DefaultApiBaseURL;
            this.ApiKey = options.ApiKey;
            this.ClientId = options.ClientId;
            this.HttpClient = options.HttpClient ?? this.DefaultHttpClient();
            this.InitializeGeneratedServices();
        }

        /// <summary>
        /// Describes the .NET SDK version.
        /// </summary>
        public static string SdkVersion =>
            typeof(WorkOSClient).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion
                .Split('+')[0] ?? "unknown";

        /// <summary>
        /// Default timeout for HTTP requests.
        /// </summary>
        public static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(60);

        /// <summary>
        /// Default base URL for the WorkOS API.
        /// </summary>
        public static string DefaultApiBaseURL => "https://api.workos.com";

        /// <summary>
        /// The base URL for the WorkOS API.
        /// </summary>
        public string ApiBaseURL { get; }

        /// <summary>
        /// The API key used to authenticate requests to the WorkOS API.
        /// </summary>
        public string ApiKey { get; }

        public string ClientId { get; }

        /// <summary>
        /// The client used to make HTTP requests to the WorkOS API.
        /// </summary>
        private HttpClient HttpClient { get; }

        /// <summary>
        /// Creates a new HTTP client instance.
        /// </summary>
        /// <returns>An instance of the HTTP client to make requests.</returns>
        public HttpClient DefaultHttpClient()
        {
            return new HttpClient
            {
                Timeout = DefaultTimeout,
            };
        }

        /// <summary>
        /// Makes a request to the WorkOS API.
        /// </summary>
        /// <param name="request">The request to make to the WorkOS API.</param>
        /// <param name="cancellationToken">A token used to cancel the request.</param>
        /// <returns>The response from the WorkOS API.</returns>
        public async Task<HttpResponseMessage> MakeRawAPIRequest(
            WorkOSRequest request,
            CancellationToken cancellationToken = default)
        {
            var requestMessage = this.CreateHttpRequestMessage(request);
            var response = await this.HttpClient.SendAsync(requestMessage, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                throw MapError(response.StatusCode, body);
            }

            return response;
        }

        /// <summary>
        /// Makes a request to the WorkOS API and parses the JSON response.
        /// </summary>
        /// <typeparam name="T">The return type from the request.</typeparam>
        /// <param name="request">The request to make to the WorkOS API.</param>
        /// <param name="cancellationToken">A token used to cancel the request.</param>
        /// <returns>The response from the WorkOS API.</returns>
        public async Task<T> MakeAPIRequest<T>(
            WorkOSRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = await this.MakeRawAPIRequest(request, cancellationToken).ConfigureAwait(false);
            var reader = new StreamReader(
                await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
            var data = await reader.ReadToEndAsync().ConfigureAwait(false);
            return RequestUtilities.FromJson<T>(data);
        }

        private static Exception MapError(HttpStatusCode statusCode, string body)
        {
            switch ((int)statusCode)
            {
                case 401:
                    return new AuthenticationError(body);
                case 404:
                    return new NotFoundError(body);
                case 422:
                    return new UnprocessableEntityError(body);
                case 429:
                    return new RateLimitExceededError(body);
                default:
                    if ((int)statusCode >= 500)
                    {
                        return new ServerError(body);
                    }

                    return new ApiError(body, statusCode);
            }
        }

        private HttpRequestMessage CreateHttpRequestMessage(WorkOSRequest request)
        {
            Uri uri = this.BuildUri(request);
            HttpContent content = null;

            if (request.Method != HttpMethod.Get)
            {
                content = RequestUtilities.CreateHttpContent(request);
            }

            var userAgentString = $"workos-dotnet/{SdkVersion}";
            var requestMessage = new HttpRequestMessage(request.Method, uri);
            requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));

            if (!string.IsNullOrWhiteSpace(request.AccessToken))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);
            }
            else
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.ApiKey);
            }

            requestMessage.Headers.TryAddWithoutValidation("User-Agent", userAgentString);
            if (request.WorkOSHeaders != null)
            {
                foreach (var header in request.WorkOSHeaders)
                {
                    requestMessage.Headers.Add(header.Key, header.Value);
                }
            }

            requestMessage.Content = content;
            return requestMessage;
        }

        private Uri BuildUri(WorkOSRequest request)
        {
            var builder = new StringBuilder();
            var options = request.Options;
            builder.Append(this.ApiBaseURL);
            builder.Append(request.Path);

            if (request.Method != HttpMethod.Post && options != null)
            {
                var queryParameters = RequestUtilities.CreateQueryString(options);
                if (queryParameters != null && queryParameters.Length > 0)
                {
                    builder.Append("?");
                    builder.Append(queryParameters);
                }
            }

            return new Uri(builder.ToString());
        }
    }
}
