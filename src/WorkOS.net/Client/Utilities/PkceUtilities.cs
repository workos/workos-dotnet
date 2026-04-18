// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>A PKCE code verifier and challenge pair.</summary>
    public class PkcePair
    {
        public string CodeVerifier { get; set; } = default!;

        public string CodeChallenge { get; set; } = default!;

        public string CodeChallengeMethod { get; set; } = "S256";
    }

    /// <summary>PKCE utilities for code verifier and challenge generation (RFC 7636).</summary>
    public static class PkceUtilities
    {
        /// <summary>Generate a cryptographically random code verifier.</summary>
        /// <param name="length">Length of the verifier (43-128, default 43).</param>
        /// <returns>A base64url-encoded random string.</returns>
        public static string GenerateCodeVerifier(int length = 43)
        {
            if (length < 43 || length > 128)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "Length must be between 43 and 128.");
            }

            var bytes = RandomNumberGenerator.GetBytes(length);
            return Base64UrlEncode(bytes).Substring(0, length);
        }

        /// <summary>Generate a S256 code challenge from a code verifier.</summary>
        /// <param name="verifier">The code verifier string.</param>
        /// <returns>A base64url-encoded SHA-256 hash.</returns>
        public static string GenerateCodeChallenge(string verifier)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(verifier));
            return Base64UrlEncode(hash);
        }

        /// <summary>Generate a complete PKCE pair (verifier + challenge).</summary>
        /// <returns>A <see cref="PkcePair"/> with code verifier, challenge, and method.</returns>
        public static PkcePair Generate()
        {
            var verifier = GenerateCodeVerifier();
            var challenge = GenerateCodeChallenge(verifier);
            return new PkcePair
            {
                CodeVerifier = verifier,
                CodeChallenge = challenge,
                CodeChallengeMethod = "S256",
            };
        }

        /// <summary>Generate a random state parameter.</summary>
        /// <returns>A random base64url-encoded string.</returns>
        public static string GenerateState()
        {
            return GenerateCodeVerifier(43);
        }

        internal static string Base64UrlEncode(byte[] input)
        {
            return Convert.ToBase64String(input)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        internal static byte[] Base64UrlDecode(string input)
        {
            var padded = input.Replace('-', '+').Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            return Convert.FromBase64String(padded);
        }
    }
}
