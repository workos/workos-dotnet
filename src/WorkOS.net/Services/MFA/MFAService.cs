namespace WorkOS
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public class MfaService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MfaService"/> class.
        /// </summary>
        public MfaService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MfaService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public MfaService(WorkOSClient client)
            : base(client)
        {
        }

        /// <summary>
        /// Enrolls user in MFA.
        /// </summary>
        /// <param name="options">Parameters used to enroll the MFA.</param>
        /// <param name="cancellationToken"> An optional token to cancel the request.</param>
        /// <returns>Enroll response.</returns>
        public async Task<Factor> EnrollFactor(
            EnrollFactorOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/auth/factors/enroll",
            };
            return await this.Client.MakeAPIRequest<Factor>(request, cancellationToken);
        }

        /// <summary>
        /// Create MFA Challenge.
        /// </summary>
        /// <param name="options">Parameters used to create the challenge.</param>
        /// <param name="cancellationToken"> An optional token to cancel the request.</param>
        /// <returns>Challenge response.</returns>
        public async Task<Challenge> ChallengeFactor(
            ChallengeFactorOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/auth/factors/challenge",
            };
            return await this.Client.MakeAPIRequest<Challenge>(request, cancellationToken);
        }

        /// <summary>
        /// Verify MFA Challenge.
        /// </summary>
        /// <param name="options">Parameters used to verify the challenge.</param>
        /// <param name="cancellationToken"> An optional token to cancel the request.</param>
        /// <returns>Successful verified response or an error response.</returns>
        public async Task<Object> VerifyFactor(
            VerifyFactorOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/auth/factors/verify",
            };
            var response = await this.Client.MakeRawAPIRequest(request, cancellationToken).ConfigureAwait(false);
            var reader = new StreamReader(
                await response.Content.ReadAsStreamAsync().ConfigureAwait(false));
            var data = await reader.ReadToEndAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                return RequestUtilities.FromJson<VerifyFactorResponseSuccess>(data);
            }
            else
            {
                return RequestUtilities.FromJson<VerifyFactorResponseError>(data);
            }
        }

        /// <summary>
        /// Gets a Factor.
        /// </summary>
        /// <param name="id">Factor unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A Factor record.</returns>
        public async Task<Factor> GetFactor(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Get,
                Path = $"/auth/factors/{id}",
            };

            return await this.Client.MakeAPIRequest<Factor>(request, cancellationToken);
        }

        /// <summary>
        /// Deletes a Factor.
        /// </summary>
        /// <param name="id">Factor unique identifier.</param>
        /// <param name="cancellationToken">
        /// An optional token to cancel the request.
        /// </param>
        /// <returns>A deleted Factor record.</returns>
        public async Task DeleteFactor(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/auth/factors/{id}",
            };

            await this.Client.MakeRawAPIRequest(request, cancellationToken);
        }
    }
}
