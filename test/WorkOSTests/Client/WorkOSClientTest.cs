namespace WorkOSTests
{
    using System;
    using System.Threading.Tasks;
    using WorkOS;
    using Xunit;

    public class WorkOSClientTest
    {
        [Fact]
        public Task TestEmptyAPIKey()
        {
            Assert.Throws<ArgumentException>(
                () => new WorkOSClient(new WorkOSOptions { }));
            return Task.CompletedTask;
        }
    }
}
