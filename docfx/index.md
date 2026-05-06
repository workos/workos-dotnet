# WorkOS .NET SDK

The official .NET client for the [WorkOS API](https://workos.com). This documentation is automatically generated from the inline XML doc comments in the SDK source code and is published with each release.

## API Reference

Browse the **API Reference** in the navigation above for the full set of types, methods, and modules exposed by `WorkOS.net`. The reference covers every public namespace, class, interface, struct, enum, and method available in the SDK.

## Installation

The package is published to NuGet. Install it into your project with the `dotnet` CLI:

```sh
dotnet add package WorkOS.net
```

## Getting started

To begin making API requests, instantiate a `WorkOSClient` with your API key. The client exposes a property for each WorkOS service (User Management, SSO, Directory Sync, Audit Logs, Organizations, Webhooks, and more).

```csharp
using WorkOS;

var client = new WorkOSClient("sk_example_...");
var user = await client.UserManagement.GetUser("user_123");
```

## Resources

- [WorkOS Dashboard](https://dashboard.workos.com) — manage your organization, environments, and API keys.
- [WorkOS API Documentation](https://workos.com/docs) — REST API reference and integration guides.
- [GitHub Repository](https://github.com/workos/workos-dotnet) — source code, issue tracker, and release notes.
- [NuGet Package](https://www.nuget.org/packages/WorkOS.net) — published package versions.
