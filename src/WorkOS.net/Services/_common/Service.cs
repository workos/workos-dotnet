namespace WorkOS
{
    public class Service
    {
        private WorkOSClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        protected Service()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="client">A client used to make requests to WorkOS.</param>
        protected Service(WorkOSClient client)
        {
            this.client = client;
        }

        /// <summary>
        /// The client used to make requests to WorkOS.
        /// </summary>
        protected WorkOSClient Client
        {
            get => this.client ?? WorkOS.WorkOSClient;
            set => this.client = value;
        }
    }
}
