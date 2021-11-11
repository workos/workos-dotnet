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
        /// Parses a JSON string from a webhook, verifies and deseralizes into a Webhook Object.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="signatureHeader">
        /// The value of the header from the webhook request.
        /// </param>
        /// <param name="secret">The webhook endpoint's signing secret.</param>
        /// <param name="tolerance">The time tolerance, in seconds.</param>
        /// <returns>The deserialized JSON.</returns>
        public Webhook ConstructEvent(
            string json,
            string signatureHeader,
            string secret,
            long tolerance)
        {
            if (this.VerifyHeader(json, signatureHeader, secret, tolerance))
            {
                Webhook webhookVerified = JsonConvert.DeserializeObject<Webhook>(json);
                return webhookVerified;
            }
            else
            {
                throw new Exception("Unable to verify Webhook");
            }
        }

        /// <summary>
        /// Verify Header.
        /// </summary>
        /// <param name="payload">
        /// json payload.
        /// </param>
        /// <param name="sigHeader">
        /// signature header of webhook.
        /// </param>
        /// <param name="secret">
        /// secret.
        /// </param>
        /// <param name="tolerance">
        /// time tolerance for timing attacks.
        /// </param>
        /// <returns>Boolean True or False of verification.</returns>
        public bool VerifyHeader(string payload, string sigHeader, string secret, long tolerance)
        {
            var timeAndSignature = this.GetTimestampAndSignature(sigHeader);
            var timeStamp = timeAndSignature.Item1;

            if (!this.Verify_Time_Tolerance(timeStamp, tolerance))
            {
                throw new Exception("Timestamp outside of the tolerance zone");
            }

            var signatureHash = timeAndSignature.Item2;
            var expectedSig = this.ComputeSignature(timeStamp, payload, secret);

            if (this.SecureCompare(expectedSig, signatureHash))
            {
                return true;
            }
            else
            {
                throw new Exception("Signature hash does not match the expected signature hash for payload");
            }
        }

        /// <summary>
        /// Get timestamp and signature hash.
        /// </summary>
        /// <param name="signatureHeader">
        /// Signatures header.
        /// </param>
        /// <returns> Tuple of [DateTime timestamp, string signaturehash].</returns>
        public (string Timestamp, string SignatureHash) GetTimestampAndSignature(string signatureHeader)
        {
            var timeAndSig = signatureHeader.Split(',');
            var timeStamp = timeAndSig[0];
            var signatureHash = timeAndSig[1];
            if (string.IsNullOrEmpty(timeStamp) || string.IsNullOrEmpty(signatureHash))
            {
                throw new ArgumentException("Unable to extract timestamp and signature hash from header");
            }

            var removeT = "t=";
            var t = timeStamp.IndexOf(removeT);
            if (t != -1)
            {
                timeStamp = timeStamp.Substring(t + removeT.Length);
            }

            if (string.IsNullOrEmpty(timeStamp))
            {
                throw new Exception("Unable to extract timestamp");
            }

            var removeV = "v1=";
            var v = signatureHash.IndexOf(removeV);
            if (v != -1)
            {
                signatureHash = signatureHash.Substring(v + removeV.Length, 64);
            }

            if (string.IsNullOrEmpty(signatureHash))
            {
                throw new Exception("Unable to extract signatureHash");
            }

            return (Timestamp: timeStamp, SignatureHash: signatureHash);
        }

        /// <summary>
        /// Convert Unix time value to a DateTime object.
        /// </summary>
        /// <param name="unixtime">The Unix time stamp you want to convert to DateTime.</param>
        /// <returns>Returns a DateTime object that represents value of the Unix time.</returns>
        public DateTime UnixTimeToDateTime(long unixtime)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(unixtime).ToLocalTime();
            return dtDateTime;
        }

        /// <summary>
        /// Verify if timestamp is within tolerance.
        /// </summary>
        /// <param name="timeStamp">Unix timestamp string.</param>
        /// <param name="tolerance">The time tolerance, in seconds.</param>
        /// <returns>bool of if time is within tolerance.</returns>
        public bool Verify_Time_Tolerance(string timeStamp, long tolerance)
        {
            if (this.UnixTimeToDateTime(long.Parse(timeStamp)) < DateTime.Now.AddSeconds(tolerance * -1))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Compute expected signature.
        /// </summary>
        /// <param name="timeStamp">
        /// string time stamp from header.
        /// </param>
        /// <param name="payload">
        /// json payload of webhook.
        /// </param>
        /// <param name="secret">
        /// secret used for encoding.
        /// </param>
        /// <returns>string of expected signature.</returns>
        public string ComputeSignature(string timeStamp, string payload, string secret)
        {
            var unhashedString = $"{timeStamp}.{payload}";
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(unhashedString);
            using (var hmSha256 = new HMACSHA256(secretBytes))
            {
                var hash = hmSha256.ComputeHash(payloadBytes);
                return $"{this.ToHexString(hash)}";
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
        /// A constant time equals comparison.
        /// </summary>
        /// <param name="expectedSig">computed signature.</param>
        /// <param name="signatureHash">signuatre from header.</param>
        /// <returns>true if arrays equal, false otherwise.</returns>
        public bool SecureCompare(string expectedSig, string signatureHash)
        {
            var a = Encoding.ASCII.GetBytes(expectedSig);
            var b = Encoding.ASCII.GetBytes(signatureHash);
            return ConstantTimeAreEqual(a, b);
        }
    }
}