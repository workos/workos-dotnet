namespace WorkOS
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A client to manage requests to the WorkOS API.
    /// </summary>
    public class WorkOSClient
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
            this.HttpClient = options.HttpClient ?? this.DefaultHttpClient();
        }

        /// <summary>
        /// Describes the .NET SDK version.
        /// </summary>
        public static string SdkVersion => "1.7.0";

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

            return await this.HttpClient.SendAsync(requestMessage, cancellationToken);
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
