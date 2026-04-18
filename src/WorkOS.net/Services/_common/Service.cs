// @oagen-ignore-file
namespace WorkOS
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Base class for all WorkOS services. Owns the lazy <see cref="WorkOSClient"/>
    /// resolution and exposes a small set of <c>GetAsync</c>/<c>PostAsync</c>/
    /// <c>PutAsync</c>/<c>PatchAsync</c>/<c>DeleteAsync</c>/<c>ListAsync</c>
    /// helpers so generated service methods can be one-liners.
    /// </summary>
    public class Service
    {
        private WorkOSClient? client;

        protected Service()
        {
        }

        protected Service(WorkOSClient client)
        {
            this.client = client;
        }

        protected WorkOSClient Client
        {
            get => this.client ?? WorkOSConfiguration.WorkOSClient;
            set => this.client = value;
        }

        protected Task<T> GetAsync<T>(
            string path,
            BaseOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default) =>
            this.SendAsync<T>(HttpMethod.Get, path, options, requestOptions, cancellationToken);

        protected Task<T> PostAsync<T>(
            string path,
            BaseOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default) =>
            this.SendAsync<T>(HttpMethod.Post, path, options, requestOptions, cancellationToken);

        protected Task<T> PutAsync<T>(
            string path,
            BaseOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default) =>
            this.SendAsync<T>(HttpMethod.Put, path, options, requestOptions, cancellationToken);

        protected Task<T> PatchAsync<T>(
            string path,
            BaseOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default) =>
            this.SendAsync<T>(new HttpMethod("PATCH"), path, options, requestOptions, cancellationToken);

        protected async Task DeleteAsync(
            string path,
            BaseOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = path,
                Options = options,
                RequestOptions = requestOptions,
            };
            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }

        protected Task<WorkOSList<T>> ListAsync<T>(
            string path,
            BaseOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default) =>
            this.GetAsync<WorkOSList<T>>(path, options, requestOptions, cancellationToken);

        protected IAsyncEnumerable<T> ListAutoPagingAsync<T>(
            string path,
            BaseOptions? options = null,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = path,
                Options = options,
                RequestOptions = requestOptions,
            };
            return this.Client.ListAutoPagingAsync<T>(request, cancellationToken);
        }

        private async Task<T> SendAsync<T>(
            HttpMethod method,
            string path,
            BaseOptions? options,
            RequestOptions? requestOptions,
            CancellationToken cancellationToken)
        {
            var request = new WorkOSRequest
            {
                Method = method,
                Path = path,
                Options = options,
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<T>(request, cancellationToken);
        }
    }
}
