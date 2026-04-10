// @oagen-ignore-file
namespace WorkOS
{
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
    }
}
