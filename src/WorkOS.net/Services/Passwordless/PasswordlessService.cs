// @oagen-ignore-file
namespace WorkOS
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    /// <summary>Options for creating a passwordless session.</summary>
    public class CreatePasswordlessSessionOptions : BaseOptions
    {
        [JsonProperty("email")]
        public string Email { get; set; } = default!;

        [JsonProperty("type")]
        public string Type { get; set; } = "MagicLink";

        [JsonProperty("redirect_uri")]
        public string? RedirectUri { get; set; }

        [JsonProperty("state")]
        public string? State { get; set; }

        [JsonProperty("expires_in")]
        public int? ExpiresIn { get; set; }
    }

    /// <summary>A passwordless session.</summary>
    public class PasswordlessSession
    {
        [JsonProperty("object")]
        public string Object { get; set; } = default!;

        [JsonProperty("id")]
        public string Id { get; set; } = default!;

        [JsonProperty("email")]
        public string Email { get; set; } = default!;

        [JsonProperty("expires_at")]
        public string ExpiresAt { get; set; } = default!;

        [JsonProperty("link")]
        public string Link { get; set; } = default!;
    }

    /// <summary>Passwordless (magic-link) session endpoints.</summary>
    public class PasswordlessService : Service
    {
        public PasswordlessService()
        {
        }

        public PasswordlessService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>Create a passwordless session.</summary>
        public async Task<PasswordlessSession> CreateSession(
            CreatePasswordlessSessionOptions options,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = "/passwordless/sessions",
                Options = options,
                RequestOptions = requestOptions,
            };
            return await this.Client.MakeAPIRequest<PasswordlessSession>(request, cancellationToken);
        }

        /// <summary>Send the magic-link email for a passwordless session.</summary>
        public async Task SendSession(
            string sessionId,
            RequestOptions? requestOptions = null,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Post,
                Path = $"/passwordless/sessions/{sessionId}/send",
                RequestOptions = requestOptions,
            };
            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }
    }
}
