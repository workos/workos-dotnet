// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;

    /// <summary>Sealed session cookie management, JWT validation, and JWKS helpers (H04-H07, H13).</summary>
    public class SessionService
    {
        private const int SealVersion = 2;
        private const string VersionDelimiter = "~";
        private readonly WorkOSClient client;
        private ConfigurationManager<OpenIdConnectConfiguration>? jwksManager;

        public SessionService(WorkOSClient client)
        {
            this.client = client;
        }

        /// <summary>Get the JWKS URL for the current client.</summary>
        /// <param name="clientId">Optional client ID override.</param>
        /// <returns>The JWKS URL string.</returns>
        public virtual string GetJwksUrl(string? clientId = null)
        {
            var id = clientId ?? this.client.ClientId;
            if (string.IsNullOrEmpty(id))
            {
                throw new InvalidOperationException("Client ID is required to build JWKS URL.");
            }

            return $"{this.client.ApiBaseURL}/sso/jwks/{Uri.EscapeDataString(id)}";
        }

        /// <summary>Authenticate a sealed session cookie.</summary>
        /// <param name="sessionData">The sealed session cookie string.</param>
        /// <param name="cookiePassword">The password used to seal the session.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The authentication result.</returns>
        public virtual async Task<SessionAuthResult> Authenticate(
            string sessionData,
            string cookiePassword,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(sessionData))
            {
                return new SessionAuthResult
                {
                    Authenticated = false,
                    Reason = SessionFailureReason.NoSessionCookieProvided,
                };
            }

            Dictionary<string, object> session;
            try
            {
                session = UnsealData(sessionData, cookiePassword);
            }
            catch (Exception)
            {
                return new SessionAuthResult
                {
                    Authenticated = false,
                    Reason = SessionFailureReason.InvalidSessionCookie,
                };
            }

            if (!session.ContainsKey("access_token") || session["access_token"] == null)
            {
                return new SessionAuthResult
                {
                    Authenticated = false,
                    Reason = SessionFailureReason.InvalidSessionCookie,
                };
            }

            var accessToken = session["access_token"].ToString()!;

            try
            {
                var claims = await this.ValidateJwt(accessToken, cancellationToken);
                return new SessionAuthResult
                {
                    Authenticated = true,
                    SessionId = claims.GetValueOrDefault("sid")?.ToString(),
                    OrganizationId = claims.GetValueOrDefault("org_id")?.ToString(),
                    Role = claims.GetValueOrDefault("role")?.ToString(),
                    AccessToken = accessToken,
                };
            }
            catch (Exception)
            {
                return new SessionAuthResult
                {
                    Authenticated = false,
                    Reason = SessionFailureReason.InvalidJwt,
                };
            }
        }

        /// <summary>Seal session data from an authentication response.</summary>
        /// <param name="accessToken">The access token.</param>
        /// <param name="refreshToken">The refresh token.</param>
        /// <param name="cookiePassword">The password to seal the session with.</param>
        /// <returns>The sealed session string.</returns>
        public static string SealSessionFromAuthResponse(
            string accessToken,
            string refreshToken,
            string cookiePassword)
        {
            var data = new Dictionary<string, object>
            {
                { "access_token", accessToken },
                { "refresh_token", refreshToken },
            };
            return SealData(data, cookiePassword);
        }

        /// <summary>Seal data using AES-GCM with a password-derived key.</summary>
        /// <param name="data">The data to seal.</param>
        /// <param name="password">The password for key derivation.</param>
        /// <returns>The sealed data string.</returns>
        public static string SealData(Dictionary<string, object> data, string password)
        {
            var json = JsonConvert.SerializeObject(data);
            var plaintext = Encoding.UTF8.GetBytes(json);

            var salt = RandomNumberGenerator.GetBytes(16);
            var key = DeriveKey(password, salt);
            var nonce = RandomNumberGenerator.GetBytes(12);
            var ciphertext = new byte[plaintext.Length];
            var tag = new byte[16];

            using var aes = new AesGcm(key, 16);
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            // Format: base64(salt + nonce + tag + ciphertext) + ~VERSION
            var combined = new byte[salt.Length + nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(salt, 0, combined, 0, salt.Length);
            Buffer.BlockCopy(nonce, 0, combined, salt.Length, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, salt.Length + nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, combined, salt.Length + nonce.Length + tag.Length, ciphertext.Length);

            return Convert.ToBase64String(combined) + VersionDelimiter + SealVersion;
        }

        /// <summary>Unseal data sealed with <see cref="SealData"/>.</summary>
        /// <param name="sealedData">The sealed data string.</param>
        /// <param name="password">The password used during sealing.</param>
        /// <returns>The unsealed data dictionary.</returns>
        public static Dictionary<string, object> UnsealData(string sealedData, string password)
        {
            var parts = sealedData.Split(new[] { VersionDelimiter }, StringSplitOptions.None);
            if (parts.Length < 2)
            {
                throw new InvalidOperationException("Invalid sealed data format.");
            }

            var combined = Convert.FromBase64String(parts[0]);
            if (combined.Length < 44)
            {
                throw new InvalidOperationException("Sealed data is too short.");
            }

            var salt = new byte[16];
            var nonce = new byte[12];
            var tag = new byte[16];
            var ciphertext = new byte[combined.Length - 44];

            Buffer.BlockCopy(combined, 0, salt, 0, 16);
            Buffer.BlockCopy(combined, 16, nonce, 0, 12);
            Buffer.BlockCopy(combined, 28, tag, 0, 16);
            Buffer.BlockCopy(combined, 44, ciphertext, 0, ciphertext.Length);

            var key = DeriveKey(password, salt);
            var plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(key, 16);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            var json = Encoding.UTF8.GetString(plaintext);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json)!;
        }

        private static byte[] DeriveKey(string password, byte[] salt)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                100000,
                HashAlgorithmName.SHA256);
            return pbkdf2.GetBytes(32); // 256-bit key for AES-256
        }

        private async Task<Dictionary<string, object>> ValidateJwt(string token, CancellationToken cancellationToken)
        {
            if (this.jwksManager == null)
            {
                var jwksUrl = this.GetJwksUrl();
                this.jwksManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                    jwksUrl,
                    new OpenIdConnectConfigurationRetriever(),
                    new HttpDocumentRetriever());
            }

            var config = await this.jwksManager.GetConfigurationAsync(cancellationToken);
            var handler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                IssuerSigningKeys = config.SigningKeys,
            };

            handler.ValidateToken(token, validationParameters, out _);

            var jwt = handler.ReadJwtToken(token);
            var claims = new Dictionary<string, object>();
            foreach (var claim in jwt.Claims)
            {
                claims[claim.Type] = claim.Value;
            }

            return claims;
        }
    }
}
