namespace WorkOSTests
{
    using System;
    using WorkOS;
    using Xunit;

    public class WorkOSClientTest
    {
        [Fact]
        public void TestEmptyAPIKey()
        {
            Assert.Throws<ArgumentException>(
                () => new WorkOSClient(new WorkOSOptions { }));
        }
    }
}
