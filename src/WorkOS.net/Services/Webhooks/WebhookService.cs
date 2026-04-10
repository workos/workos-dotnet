// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;

    public class WebhookService
    {
        private const int DefaultTimeTolerance = 300;

        public static bool ConstantTimeAreEqual(byte[] a, byte[] b)
        {
            int i = a.Length;
            if (i != b.Length)
            {
                return false;
            }

            int cmp = 0;
            while (i != 0)
            {
                --i;
                cmp |= a[i] ^ b[i];
            }

            return cmp == 0;
        }

        /// <summary>
        /// Parses a webhook payload, verifies its signature, and deserializes it.
        /// </summary>
        /// <param name="json">The webhook JSON payload.</param>
        /// <param name="signatureHeader">The value of the webhook signature header.</param>
        /// <param name="secret">The webhook endpoint signing secret.</param>
        /// <param name="tolerance">The time tolerance, in seconds.</param>
        /// <returns>The deserialized webhook event.</returns>
        public Webhook ConstructEvent(string json, string signatureHeader, string secret, long tolerance = DefaultTimeTolerance)
        {
            this.VerifyHeader(json, signatureHeader, secret, tolerance);
            return JsonConvert.DeserializeObject<Webhook>(json);
        }

        /// <summary>
        /// Verifies a webhook payload and signature header.
        /// </summary>
        /// <param name="payload">The webhook JSON payload.</param>
        /// <param name="sigHeader">The webhook signature header.</param>
        /// <param name="secret">The webhook endpoint signing secret.</param>
        /// <param name="tolerance">The time tolerance, in seconds.</param>
        public void VerifyHeader(string payload, string sigHeader, string secret, long tolerance = DefaultTimeTolerance)
        {
            var timeAndSignature = this.GetTimestampAndSignature(sigHeader);
            var timeStamp = timeAndSignature.Item1;

            if (!this.VerifyTimeTolerance(timeStamp, tolerance))
            {
                throw new WorkOSWebhookException("Timestamp outside of the tolerance zone");
            }

            var signatureHash = timeAndSignature.Item2;
            var expectedSig = this.ComputeSignature(timeStamp, payload, secret);

            if (!this.SecureCompare(expectedSig, signatureHash))
            {
                throw new WorkOSWebhookException("Signature hash does not match the expected signature hash for payload");
            }
        }

        /// <summary>
        /// Extracts the timestamp and signature hash from a webhook signature header.
        /// </summary>
        /// <param name="signatureHeader">The webhook signature header.</param>
        /// <returns>The timestamp and signature hash.</returns>
        public (string TimeStamp, string SignatureHash) GetTimestampAndSignature(string signatureHeader)
        {
            var timeAndSig = signatureHeader.Split(',');
            var timeStamp = timeAndSig[0];
            var signatureHash = timeAndSig[1];
            if (!signatureHeader.Contains("t=") || !signatureHeader.Contains("v1="))
            {
                throw new ArgumentException("Unable to extract timestamp and signature hash from header");
            }

            timeStamp = timeStamp.Substring(timeStamp.IndexOf("t=", StringComparison.Ordinal) + 2).Trim();
            signatureHash = signatureHash.Substring(signatureHash.IndexOf("v1=", StringComparison.Ordinal) + 3).Trim();

            return (TimeStamp: timeStamp, SignatureHash: signatureHash);
        }

        /// <summary>
        /// Verifies that the signature timestamp is still within tolerance.
        /// </summary>
        /// <param name="timeStamp">The Unix timestamp string.</param>
        /// <param name="tolerance">The time tolerance, in seconds.</param>
        /// <returns><c>true</c> if the timestamp is valid.</returns>
        public bool VerifyTimeTolerance(string timeStamp, long tolerance)
        {
            return this.UnixTimeToDateTime(long.Parse(timeStamp)) >= DateTime.Now.AddSeconds(tolerance * -1);
        }

        /// <summary>
        /// Computes the expected webhook signature.
        /// </summary>
        /// <param name="timeStamp">The timestamp from the webhook signature header.</param>
        /// <param name="payload">The webhook JSON payload.</param>
        /// <param name="secret">The webhook endpoint signing secret.</param>
        /// <returns>The computed signature hash.</returns>
        public string ComputeSignature(string timeStamp, string payload, string secret)
        {
            var unhashedString = $"{timeStamp}.{payload}";
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(unhashedString);
            using (var hmSha256 = new HMACSHA256(secretBytes))
            {
                var hash = hmSha256.ComputeHash(payloadBytes);
                return this.ToHexString(hash);
            }
        }

        public string ToHexString(byte[] array)
        {
            var hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// Compares webhook signatures using constant-time comparison.
        /// </summary>
        /// <param name="expectedSig">The computed signature.</param>
        /// <param name="signatureHash">The signature hash from the webhook header.</param>
        /// <returns><c>true</c> if the signatures are equal.</returns>
        public bool SecureCompare(string expectedSig, string signatureHash)
        {
            var a = Encoding.ASCII.GetBytes(expectedSig);
            var b = Encoding.ASCII.GetBytes(signatureHash);
            return ConstantTimeAreEqual(a, b);
        }

        private DateTime UnixTimeToDateTime(long unixtime)
        {
            DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixtime).ToLocalTime();
            return dtDateTime;
        }
    }
}
