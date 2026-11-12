# WorkOS.WorkloadIdentity.Gcp

Google Cloud identity token provider for workload identity federation with the [WorkOS .NET SDK](https://github.com/workos/workos-dotnet). A workload on Google Cloud authenticates to WorkOS with the identity token of its service account, so no API key is stored.

## Installation

```sh
dotnet add package WorkOS.WorkloadIdentity.Gcp
```

## Usage

```csharp
using WorkOS;
using WorkOS.WorkloadIdentity;

var credentials = new WorkloadIdentityCredentials(new WorkloadIdentityCredentialsOptions
{
    ClientId = "client_123",
    TokenUrl = "<the token endpoint for your environment>",
    EnvironmentId = "environment_123",
    IdentityTokenProvider = GcpMetadata.Create("https://api.workos.com"),
});

var client = new WorkOSClient(new WorkOSOptions { Credentials = credentials });
```

The audience passed to `GcpMetadata.Create` must match the audience of the trust rule. The provider asks the metadata server for a token for the default service account, so it works wherever that server is reachable: Compute Engine, GKE, Cloud Run, Cloud Functions and App Engine.

See "Workload identity" in the [SDK README](https://github.com/workos/workos-dotnet#workload-identity) for the settings and their environment variables.
