// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
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
        // Non-spec service backing fields (hand-maintained)
        private PasswordlessService? passwordless;
        private VaultService? vault;
        private ActionsService? actions;
        private SessionService? session;

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
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkOSClient"/> class for mocking.
        /// </summary>
        protected WorkOSClient()
        {
            this.ApiBaseURL = default!;
            this.ApiKey = default!;
            this.HttpClient = default!;
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

        /// <summary>
        /// The WorkOS Client ID (e.g., <c>client_01H...</c>) used for SSO and User Management
        /// endpoints that require an OAuth client identifier. May be <see langword="null"/>
        /// if the client was constructed without one; operations that require a Client ID will
        /// throw via <see cref="RequireClientId"/>.
        /// </summary>
        public string? ClientId { get; }

        // Non-spec service accessors (hand-maintained)

        /// <summary>Gets the Passwordless service for magic-link sessions.</summary>
        public virtual PasswordlessService Passwordless => this.passwordless ??= new PasswordlessService(this);

        /// <summary>Gets the Vault service for KV storage and encryption.</summary>
        public virtual VaultService Vault => this.vault ??= new VaultService(this);

        /// <summary>Gets the Actions service for AuthKit Actions verification and signing.</summary>
        public virtual ActionsService Actions => this.actions ??= new ActionsService();

        /// <summary>Gets the Session service for sealed session management and JWT validation.</summary>
        public virtual SessionService Session => this.session ??= new SessionService(this);

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
        /// Constructs the absolute URL the SDK would send for <paramref name="request"/>,
        /// without performing any HTTP I/O. Used by URL-builder methods such as
        /// <see cref="SSOService.GetAuthorizationUrl"/> and
        /// <see cref="UserManagementService.GetAuthorizationUrl"/>, which return a URL
        /// that the caller redirects the browser to rather than calling themselves.
        /// </summary>
        /// <param name="request">The request to build a URI for. Only the Path and (for
        /// GET/DELETE) Options participate; Method, AccessToken, headers, and request
        /// bodies are ignored.</param>
        /// <returns>The full request URI including any query string.</returns>
        public virtual Uri BuildRequestUri(WorkOSRequest request)
        {
            return this.BuildUri(request);
        }

        /// <summary>
        /// Returns the configured <see cref="ClientId"/>, throwing an <see cref="InvalidOperationException"/>
        /// with a descriptive message if it was not set. Used by generated services for endpoints
        /// that require a <c>client_id</c> parameter (SSO, User Management authenticate-with-* flows).
        /// </summary>
        /// <returns>The non-empty Client ID.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the client was configured without a <see cref="ClientId"/>.</exception>
        public string RequireClientId()
        {
            if (string.IsNullOrEmpty(this.ClientId))
            {
                throw new InvalidOperationException(
                    "ClientId is required for this operation. Set WorkOSOptions.ClientId when constructing the WorkOSClient.");
            }

            return this.ClientId!;
        }

        /// <summary>
        /// Makes a request to the WorkOS API.
        /// </summary>
        /// <param name="request">The request to make to the WorkOS API.</param>
        /// <param name="cancellationToken">A token used to cancel the request.</param>
        /// <returns>The response from the WorkOS API.</returns>
        public virtual async Task<HttpResponseMessage> MakeRawAPIRequest(
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
        public virtual async Task<T> MakeAPIRequest<T>(
            WorkOSRequest request,
            CancellationToken cancellationToken = default)
        {
            var response = await this.MakeRawAPIRequest(request, cancellationToken).ConfigureAwait(false);
            var reader = new StreamReader(
                await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
            var data = await reader.ReadToEndAsync().ConfigureAwait(false);
            return RequestUtilities.FromJson<T>(data);
        }

        /// <summary>
        /// Auto-pages through a list endpoint, yielding individual items across all pages.
        /// </summary>
        /// <typeparam name="T">The item type.</typeparam>
        /// <param name="request">The initial list request.</param>
        /// <param name="cancellationToken">A token used to cancel the request.</param>
        /// <returns>An async sequence of items.</returns>
        public async IAsyncEnumerable<T> ListAutoPagingAsync<T>(
            WorkOSRequest request,
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Clone the caller's options up front so advancement never
            // mutates state the caller still holds a reference to. The
            // first request uses the untouched clone; subsequent pages
            // update only our copy.
            var workingOptions = request.Options?.Clone();
            var workingRequest = new WorkOSRequest
            {
                Method = request.Method,
                Path = request.Path,
                AccessToken = request.AccessToken,
                Options = workingOptions,
                RequestOptions = request.RequestOptions,
            };

            string? afterCursor = null;
            while (true)
            {
                if (afterCursor != null && workingOptions is ListOptions listOpts)
                {
                    // Advancing with `after` — clear `before` so we don't send
                    // both cursors (which the API treats as an invalid request).
                    listOpts.After = afterCursor;
                    listOpts.Before = null;
                }

                var page = await this.MakeAPIRequest<WorkOSList<T>>(workingRequest, cancellationToken)
                    .ConfigureAwait(false);

                if (page?.Data == null)
                {
                    yield break;
                }

                foreach (var item in page.Data)
                {
                    yield return item;
                }

                afterCursor = page.ListMetadata?.After;
                if (string.IsNullOrEmpty(afterCursor))
                {
                    yield break;
                }
            }
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

        private static bool UsesQueryString(HttpMethod method) =>
            method == HttpMethod.Get || method == HttpMethod.Delete;

        private HttpRequestMessage CreateHttpRequestMessage(WorkOSRequest request)
        {
            Uri uri = this.BuildUri(request);
            HttpContent? content = null;

            if (!UsesQueryString(request.Method))
            {
                content = RequestUtilities.CreateHttpContent(request);
            }

            var userAgentString = $"workos-dotnet/{SdkVersion}";
            var requestMessage = new HttpRequestMessage(request.Method, uri);
            requestMessage.Headers.AcceptEncoding.Add(new StringWithQualityHeaderValue("utf-8"));

            // Per-request API key override via RequestOptions
            var apiKey = request.RequestOptions?.ApiKey ?? this.ApiKey;

            if (!string.IsNullOrWhiteSpace(request.AccessToken))
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.AccessToken);
            }
            else
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            }

            // Idempotency key via RequestOptions
            if (!string.IsNullOrWhiteSpace(request.RequestOptions?.IdempotencyKey))
            {
                requestMessage.Headers.TryAddWithoutValidation("Idempotency-Key", request.RequestOptions.IdempotencyKey);
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

            if (UsesQueryString(request.Method) && options != null)
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
