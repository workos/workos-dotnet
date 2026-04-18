// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography;
    using System.Text;
    using Newtonsoft.Json;

    /// <summary>The action type for AuthKit Actions.</summary>
    public enum ActionType
    {
        Authentication,
        UserRegistration,
    }

    /// <summary>The verdict for an action response.</summary>
    public enum ActionVerdict
    {
        Allow,
        Deny,
    }

    /// <summary>A signed action response.</summary>
    public class SignedActionResponse
    {
        [JsonProperty("object")]
        public string Object { get; set; } = default!;

        [JsonProperty("payload")]
        public object Payload { get; set; } = default!;

        [JsonProperty("signature")]
        public string Signature { get; set; } = default!;
    }

    /// <summary>AuthKit Actions request verification and response signing (H03).</summary>
    public class ActionsService
    {
        private const int DefaultTolerance = 30;

        /// <summary>Verify the signature header of an incoming action request.</summary>
        /// <param name="payload">The raw request body string.</param>
        /// <param name="sigHeader">The signature header value (t=...,sig=...).</param>
        /// <param name="secret">The action signing secret.</param>
        /// <param name="tolerance">Time tolerance in seconds (default 30).</param>
        public void VerifyHeader(string payload, string sigHeader, string secret, int tolerance = DefaultTolerance)
        {
            var (timestamp, signature) = ParseSignatureHeader(sigHeader);
            var timestampSeconds = long.Parse(timestamp);
            var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (Math.Abs(nowMs - timestampSeconds) > tolerance * 1000L)
            {
                throw new InvalidOperationException("Timestamp outside of the tolerance zone.");
            }

            var expectedSignature = ComputeSignature(timestamp, payload, secret);
            if (!SecureCompare(expectedSignature, signature))
            {
                throw new InvalidOperationException("Signature does not match the expected signature for payload.");
            }
        }

        /// <summary>Verify and parse an incoming action request.</summary>
        /// <param name="payload">The raw request body string.</param>
        /// <param name="sigHeader">The signature header value.</param>
        /// <param name="secret">The action signing secret.</param>
        /// <param name="tolerance">Time tolerance in seconds (default 30).</param>
        /// <returns>The parsed action context as a dictionary.</returns>
        public Dictionary<string, object> ConstructAction(string payload, string sigHeader, string secret, int tolerance = DefaultTolerance)
        {
            this.VerifyHeader(payload, sigHeader, secret, tolerance);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(payload)!;
        }

        /// <summary>Sign an action response.</summary>
        /// <param name="actionType">The action type (authentication or user_registration).</param>
        /// <param name="verdict">The verdict (Allow or Deny).</param>
        /// <param name="secret">The action signing secret.</param>
        /// <param name="errorMessage">Optional error message for Deny verdicts.</param>
        /// <returns>A signed action response with payload and signature.</returns>
        public SignedActionResponse SignResponse(ActionType actionType, ActionVerdict verdict, string secret, string? errorMessage = null)
        {
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            var objectType = actionType == ActionType.Authentication
                ? "authentication_action_response"
                : "user_registration_action_response";

            var payloadDict = new Dictionary<string, object>
            {
                { "timestamp", timestamp },
                { "verdict", verdict.ToString() },
            };
            if (errorMessage != null)
            {
                payloadDict["error_message"] = errorMessage;
            }

            var payloadJson = JsonConvert.SerializeObject(payloadDict);
            var signature = ComputeSignature(timestamp, payloadJson, secret);

            return new SignedActionResponse
            {
                Object = objectType,
                Payload = payloadDict,
                Signature = signature,
            };
        }

        private static (string Timestamp, string Signature) ParseSignatureHeader(string header)
        {
            string? timestamp = null;
            string? signature = null;

            foreach (var part in header.Split(','))
            {
                var trimmed = part.Trim();
                if (trimmed.StartsWith("t="))
                {
                    timestamp = trimmed.Substring(2);
                }
                else if (trimmed.StartsWith("sig="))
                {
                    signature = trimmed.Substring(4);
                }
            }

            if (timestamp == null || signature == null)
            {
                throw new ArgumentException("Unable to extract timestamp and signature from header.");
            }

            return (timestamp, signature);
        }

        private static string ComputeSignature(string timestamp, string payload, string secret)
        {
            var message = $"{timestamp}.{payload}";
            var secretBytes = Encoding.UTF8.GetBytes(secret);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using var hmac = new HMACSHA256(secretBytes);
            var hash = hmac.ComputeHash(messageBytes);
            return ToHexString(hash);
        }

        private static bool SecureCompare(string a, string b)
        {
            var aBytes = Encoding.ASCII.GetBytes(a);
            var bBytes = Encoding.ASCII.GetBytes(b);
            return WebhookService.ConstantTimeAreEqual(aBytes, bBytes);
        }

        private static string ToHexString(byte[] array)
        {
            var hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return hex.ToString();
        }
    }
}
