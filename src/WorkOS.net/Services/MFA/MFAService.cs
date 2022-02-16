namespace WorkOS
{
    using System;
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
        /// <returns>Enroll response.</returns>
        public async Task<EnrollFactorResponse> EnrollFactor(EnrollFactorOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/auth/factors/enroll",
            };
            return await this.Client.MakeAPIRequest<EnrollFactorResponse>(request);
        }

        /// <summary>
        /// Create MFA Challenge.
        /// </summary>
        /// <param name="options">Parameters used to create the challenge.</param>
        /// <returns>Challenge response.</returns>
        public async Task<ChallengeFactorResponse> ChallengeFactor(ChallengeFactorOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/auth/factors/enroll",
            };
            return await this.Client.MakeAPIRequest<ChallengeFactorResponse>(request);
        }

        /// <summary>
        /// Verify MFA Challenge.
        /// </summary>
        /// <param name="options">Parameters used to verify the challenge.</param>
        /// <returns>Verified Challenge response.</returns>
        public async Task<VerifyFactorResponse> VerifyFactor(VerifyFactorOptions options)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/auth/factors/verify",
            };
            return await this.Client.MakeAPIRequest<VerifyFactorResponse>(request);
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
        public async Task<Factor> DeleteFactor(string id, CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Method = HttpMethod.Delete,
                Path = $"/auth/factors/{id}",
            };

            return await this.Client.MakeAPIRequest<Factor>(request, cancellationToken);
        }
    }
}
