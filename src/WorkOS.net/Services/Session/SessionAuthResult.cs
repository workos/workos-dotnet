// @oagen-ignore-file
namespace WorkOS
{
    using System.Collections.Generic;

    /// <summary>Result from authenticating a session cookie.</summary>
    public class SessionAuthResult
    {
        /// <summary>Whether the session was successfully authenticated.</summary>
        public bool Authenticated { get; set; }

        /// <summary>The unique identifier of the authenticated session.</summary>
        public string? SessionId { get; set; }

        /// <summary>The identifier of the organization the session belongs to.</summary>
        public string? OrganizationId { get; set; }

        /// <summary>The primary role assigned to the user in this session.</summary>
        public string? Role { get; set; }

        /// <summary>All roles assigned to the user in this session.</summary>
        public List<string>? Roles { get; set; }

        /// <summary>The permissions granted to the user in this session.</summary>
        public List<string>? Permissions { get; set; }

        /// <summary>The entitlements available to the user in this session.</summary>
        public List<string>? Entitlements { get; set; }

        /// <summary>Feature flags evaluated for the user in this session.</summary>
        public Dictionary<string, object>? FeatureFlags { get; set; }

        /// <summary>The raw access token from the session cookie.</summary>
        public string? AccessToken { get; set; }

        /// <summary>The reason authentication failed, when <see cref="Authenticated"/> is <c>false</c>.</summary>
        public SessionFailureReason? Reason { get; set; }
    }
}
