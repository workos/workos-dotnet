// @oagen-ignore-file
namespace WorkOSTests
{
    using System;
    using WorkOS;
    using Xunit;

    public class WebhookTests
    {
        [Fact]
        public void TestParseTime()
        {
            var service = new WebhookService();
            string timeStamp = "1634310434103";
            string sigHeader = "sig_header: 't=1634310434103, v1=a8cab83e055b343942570916202b2162eadb387f66f369938636d70cc7f540c0";
            var returnTuple = service.GetTimestampAndSignature(sigHeader);

            Assert.Equal(timeStamp, returnTuple.Item1, true);
        }

        [Fact]
        public void TestParseSig()
        {
            var service = new WebhookService();
            string signature = "a8cab83e055b343942570916202b2162eadb387f66f369938636d70cc7f540c0";
            string sigHeader = "sig_header: 't=1634310434103, v1=a8cab83e055b343942570916202b2162eadb387f66f369938636d70cc7f540c0";
            var returnTuple = service.GetTimestampAndSignature(sigHeader);

            Assert.Equal(signature, returnTuple.Item2, true);
        }

        [Fact]
        public void VerifyTestTimeTolerance()
        {
            var service = new WebhookService();
            string timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            long tolerance = 300;

            Assert.True(service.VerifyTimeTolerance(timeStamp, tolerance));
        }

        [Fact]
        public void TestExpectedSignature()
        {
            var service = new WebhookService();
            string payload = "{\"id\":\"wh_01FJ27WB8SNT7VN72KRMYJYV8P\",\"data\":{\"id\":\"directory_user_01E1X1B89NH8Z3SDFJR4H7RGX7\",\"state\":\"active\",\"emails\":[{\"type\":\"work\",\"value\":\"veda@example.com\",\"primary\":true}],\"username\":\"veda@example.com\",\"last_name\":\"Block\",\"first_name\":\"Lela\",\"directory_id\":\"directory_01E1X194NTJ3PYMAY79DYV0F0P\"},\"event\":\"dsync.user.created\"}";
            string secret = "nTqlYkHe6GyqYkmUQWksUWYmQ";
            string timeStamp = "1634310434103";
            string signature = "0de6f84a9c3dd217973c44dffbcb3fd1225a4ab453e08b608491479ebab382e8";
            string expectedSig = service.ComputeSignature(timeStamp, payload, secret);

            Assert.True(service.SecureCompare(expectedSig, signature));
        }
    }
}
