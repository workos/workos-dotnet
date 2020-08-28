namespace WorkOS
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    public class WorkOSClient
    {
        public WorkOSClient(WorkOSOptions options)
        {
            this.ApiBaseURL = options.ApiBaseURL ?? DefaultApiBaseURL;
            this.ApiKey = options.ApiKey;
            this.HttpClient = options.HttpClient ?? this.DefaultHttpClient();
        }

        public static TimeSpan DefaultTimeout => TimeSpan.FromSeconds(60);

        public static string DefaultApiBaseURL => "https://api.workos.com";

        public readonly string ApiBaseURL;

        public readonly string ApiKey;

        private readonly HttpClient HttpClient;

        public HttpClient DefaultHttpClient()
        {
            return new HttpClient
            {
                Timeout = DefaultTimeout,
            };
        }

        public T MakeAPIRequest<T>(WorkOSRequest request)
        {
            return this.MakeAPIRequestAsync<T>(request)
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task<T> MakeAPIRequestAsync<T>(
            WorkOSRequest request,
            CancellationToken cancellationToken = default)
        {
            var requestMessage = this.CreateHttpRequestMessage(request);

            HttpResponseMessage response = null;

            try
            {
                response = await this.HttpClient
                    .SendAsync(requestMessage, cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (HttpRequestException exception)
            {

            }

            var reader = new StreamReader(
                await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
            var data = await reader.ReadToEndAsync().ConfigureAwait(false);
            return RequestUtilities.FromJson<T>(data);
        }

        private HttpRequestMessage CreateHttpRequestMessage(WorkOSRequest request)
        {
            string url = this.ApiBaseURL + request.Path;
            StringContent content = null;

            if (request.Method == HttpMethod.Post)
            {
                content = RequestUtilities.CreateHttpContent(request.Options);
            }

            var requestMessage = new HttpRequestMessage(request.Method, new Uri(url));
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this.ApiKey);
            //requestMessage.Headers.UserAgent = 
            requestMessage.Content = content;
            return requestMessage;
        }
    }
}
