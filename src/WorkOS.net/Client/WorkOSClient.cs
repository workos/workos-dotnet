namespace WorkOS
{
    public class WorkOSClient
    {
        public WorkOSClient(WorkOSOptions options)
        {
            this.ApiBase = options.ApiBase ?? DefaultApiBase;
            this.ApiKey = options.ApiKey;
        }

        public static string DefaultApiBase => "https://api.workos.com";

        public string ApiBase { get; }

        public string ApiKey { get; }
    }
}
