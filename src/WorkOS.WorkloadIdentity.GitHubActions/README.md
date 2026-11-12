# WorkOS.WorkloadIdentity.GitHubActions

GitHub Actions identity token provider for workload identity federation with the [WorkOS .NET SDK](https://github.com/workos/workos-dotnet). A workflow authenticates to WorkOS with the OIDC token GitHub issues for the job, so no API key is stored in the repository.

## Installation

```sh
dotnet add package WorkOS.WorkloadIdentity.GitHubActions
```

## Usage

The workflow or job needs `permissions: id-token: write`.

```csharp
using WorkOS;
using WorkOS.WorkloadIdentity;

var credentials = new WorkloadIdentityCredentials(new WorkloadIdentityCredentialsOptions
{
    ClientId = "client_123",
    TokenUrl = "<the token endpoint for your environment>",
    EnvironmentId = "environment_123",
    IdentityTokenProvider = GitHubActions.Create("https://api.workos.com"),
});

var client = new WorkOSClient(new WorkOSOptions { Credentials = credentials });
```

The audience passed to `GitHubActions.Create` must match the audience of the trust rule. The provider reads `ACTIONS_ID_TOKEN_REQUEST_URL` and `ACTIONS_ID_TOKEN_REQUEST_TOKEN`, which GitHub sets for the job.

See "Workload identity" in the [SDK README](https://github.com/workos/workos-dotnet#workload-identity) for the settings and their environment variables.
