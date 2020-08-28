namespace WorkOS
{
    using System;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class SSOService
    {
        public SSOService(WorkOSClient client)
        {
            this.Client = client;
        }

        private WorkOSClient Client { get; }

        public string GetAuthorizationURL(GetAuthorizationURLOptions options)
        {
            if (options.Domain == null && options.Provider == null)
            {
                throw new ArgumentNullException("Incomplete arguments. Need to specify either a 'domain' or 'provider'.");
            }

            var query = RequestUtilities.CreateQueryString(options);
            return $"{this.Client.ApiBaseURL}/sso/authorize?{query}";
        }

        public GetProfileResponse GetProfile(GetProfileOptions options)
        {
            options.ClientSecret = this.Client.ApiKey;
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/sso/token",
            };
            return this.Client.MakeAPIRequest<GetProfileResponse>(request);
        }

        public async Task<GetProfileResponse> GetProfileAsync(
            GetProfileOptions options,
            CancellationToken cancellationToken = default)
        {
            options.ClientSecret = this.Client.ApiKey;
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/sso/token",
            };
            return await this.Client.MakeAPIRequestAsync<GetProfileResponse >(request, cancellationToken);
        }

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
