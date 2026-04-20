// @oagen-ignore-file
namespace WorkOS
{
    /// <summary>Failure reasons for session cookie authentication.</summary>
    public enum SessionFailureReason
    {
        NoSessionCookieProvided,
        InvalidSessionCookie,
        InvalidJwt,
        InvalidGrant,
        MfaEnrollment,
        SsoRequired,
    }
}
