# WorkOS.WorkloadIdentity.Aws

AWS identity token provider for workload identity federation with the [WorkOS .NET SDK](https://github.com/workos/workos-dotnet). A workload on AWS authenticates to WorkOS with a web identity token that AWS STS issues for its IAM principal, so no API key is stored.

## Installation

```sh
dotnet add package WorkOS.WorkloadIdentity.Aws
```

## Usage

The IAM principal needs the `sts:GetWebIdentityToken` permission.

```csharp
using WorkOS;
using WorkOS.WorkloadIdentity;

var credentials = new WorkloadIdentityCredentials(new WorkloadIdentityCredentialsOptions
{
    ClientId = "client_123",
    TokenUrl = "<the token endpoint for your environment>",
    EnvironmentId = "environment_123",
    IdentityTokenProvider = AwsSts.Create("https://api.workos.com", region: "us-east-1"),
});

var client = new WorkOSClient(new WorkOSOptions { Credentials = credentials });
```

The audience passed to `AwsSts.Create` must match the audience of the trust rule. The region is optional when the AWS SDK can resolve one (`AWS_REGION`, the shared config file, instance metadata). The provider calls STS `GetWebIdentityToken` on that region's endpoint, with the AWS SDK's default credential chain, and requests an `RS256` token.

This is the only WorkOS package that depends on the AWS SDK (`AWSSDK.SecurityToken`).

See "Workload identity" in the [SDK README](https://github.com/workos/workos-dotnet#workload-identity) for the settings and their environment variables.
