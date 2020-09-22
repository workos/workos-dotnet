namespace WorkOS
{
    using System;

    /// <summary>
    /// Global configuration class for interacting with the WorkOS API.
    /// </summary>
    public static class WorkOS
    {
        private static string _apiKey;

        private static WorkOSClient _workOSClient;

        /// <summary>
        /// Gets or sets a default or custom <see cref="WorkOSClient"/>.
        /// </summary>
        public static WorkOSClient WorkOSClient
        {
            get
            {
                if (_workOSClient == null)
                {
                    _workOSClient = DefaultWorkOSClient();
                }

                return _workOSClient;
            }
            set => _workOSClient = value;
        }

        /// <summary>
        /// Sets the API key.
        /// </summary>
        /// <param name="apiKey">The API key value.</param>
        public static void SetApiKey(string apiKey)
        {
            if (apiKey == null || apiKey.Length == 0)
            {
                throw new ArgumentException("API Key is required", nameof(apiKey));
            }

            _apiKey = apiKey;
        }

        private static WorkOSClient DefaultWorkOSClient()
        {
            return new WorkOSClient(
                new WorkOSOptions
                {
                    ApiKey = _apiKey,
                });
        }
    }
}
