namespace WorkOS
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A service that offers methods to interact with WorkOS SSO.
    /// </summary>
    public class SSOService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SSOService"/> class.
        /// </summary>
        public SSOService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SSOService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public SSOService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Generates an Authorization URL to initiate the WorkOS OAuth 2.0 flow.
        /// </summary>
        /// <param name="options">Parameters used to generate the URL.</param>
        /// <returns>An Authorization URL.</returns>
        public string GetAuthorizationURL(GetAuthorizationURLOptions options)
        {
            if (options.Domain == null && options.Provider == null)
            {
                throw new ArgumentNullException("Incomplete arguments. Need to specify either a 'domain' or 'provider'.");
            }

            var query = RequestUtilities.CreateQueryString(options);
            return $"{this.Client.ApiBaseURL}/sso/authorize?{query}";
        }

        /// <summary>
        /// Retrieves a <see cref="Profile"/> for an authenticated User.
        /// </summary>
        /// <param name="options">Options to fetch an authenticated User.</param>
        /// <returns>A WorkOS Profile record.</returns>
        public GetProfileResponse GetProfile(GetProfileOptions options)
        {
            options.ClientSecret = this.Client.ApiKey;
            var request = new WorkOSRequest
            {
                IsJsonContentType = false,
                Options = options,
                Method = HttpMethod.Post,
                Path = "/sso/token",
            };
            return this.Client.MakeAPIRequest<GetProfileResponse>(request);
        }

        /// <summary>
        /// Asynchronously retrieves a <see cref="Profile"/> for an
        /// authenticated User.
        /// </summary>
        /// <param name="options">Options to fetch an authenticated User.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a WorkOS Profile record.</returns>
        public async Task<GetProfileResponse> GetProfileAsync(
            GetProfileOptions options,
            CancellationToken cancellationToken = default)
        {
            options.ClientSecret = this.Client.ApiKey;
            var request = new WorkOSRequest
            {
                IsJsonContentType = false,
                Options = options,
                Method = HttpMethod.Post,
                Path = "/sso/token",
            };
            return await this.Client.MakeAPIRequestAsync<GetProfileResponse>(request, cancellationToken);
        }

        /// <summary>
        /// Activates a WorkOS Draft Connection.
        /// </summary>
        /// <param name="options">Options to activate a Draft Connection.</param>
        /// <returns>A WorkOS Connection.</returns>
        public Connection CreateConnection(CreateConnectionOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/connections",
            };
            return this.Client.MakeAPIRequest<Connection>(request);
        }

        /// <summary>
        /// Asynchronously activates a WorkOS Draft Connection.
        /// </summary>
        /// <param name="options">Options to activate a Draft Connection.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Task wrapping a WorkOS Connection record.</returns>
        public async Task<Connection> CreateConnectionAsync(
            CreateConnectionOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/connections",
            };
            return await this.Client.MakeAPIRequestAsync<Connection>(request, cancellationToken);
        }
    }
}
