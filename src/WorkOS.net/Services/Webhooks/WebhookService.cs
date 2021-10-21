namespace WorkOS
{
    using System;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;

    public class WebhookService
    {
        private const int DefaultTimeTolerance = 300;

        /// <summary>
        /// Parses a JSON string from a webhook, verifies and deseralizes into a Webhook Object.
        /// </summary>
        /// <param name="json">The JSON string to parse.</param>
        /// <param name="stripeSignatureHeader">
        /// The value of the header from the webhook request.
        /// </param>
        /// <param name="secret">The webhook endpoint's signing secret.</param>
        /// <param name="tolerance">The time tolerance, in seconds.</param>
        /// <returns>The deserialized JSON.</returns>
        public Webhook ConstructEvent(
            string json,
            string stripeSignatureHeader,
            string secret,
            long tolerance)
        {
            if (this.Verify_header(json, stripeSignatureHeader, secret, tolerance))
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
        /// <param name="sig_header">
        /// signature header of webhook.
        /// </param>
        /// <param name="secret">
        /// secret.
        /// </param>
        /// <param name="tolerance">
        /// time tolerance for timing attacks.
        /// </param>
        /// <returns>Boolean True or False of verification.</returns>
        public bool Verify_header(string payload, string sig_header, string secret, long tolerance)
        {
            var timeAndSignature = this.Get_Timestamp_and_Signature_Hash(sig_header);
            string timeStamp = timeAndSignature.Item1;

            if (this.UnixTimeToDateTime(long.Parse(timeStamp)) < DateTime.Now.AddSeconds(tolerance * -1))
            {
                throw new Exception("Timestamp outside of the tolerance zone");
            }

            string signatureHash = timeAndSignature.Item2;
            string expected_sig = this.Compute_Signature(timeStamp, payload, secret);
            byte[] a = Encoding.ASCII.GetBytes(expected_sig);
            byte[] b = Encoding.ASCII.GetBytes(signatureHash);

            if (a == b)
            {
                return true;
            }
            else
            {
                throw new Exception("expected signature: " + expected_sig + " did not match signature: " + signatureHash);
            }
        }

        /// <summary>
        /// Get timestamp and signature hash.
        /// </summary>
        /// <param name="signature_header">
        /// Signatures header.
        /// </param>
        /// <returns> Tuple of [DateTime timestamp, string signaturehash].</returns>
        public Tuple<string, string> Get_Timestamp_and_Signature_Hash(string signature_header)
        {
            string[] timeAndSig = signature_header.Split(',');
            string timeStamp = timeAndSig[0];
            string signatureHash = timeAndSig[1];
            if (string.IsNullOrEmpty(timeStamp) || string.IsNullOrEmpty(signatureHash))
            {
                Console.WriteLine("Unable to extract timestamp and signature hash from header");
            }

            string removeT = "t=";
            int t = timeStamp.IndexOf(removeT);
            if (t != -1)
            {
                timeStamp = timeStamp.Substring(t + removeT.Length);
            }

            if (string.IsNullOrEmpty(timeStamp))
            {
                throw new Exception("Unable to extract timestamp");
            }

            string removeV = "v1=";
            int v = signatureHash.IndexOf(removeV);
            if (v != -1)
            {
                signatureHash = signatureHash.Substring(v + removeV.Length, 64);
            }

            if (string.IsNullOrEmpty(signatureHash))
            {
                throw new Exception("Unable to extract signatureHash");
            }

            var returnTuple = Tuple.Create(timeStamp, signatureHash);
            return returnTuple;
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
        public string Compute_Signature(string timeStamp, string payload, string secret)
        {
            string unhashed_string = $"{timeStamp}.{payload}";
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var payloadBytes = Encoding.UTF8.GetBytes(unhashed_string);
            using (var hmSha256 = new HMACSHA256(secretBytes))
            {
                var hash = hmSha256.ComputeHash(payloadBytes);
                return $"{this.ToHexString(hash)}";
            }
        }

        public string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }

        /// <summary>
        /// A constant time equals comparison.
        /// </summary>
        /// <param name="left">first array.</param>
        /// <param name="right">second array.</param>
        /// <returns>true if arrays equal, false otherwise.</returns>
        public bool ConstantTimeAreEqual(byte[] left, byte[] right)
        {
            return System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(left, right);
        }
    }
}