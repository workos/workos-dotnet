namespace WorkOSTests
{
    using System;
    using Xunit;

    using WorkOS;

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
