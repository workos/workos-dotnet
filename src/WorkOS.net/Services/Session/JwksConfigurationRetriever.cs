// @oagen-ignore-file
namespace WorkOS
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.IdentityModel.Protocols;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.IdentityModel.Tokens;

    /// <summary>
    /// Retrieves an <see cref="OpenIdConnectConfiguration"/> from a raw JWKS
    /// endpoint (one that returns <c>{ "keys": [...] }</c> directly) rather than
    /// from an OIDC discovery document.
    /// </summary>
    public sealed class JwksConfigurationRetriever : IConfigurationRetriever<OpenIdConnectConfiguration>
    {
        /// <inheritdoc/>
        public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(
            string address,
            IDocumentRetriever retriever,
            CancellationToken cancel)
        {
            var jwksJson = await retriever.GetDocumentAsync(address, cancel).ConfigureAwait(false);
            var jwks = new JsonWebKeySet(jwksJson);
            var config = new OpenIdConnectConfiguration();
            foreach (var key in jwks.GetSigningKeys())
            {
                config.SigningKeys.Add(key);
            }

            return config;
        }
    }
}
