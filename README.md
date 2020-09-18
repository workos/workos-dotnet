# workos-dotnet

[![Build Status](https://workos.semaphoreci.com/badges/workos-dotnet/branches/v0.1.0.svg?style=shields&key=343c1d18-79da-4ea3-89ce-8a6195a9d3d9)](https://workos.semaphoreci.com/projects/workos-dotnet)
[![NuGet version (WorkOS.net)](https://img.shields.io/nuget/v/WorkOS.net.svg?style=flat-square)](https://www.nuget.org/packages/WorkOS.net/)

The official [WorkOS](https://www.workos.com/) .NET SDK supporting .NET Standard 2.0+ and .NET Framework 4.6.1+.

## Documentation

Complete documentation for the library can be found [here](https://workos.com/docs/).

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

## Example Usage

The example below demonstrates exchanging an authorization code for a WorkOS Profile.

```c#
var ssoService = new SSOService();
var options = new GetProfileOptions
{
    ClientId = "project_123",
    Code = "code_123",
};
var response = ssoService.GetProfile(options);
```

When applicable, calls can be made asynchronously and return a `Task`.

```c#
var response = await ssoService.GetProfileAsync(options);
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
