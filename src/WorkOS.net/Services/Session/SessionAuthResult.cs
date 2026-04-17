// @oagen-ignore-file
namespace WorkOS
{
    using System.Collections.Generic;

    /// <summary>Success result from authenticating a session cookie.</summary>
    public class SessionAuthResult
    {
        public bool Authenticated { get; set; }

        public string? SessionId { get; set; }

        public string? OrganizationId { get; set; }

        public string? Role { get; set; }

        public List<string>? Roles { get; set; }

        public List<string>? Permissions { get; set; }

        public List<string>? Entitlements { get; set; }

        public Dictionary<string, object>? FeatureFlags { get; set; }

        public string? AccessToken { get; set; }

        public SessionFailureReason? Reason { get; set; }
    }
}
