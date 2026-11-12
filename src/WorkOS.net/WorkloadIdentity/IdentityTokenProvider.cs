// @oagen-ignore-file
namespace WorkOS
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Identity token providers that ship with the SDK. Providers for GitHub Actions,
    /// Google Cloud and AWS are separate packages (<c>WorkOS.WorkloadIdentity.*</c>).
    /// </summary>
    public static class IdentityTokenProvider
    {
        /// <summary>
        /// Reads the identity token from a file, for example a projected service account
        /// token. The file is read again on every exchange, so a rotated token is picked up.
        /// Surrounding whitespace is trimmed; a missing or empty file is a failure.
        /// </summary>
        /// <param name="path">The path of the file that holds the JWT.</param>
        /// <returns>A provider that reads the file on every call.</returns>
        public static IIdentityTokenProvider TokenFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A token file path is required.", nameof(path));
            }

            return new TokenFileProvider(path);
        }

        /// <summary>
        /// Reads the identity token from an environment variable. The variable is read again
        /// on every exchange; an unset or empty variable is a failure.
        /// </summary>
        /// <param name="name">The name of the environment variable that holds the JWT.</param>
        /// <returns>A provider that reads the variable on every call.</returns>
        public static IIdentityTokenProvider TokenEnv(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("An environment variable name is required.", nameof(name));
            }

            return new TokenEnvProvider(name);
        }

        /// <summary>
        /// Wraps a callback as a provider. Used for
        /// <see cref="WorkloadIdentityCredentialsOptions.IdentityTokenCallback"/>.
        /// </summary>
        /// <param name="callback">The function that returns the JWT.</param>
        /// <returns>A provider that invokes the callback on every call.</returns>
        internal static IIdentityTokenProvider FromCallback(Func<CancellationToken, Task<string>> callback)
        {
            return new CallbackProvider(callback);
        }

        private sealed class TokenFileProvider : IIdentityTokenProvider
        {
            private readonly string path;

            internal TokenFileProvider(string path) => this.path = path;

            public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
            {
                var token = (await File.ReadAllTextAsync(this.path, cancellationToken).ConfigureAwait(false)).Trim();
                if (token.Length == 0)
                {
                    throw new InvalidOperationException($"The identity token file is empty: {this.path}");
                }

                return token;
            }
        }

        private sealed class TokenEnvProvider : IIdentityTokenProvider
        {
            private readonly string name;

            internal TokenEnvProvider(string name) => this.name = name;

            public Task<string> GetTokenAsync(CancellationToken cancellationToken)
            {
                var token = Environment.GetEnvironmentVariable(this.name);
                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException($"The environment variable {this.name} is not set or is empty.");
                }

                return Task.FromResult(token);
            }
        }

        private sealed class CallbackProvider : IIdentityTokenProvider
        {
            private readonly Func<CancellationToken, Task<string>> callback;

            internal CallbackProvider(Func<CancellationToken, Task<string>> callback) => this.callback = callback;

            public Task<string> GetTokenAsync(CancellationToken cancellationToken) => this.callback(cancellationToken);
        }
    }
}
