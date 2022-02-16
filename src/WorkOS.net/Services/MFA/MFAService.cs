namespace WorkOS
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public class MFAService : Service
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MFAService"/> class.
        /// </summary>
        public MFAService()
            : base(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MFAService"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        public MFAService(WorkOSClient client)
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
    }
}
