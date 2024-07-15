# WorkOS .NET Library

The WorkOS library for .NET provides convenient access to the WorkOS API from applications using .NET.
Supports .NET Standard 2.0+ and .NET Framework 4.6.1+

## Documentation

See the [API Reference](https://workos.com/docs/reference/client-libraries) for .NET usage examples.

## Installation

There are several options to install the WorkOS .NET SDK.

### Via the NuGet Package Manager

```sh
nuget install WorkOS.net
```

### Via the .NET Core Command Line Tools

```sh
dotnet add package WorkOS.net
```

### Via Visual Studio IDE

```sh
Install-Package WorkOS.net
```

## Configuration

To use the WorkOS client, you must provide an API key from the WorkOS dashboard.

```c#
WorkOS.SetApiKey("sk_key123");
```

You can also optionally provide a custom [HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient).

```c#
var client = new WorkOSClient(
    new WorkOSOptions
    {
        ApiKey = "sk_key123",
        HttpClient = ...,
    });
WorkOS.WorkOSClient = client;
```

## Development and Testing

Run all tests with the following command:

```sh
dotnet test test/WorkOSTests/WorkOSTests.csproj
```

Run tests for a specific framework with the following command:

```sh
dotnet test test/WorkOSTests/WorkOSTests.csproj -f netcoreapp3.1
```

## SDK Versioning

For our SDKs WorkOS follows a Semantic Versioning process where all releases will have a version X.Y.Z (like 1.0.0) pattern wherein Z would be a bug fix (I.e. 1.0.1), Y would be a minor release (1.1.0) and X would be a major release (2.0.0). We permit any breaking changes to only be released in major versions and strongly recommend reading changelogs before making any major version upgrades.

## Beta Releases

WorkOS has features in Beta that can be accessed via Beta releases. We would love for you to try these
and share feedback with us before these features reach general availability (GA). To install a Beta version,
please follow the [installation steps](#installation) above using the Beta release version.

> Note: there can be breaking changes between Beta versions. Therefore, we recommend pinning the package version to a
> specific version. This way you can install the same version each time without breaking changes unless you are
> intentionally looking for the latest Beta version.

We highly recommend keeping an eye on when the Beta feature you are interested in goes from Beta to stable so that you
can move to using the stable version.

## More Information

- [Single Sign-On Guide](https://workos.com/docs/sso/guide)
- [Directory Sync Guide](https://workos.com/docs/directory-sync/guide)
- [Admin Portal Guide](https://workos.com/docs/admin-portal/guide)
- [Magic Link Guide](https://workos.com/docs/magic-link/guide)
