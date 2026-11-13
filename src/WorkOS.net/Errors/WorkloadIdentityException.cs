// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Thrown when workload identity federation cannot produce an access token: the
    /// credentials are misconfigured, the identity token provider failed, or the token
    /// endpoint rejected the exchange or kept failing.
    /// Neither the identity token nor the access token ever appears in the message.
    /// </summary>
    public class WorkloadIdentityException : Exception
    {
        /// <summary>The token endpoint could not be reached, kept failing, or answered with a redirect.</summary>
        internal const string ExchangeFailed = "exchange_failed";

        /// <summary>The token endpoint answered with something the SDK could not read.</summary>
        internal const string InvalidResponse = "invalid_response";

        /// <summary>The identity token provider failed on every attempt.</summary>
        internal const string ProviderFailed = "provider_failed";

        /// <summary>The credentials are missing a required setting.</summary>
        internal const string InvalidConfiguration = "invalid_configuration";

        private const string UnknownOAuthCodeHint = "The token endpoint rejected the exchange.";

        // The one table of hints. The wording is pending the server team's final error
        // codes and descriptions, so every hint lives here to keep that a one-place change.
        private static readonly Dictionary<string, string> Hints = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["invalid_grant"] = "The identity token was not accepted. Check its issuer, audience and subject claims against the trust rule, and that it has not expired. A token that was already exchanged is also refused: each exchange needs a new identity token.",
            ["invalid_target"] = "The resource is not one the token endpoint accepts. Leave the resource unset to use the default, https://api.workos.com.",
            ["invalid_client"] = "The client ID is unknown, or the application has no enabled trust rule. Check that the client ID is the M2M application that has the trust rule.",
            ["unauthorized_client"] = "This application cannot use workload identity federation. Check that the feature is enabled for your team and that the application is bound to a non-production environment.",
            ["invalid_scope"] = "The requested scope is not allowed for this application. Fix the scope or leave it unset.",
            ["invalid_request"] = "The token request was malformed. Check that the token URL points at the /oauth2/token endpoint.",
            [ExchangeFailed] = "The token endpoint could not be reached or kept failing. Check the token URL and try again.",
            [InvalidResponse] = "The token endpoint returned a response the SDK could not read. Check that the token URL points at the /oauth2/token endpoint.",
            [ProviderFailed] = "The identity token provider failed, so there was no token to exchange.",
            [InvalidConfiguration] = "Workload identity credentials are missing a required setting.",
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkloadIdentityException"/> class.
        /// </summary>
        /// <param name="code">The OAuth error code from the token endpoint, or one of the SDK's own codes.</param>
        /// <param name="description">Detail about the failure; kept to 500 characters.</param>
        /// <param name="innerException">The error that caused this one, if any.</param>
        public WorkloadIdentityException(string code, string? description = null, Exception? innerException = null)
            : base(BuildMessage(code, description), innerException)
        {
            this.Code = code;
            this.Description = Truncate(description);
            this.Hint = HintFor(code);
        }

        /// <summary>
        /// The OAuth error code from the token endpoint (for example <c>invalid_grant</c>), or one
        /// of the SDK's own codes: <c>exchange_failed</c>, <c>invalid_response</c>,
        /// <c>provider_failed</c> or <c>invalid_configuration</c>.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// The token endpoint's <c>error_description</c>, or the SDK's detail about the failure.
        /// At most 500 characters; empty when there is none.
        /// </summary>
        public string Description { get; }

        /// <summary>One sentence on what to check. Never empty.</summary>
        public string Hint { get; }

        private static string HintFor(string code)
        {
            return Hints.TryGetValue(code, out var hint) ? hint : UnknownOAuthCodeHint;
        }

        private static string Truncate(string? description)
        {
            if (string.IsNullOrEmpty(description))
            {
                return string.Empty;
            }

            return description.Length <= WorkloadIdentityConstants.DescriptionLimit
                ? description
                : description.Substring(0, WorkloadIdentityConstants.DescriptionLimit);
        }

        private static string BuildMessage(string code, string? description)
        {
            var detail = Truncate(description);
            var message = $"{code}: {HintFor(code)}";
            return detail.Length == 0 ? message : $"{message} {detail}";
        }
    }
}
