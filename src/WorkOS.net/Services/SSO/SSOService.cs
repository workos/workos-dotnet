namespace WorkOS
{
    using System;

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
                throw new ArgumentNullException("Incomplete arguments.Need to specify either a 'domain' or 'provider'.");
            }

            var query = HttpUtils.CreateQueryString(options);
            return $"{this.Client.ApiBase}/sso/authorize?{query}";
        }
    }
}
