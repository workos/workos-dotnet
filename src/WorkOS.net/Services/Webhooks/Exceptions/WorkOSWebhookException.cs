namespace WorkOS
{
    using System;

    public class WorkOSWebhookException : Exception
    {
        public WorkOSWebhookException(string message)
            : base(message)
        {
        }
    }
}