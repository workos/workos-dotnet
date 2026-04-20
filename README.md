# WorkOS .NET Library

[![Packagist Version](https://img.shields.io/nuget/v/WorkOS.net)](https://www.nuget.org/packages/WorkOS.net)
[![CI](https://github.com/workos/workos-dotnet/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/workos/workos-dotnet/actions/workflows/ci.yml)

The WorkOS library for .NET provides convenient access to the WorkOS API from applications using .NET.
Supports .NET 8+

## Documentation

See the [API Reference](https://workos.com/docs/reference/client-libraries) for .NET usage examples.

## Installation

There are several options to install the WorkOS .NET SDK.

### Via the NuGet Package Manager

```sh
nuget install WorkOS.net
```

### Via the .NET CLI

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
WorkOSConfiguration.SetApiKey("sk_key123");
```

You can also optionally provide a custom [HttpClient](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient).

```c#
var client = new WorkOSClient(
    new WorkOSOptions
    {
        ApiKey = "sk_key123",
        HttpClient = ...,
    });
WorkOSConfiguration.WorkOSClient = client;
```

### Client ID

SSO and User Management endpoints that accept a `client_id` parameter (for example,
`UserManagement.AuthenticateWithPassword` and `SSO.GetAuthorizationUrl`) require the
WorkOS Client ID. Provide it via `WorkOSOptions.ClientId`:

```c#
var client = new WorkOSClient(
    new WorkOSOptions
    {
        ApiKey = "sk_key123",
        ClientId = "client_01H...",
    });
WorkOSConfiguration.WorkOSClient = client;
```

Operations that require a Client ID throw `InvalidOperationException` if one was not
configured, instead of silently sending an empty string to the API.

## Authentication URLs

Most authentication flows start by redirecting the end-user to a WorkOS-hosted
URL that the SDK builds for you. `WorkOSClient` exposes URL builders for
AuthKit, SSO, and logout. These helpers build URLs locally and do not make a
network call.

### AuthKit

```c#
var url = WorkOSConfiguration.WorkOSClient.UserManagement.GetAuthorizationUrl(
    new UserManagementGetAuthorizationUrlOptions
    {
        RedirectUri = "https://example.com/callback",
        Provider = UserManagementAuthenticationProvider.Authkit,
        State = csrfToken,
    });

return Redirect(url);
```

If you are building a browser, mobile, CLI, or desktop app that should not
hold an API key, use `PublicWorkOSClient` instead. It accepts a `client_id`
only and exposes only public-safe URL builders:

```c#
var publicClient = new PublicWorkOSClient(
    new PublicWorkOSOptions { ClientId = "client_01H..." });

var url = publicClient.GetAuthorizationUrl(
    new AuthKitAuthorizationUrlOptions
    {
        RedirectUri = "myapp://callback",
        State = csrfToken,
    });
```

### AuthKit with PKCE

For any client that cannot safely hold an API key, use PKCE. The SDK generates
and returns the `code_verifier` alongside the URL. Store that verifier in your
app's session so you can pass it back on the token exchange step.

```c#
var result = publicClient.GetAuthorizationUrlWithPkce(
    new AuthKitAuthorizationUrlOptions
    {
        RedirectUri = "myapp://callback",
    });

HttpContext.Session.SetString("pkce_verifier", result.CodeVerifier);
return Redirect(result.Url);
```

### SSO

For integrations where you already know the connection, organization, or
provider, use the SSO builder:

```c#
var url = WorkOSConfiguration.WorkOSClient.SSO.GetAuthorizationUrl(
    new SSOGetAuthorizationUrlOptions
    {
        RedirectUri = "https://example.com/callback",
        Connection = "conn_01H...",
        State = csrfToken,
    });
```

`PublicWorkOSClient` also exposes `GetSSOAuthorizationUrl` and
`GetSSOAuthorizationUrlWithPkce` for public clients.

### Completing the login

After the user returns to your redirect URI, exchange the `code` for a user
and session:

```c#
var authResponse = await WorkOSConfiguration.WorkOSClient.UserManagement
    .AuthenticateWithCodeAsync(
        new AuthenticateWithCodeOptions
        {
            Code = Request.Query["code"],
            // If you used PKCE, include the verifier you saved earlier.
            CodeVerifier = HttpContext.Session.GetString("pkce_verifier"),
        });

var user = authResponse.User;
```

## Logout URLs

Ending a WorkOS session can also require signing the user out of the upstream
identity provider. Use `GetLogoutUrl` to build the redirect URL, then send the
browser to it:

```c#
var logoutUrl = WorkOSConfiguration.WorkOSClient.UserManagement.GetLogoutUrl(
    new UserManagementGetLogoutUrlOptions
    {
        SessionId = sessionId,
        ReturnTo = "https://example.com/",
    });

Response.Cookies.Delete("wos_session");
return Redirect(logoutUrl);
```

If you manage sessions outside AuthKit, the same helper is available on
`PublicWorkOSClient`:

```c#
var logoutUrl = publicClient.GetLogoutUrl(sessionId, returnTo: "https://example.com/");
```

## JSON serialization

The SDK ships JSON metadata for **both** [Newtonsoft.Json](https://www.newtonsoft.com/json)
(used by the runtime to talk to the WorkOS API) and
[`System.Text.Json`](https://learn.microsoft.com/dotnet/api/system.text.json)
(STJ). All generated DTOs, enums, `AnyOf<T...>` values, and webhook envelopes
work under either serializer:

```c#
// Both round-trip the same payload.
var newtonsoft = JsonConvert.DeserializeObject<Organization>(json);
var stj = System.Text.Json.JsonSerializer.Deserialize<Organization>(json);
```

Enum forward compatibility is identical on both stacks: an enum value the SDK
hasn't seen before deserializes to the type's `Unknown` member instead of
throwing.

## Services

Most services on `WorkOSClient` are generated from the WorkOS OpenAPI
specification (`Organizations`, `UserManagement`, `DirectorySync`, `SSO`,
`AuditLogs`, `Events`, `Webhooks`, etc.). In addition, the SDK ships a handful
of **hand-maintained** services that do not correspond to a single REST
endpoint:

| Service               | Purpose                                                          |
| --------------------- | ---------------------------------------------------------------- |
| `client.Passwordless` | Magic-link / passwordless session helpers.                       |
| `client.Vault`        | Key-value storage and envelope encryption helpers.               |
| `client.Actions`      | AuthKit Actions signing-secret verification and payload signing. |
| `client.Session`      | Sealed-session management, cookie helpers, and JWT validation.   |

These services are fully supported and are accessed the same way as generated
services:

```c#
var payload = client.Actions.VerifyAndParse(rawBody, signatureHeader);
var session = client.Session.LoadSealedSession(cookieValue);
```

## Making direct API requests

Every REST endpoint in the WorkOS OpenAPI spec has a typed wrapper on one of
the services under `WorkOSClient`, and you should usually prefer those.
When you need to call an endpoint the SDK does not yet expose, you can drop
down to `WorkOSClient.MakeAPIRequest<T>` or `WorkOSClient.MakeRawAPIRequest`.

Both methods take a `WorkOSRequest`:

| Property         | Purpose                                                                                                                                |
| ---------------- | -------------------------------------------------------------------------------------------------------------------------------------- |
| `Method`         | `HttpMethod.Get`, `HttpMethod.Post`, etc.                                                                                              |
| `Path`           | Absolute path starting with `/`, for example `/organizations`.                                                                         |
| `Options`        | Any `BaseOptions` subclass. The SDK serializes it as query params for `GET`/`DELETE` requests or as a JSON body for mutating requests. |
| `AccessToken`    | Optional bearer token to send instead of the API key.                                                                                  |
| `WorkOSHeaders`  | Optional WorkOS-specific headers to include on the request.                                                                            |
| `RequestOptions` | Per-call overrides such as `ApiKey`, `IdempotencyKey`, or `MaxRetries`.                                                                |

`BaseOptions` is a marker base class. For direct calls, you define your own
subclass with the fields you want to serialize.

### Typed response

```c#
public class CustomListOptions : BaseOptions
{
    public string? OrganizationId { get; set; }

    public int? Limit { get; set; }
}

var request = new WorkOSRequest
{
    Method = HttpMethod.Get,
    Path = "/some/new/endpoint",
    Options = new CustomListOptions
    {
        OrganizationId = "org_01H...",
        Limit = 50,
    },
};

var response = await WorkOSConfiguration.WorkOSClient
    .MakeAPIRequest<MyCustomResponse>(request);
```

### Raw response

If you need access to headers, status code, or a non-JSON body, use the raw
variant and handle the `HttpResponseMessage` yourself:

```c#
using var response = await WorkOSConfiguration.WorkOSClient
    .MakeRawAPIRequest(request);

response.EnsureSuccessStatusCode();
var retryAfter = response.Headers.RetryAfter;
var body = await response.Content.ReadAsStringAsync();
```

### Per-request overrides

Generated service methods and raw requests both accept `RequestOptions`:

```c#
var org = await WorkOSConfiguration.WorkOSClient.Organizations.CreateAsync(
    new OrganizationsCreateOptions { Name = "Acme" },
    new RequestOptions
    {
        IdempotencyKey = "acme-tenant-create-v1",
        MaxRetries = 0,
    });
```

The SDK automatically adds an `Idempotency-Key` header to `POST` requests when
one is not provided explicitly.

## Error handling

All non-2xx responses from the WorkOS API raise subclasses of `WorkOS.ApiException`.
The SDK maps well-known status codes to specific exception types so you can
`catch` exactly what you care about:

| HTTP status | Exception type                 |
| ----------- | ------------------------------ |
| 401         | `AuthenticationException`      |
| 404         | `NotFoundException`            |
| 422         | `UnprocessableEntityException` |
| 429         | `RateLimitExceededException`   |
| 5xx         | `ServerException`              |
| other       | `ApiException`                 |

Every exception exposes a `StatusCode` property and carries the raw response
body in `Exception.Message`.

```c#
try
{
    var org = await client.Organizations.GetOrganization("org_01H...");
}
catch (NotFoundException)
{
    // organization does not exist
}
catch (RateLimitExceededException)
{
    // back off per Retry-After and retry
}
catch (ApiException ex)
{
    Console.Error.WriteLine($"WorkOS API error {(int)ex.StatusCode}: {ex.Message}");
    throw;
}
```

### Retry behavior

The SDK automatically retries failed requests that receive a **429** (rate limit)
or **5xx** (server error) response. Retries use exponential backoff with full
jitter and honor the `Retry-After` header when present. By default the SDK
retries up to **2** times; you can change this via `WorkOSOptions.MaxRetries`
or disable retries entirely by setting it to `0`.

## Development and Testing

### Running the test suite

Run all tests with the following command:

```sh
dotnet test test/WorkOSTests/WorkOSTests.csproj
```

Run tests for a specific framework with the following command:

```sh
dotnet test test/WorkOSTests/WorkOSTests.csproj -f net8.0
```

### Mocking the SDK in your own tests

The SDK follows the [Azure SDK mocking guidelines](https://azure.github.io/azure-sdk/dotnet_introduction.html#dotnet-mocking):

- Every service class (`UserManagementService`, `SSOService`,
  `OrganizationsService`, `DirectorySyncService`, etc.) exposes a
  parameterless constructor.
- Service accessors on `WorkOSClient` are declared `virtual`, and generated
  service methods are virtual where the SDK surface permits overriding.

That lets mocking libraries such as [Moq](https://github.com/moq/moq) or
[NSubstitute](https://nsubstitute.github.io/) substitute the SDK without
requiring custom interfaces.

```c#
using Moq;
using WorkOS;

var userManagement = new Mock<UserManagementService>();
userManagement
    .Setup(s => s.GetAsync(
        It.IsAny<string>(),
        It.IsAny<RequestOptions>(),
        It.IsAny<CancellationToken>()))
    .ReturnsAsync(new User { Id = "user_123", Email = "marceline@example.com" });

var client = new Mock<WorkOSClient>(new WorkOSOptions { ApiKey = "sk_test" });
client.Setup(c => c.UserManagement).Returns(userManagement.Object);

var sut = new UserController(client.Object);
```

Prefer constructor-injecting `WorkOSClient` into your own classes rather than
reaching for `WorkOSConfiguration.WorkOSClient` directly. That keeps mocked
clients isolated to the test under test.

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
