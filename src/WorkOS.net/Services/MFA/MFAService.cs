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
        /// <returns>Verified Challenge response.</returns>
        public async Task<VerifyFactorResponse> VerifyFactor(
            VerifyFactorOptions options,
            CancellationToken cancellationToken = default)
        {
            var request = new WorkOSRequest
            {
                Options = options,
                Method = HttpMethod.Post,
                Path = "/auth/factors/verify",
            };
            var response = await this.Client.MakeAPIRequest<VerifyFactorResponse>(request, cancellationToken);

            if (response.Challenge is null && response.IsValid is false && response.ErrorCode.Equals("authentication_challenge_previously_verified"))
            {
                throw new Exception($"The authentication challenge {options.ChallengeId} has already been verified.");
            }

            else if (response.Challenge is null && response.IsValid is false && response.ErrorCode.Equals("authentication_challenge_expired"))
            {
                throw new Exception($"The authentication challenge {options.ChallengeId} has expired.");
            }

            else if (response.Challenge is null && response.ErrorCode is null){
                throw new Exception("The application has encountered an unknown error. Please reach out to support@Workos.com");
            }

            return response;
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
