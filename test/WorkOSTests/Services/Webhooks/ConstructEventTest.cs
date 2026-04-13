// @oagen-ignore-file
namespace WorkOSTests
{
    using System;
    using WorkOS;
    using Xunit;

    /// <summary>
    /// Tests for <see cref="WebhookService.ConstructEvent"/>: signature
    /// verification (success + the major failure modes) and deserialization
    /// into the <see cref="Webhook"/> envelope. Webhook handling is
    /// security-sensitive, so these paths must be covered directly.
    /// </summary>
    public class ConstructEventTest
    {
        private const string Secret = "nTqlYkHe6GyqYkmUQWksUWYmQ";

        private const string Payload =
            "{\"id\":\"wh_01FJ27WB8SNT7VN72KRMYJYV8P\"," +
            "\"data\":{\"id\":\"directory_user_01E1X1B89NH8Z3SDFJR4H7RGX7\"," +
            "\"state\":\"active\"," +
            "\"emails\":[{\"type\":\"work\",\"value\":\"veda@example.com\",\"primary\":true}]," +
            "\"username\":\"veda@example.com\",\"last_name\":\"Block\",\"first_name\":\"Lela\"," +
            "\"directory_id\":\"directory_01E1X194NTJ3PYMAY79DYV0F0P\"}," +
            "\"event\":\"dsync.user.created\"}";

        [Fact]
        public void ConstructEvent_ValidSignature_DeserializesWebhook()
        {
            var service = new WebhookService();
            var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var signature = service.ComputeSignature(timeStamp, Payload, Secret);
            var header = $"t={timeStamp}, v1={signature}";

            var webhook = service.ConstructEvent(Payload, header, Secret);

            Assert.NotNull(webhook);
            Assert.Equal("wh_01FJ27WB8SNT7VN72KRMYJYV8P", webhook.Id);
            Assert.Equal("dsync.user.created", webhook.Event);
            Assert.NotNull(webhook.Data);
        }

        [Fact]
        public void ConstructEvent_TamperedPayload_Throws()
        {
            var service = new WebhookService();
            var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var signature = service.ComputeSignature(timeStamp, Payload, Secret);
            var header = $"t={timeStamp}, v1={signature}";

            var tampered = Payload.Replace("veda@example.com", "attacker@example.com");

            Assert.Throws<WorkOSWebhookException>(() => service.ConstructEvent(tampered, header, Secret));
        }

        [Fact]
        public void ConstructEvent_WrongSecret_Throws()
        {
            var service = new WebhookService();
            var timeStamp = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();
            var signature = service.ComputeSignature(timeStamp, Payload, Secret);
            var header = $"t={timeStamp}, v1={signature}";

            Assert.Throws<WorkOSWebhookException>(
                () => service.ConstructEvent(Payload, header, "a_different_secret"));
        }

        [Fact]
        public void ConstructEvent_ExpiredTimestamp_Throws()
        {
            var service = new WebhookService();

            // 1 hour ago (well outside the default 300s tolerance)
            var oldTs = DateTimeOffset.Now.AddHours(-1).ToUnixTimeMilliseconds().ToString();
            var signature = service.ComputeSignature(oldTs, Payload, Secret);
            var header = $"t={oldTs}, v1={signature}";

            Assert.Throws<WorkOSWebhookException>(
                () => service.ConstructEvent(Payload, header, Secret));
        }

        [Fact]
        public void ConstructEvent_MalformedHeader_Throws()
        {
            var service = new WebhookService();
            Assert.Throws<ArgumentException>(
                () => service.ConstructEvent(Payload, "garbage-header,without-components", Secret));
        }
    }
}
