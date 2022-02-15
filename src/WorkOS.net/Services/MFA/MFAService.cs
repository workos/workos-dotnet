namespace WorkOS
{
    using System.Net.Http;
    using System.Threading.Tasks;
    public class MFAService : Service
    {
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

    }

}