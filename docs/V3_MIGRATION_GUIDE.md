# V3 Migration Guide

This guide covers the breaking changes in the v3 branch of `WorkOS.net` and shows how to update v2-style code to the new APIs.

## At a Glance

The largest breaking changes are:

- `WorkOS` was renamed to `WorkOSConfiguration`
- the SDK now targets `.NET 8`
- many services, methods, and option classes were renamed to match the OpenAPI-generated surface
- path identifiers now move out of option objects and into method arguments
- auth URL and AuthKit authentication helpers now read `client_id` from `WorkOSOptions.ClientId`
- ad hoc idempotency arguments were replaced by `RequestOptions`
- service calls now throw typed `ApiException` exceptions for non-2xx responses
- retry behavior and idempotency headers are now handled by the client runtime

## 1. Upgrade Runtime Requirements

V2 supported `netstandard2.0` and `net461`. V3 targets `net8.0`.

If your application is not already on .NET 8, upgrade that first before taking this SDK version.

## 2. Rename Global Configuration

The global static configuration type changed from `WorkOS` to `WorkOSConfiguration`.

### Before

```csharp
WorkOS.SetApiKey("sk_test_123");
var client = WorkOS.WorkOSClient;
```

### After

```csharp
WorkOSConfiguration.SetApiKey("sk_test_123");
var client = WorkOSConfiguration.WorkOSClient;
```

If you need `client_id`-backed helpers such as SSO/AuthKit authorization URLs or User Management authentication, `SetApiKey()` alone is no longer enough. Construct a client with both `ApiKey` and `ClientId` and assign it explicitly:

```csharp
WorkOSConfiguration.WorkOSClient = new WorkOSClient(
    new WorkOSOptions
    {
        ApiKey = "sk_test_123",
        ClientId = "client_123",
    });
```

## 3. Configure `ClientId` on the Client, Not Per Call

In v2, several auth-related option objects exposed `ClientId`, `ClientSecret`, or `GrantType`.

In v3:

- `SSO.GetAuthorizationUrl(...)` injects `client_id` from `WorkOSOptions.ClientId`
- `UserManagement.GetAuthorizationUrl(...)` injects `client_id` from `WorkOSOptions.ClientId`
- most `UserManagement.AuthenticateWith*` methods inject `grant_type`, `client_id`, and `client_secret` for you
- `UserManagement.AuthenticateWithDeviceCode(...)` injects `grant_type` and `client_id`

That means old code like this must change.

### Before

```csharp
var sso = new SSOService();

var url = sso.GetAuthorizationURL(new GetAuthorizationURLOptions
{
    ClientId = "client_123",
    RedirectURI = "https://example.com/callback",
    Organization = "org_123",
});
```

### After

```csharp
var client = new WorkOSClient(
    new WorkOSOptions
    {
        ApiKey = "sk_test_123",
        ClientId = "client_123",
    });

var url = client.SSO.GetAuthorizationUrl(new SSOGetAuthorizationUrlOptions
{
    RedirectUri = "https://example.com/callback",
    Organization = "org_123",
});
```

For AuthKit authentication:

### Before

```csharp
var userManagement = new UserManagementService();

var response = await userManagement.AuthenticateWithCode(new AuthenticateWithCodeOptions
{
    Code = code,
    ClientId = "client_123",
    ClientSecret = "sk_test_123",
});
```

### After

```csharp
var client = new WorkOSClient(
    new WorkOSOptions
    {
        ApiKey = "sk_test_123",
        ClientId = "client_123",
    });

var response = await client.UserManagement.AuthenticateWithCode(
    new AuthenticateWithCodeOptions
    {
        Code = code,
    });
```

If you previously used helper methods that were URL-builders in v2, note these signature changes as well:

| V2                                                                    | V3                                                                                                       |
| --------------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------- |
| `userManagement.GetLogoutUrl(sessionId, returnTo)`                    | `userManagement.GetLogoutUrl(new UserManagementGetLogoutUrlOptions { SessionId = ..., ReturnTo = ... })` |
| `userManagement.GetJwksUrl(clientId)`                                 | `client.Session.GetJwksUrl(clientId)`                                                                    |
| `userManagement.GetJwksUrl(clientId)` when you want the JWKS document | `client.UserManagement.GetJwks(clientId)`                                                                |
| `sso.GetProfile(new GetProfileOptions { AccessToken = token })`       | `sso.GetProfile(token)`                                                                                  |

## 4. Expect Renamed Services, Methods, and Option Classes

V3 standardizes much of the surface area around a small set of method names: `List`, `Create`, `Get`, `Update`, and `Delete`.

It also renames many option classes to include the service name. A few representative examples:

| V2                           | V3                               |
| ---------------------------- | -------------------------------- |
| `CreateOrganizationOptions`  | `OrganizationsCreateOptions`     |
| `ListOrganizationsOptions`   | `OrganizationsListOptions`       |
| `UpdateOrganizationOptions`  | `OrganizationsUpdateOptions`     |
| `CreateUserOptions`          | `UserManagementCreateOptions`    |
| `ListUsersOptions`           | `UserManagementListOptions`      |
| `UpdateUserOptions`          | `UserManagementUpdateOptions`    |
| `GetAuthorizationURLOptions` | `SSOGetAuthorizationUrlOptions`  |
| `GenerateLinkOptions`        | `AdminPortalGenerateLinkOptions` |

Representative method renames:

| V2                                | V3                             |
| --------------------------------- | ------------------------------ |
| `GetOrganization(id)`             | `Get(id)`                      |
| `ListOrganizations(options)`      | `List(options)`                |
| `CreateOrganization(options)`     | `Create(options)`              |
| `UpdateOrganization(options)`     | `Update(id, options)`          |
| `DeleteOrganization(id)`          | `Delete(id)`                   |
| `GetUser(id)`                     | `Get(id)`                      |
| `GetUserByExternalId(externalId)` | `GetByExternalId(externalId)`  |
| `ListUsers(options)`              | `List(options)`                |
| `CreateUser(options)`             | `Create(options)`              |
| `UpdateUser(options)`             | `Update(id, options)`          |
| `DeleteUser(id)`                  | `Delete(id)`                   |
| `GetAuthorizationURL(options)`    | `GetAuthorizationUrl(options)` |

### Before

```csharp
var organizations = new OrganizationsService();

var org = await organizations.UpdateOrganization(new UpdateOrganizationOptions
{
    Organization = "org_123",
    Name = "Example, Inc.",
});
```

### After

```csharp
var client = WorkOSConfiguration.WorkOSClient;

var org = await client.Organizations.Update(
    "org_123",
    new OrganizationsUpdateOptions
    {
        Name = "Example, Inc.",
    });
```

## 5. Pass Path IDs as Method Arguments

A common v2 pattern was to put path parameters inside the options object. In v3, path identifiers are passed explicitly as method arguments instead.

This affects methods like:

- `Organizations.Update(...)`
- `UserManagement.Update(...)`
- `MultiFactorAuth.ChallengeFactor(...)`
- `MultiFactorAuth.VerifyChallenge(...)`

### Before

```csharp
await userManagement.UpdateUser(new UpdateUserOptions
{
    Id = "user_123",
    FirstName = "Ada",
});
```

### After

```csharp
await client.UserManagement.Update(
    "user_123",
    new UserManagementUpdateOptions
    {
        FirstName = "Ada",
    });
```

### Before

```csharp
var challenge = await mfa.ChallengeFactor(new ChallengeFactorOptions
{
    FactorId = "factor_123",
});

var verified = await mfa.VerifyChallenge(new VerifyChallengeOptions
{
    ChallengeId = challenge.Id,
    Code = "123456",
});
```

### After

```csharp
var challenge = await client.MultiFactorAuth.ChallengeFactor(
    "factor_123",
    new MultiFactorAuthChallengeFactorOptions());

var verified = await client.MultiFactorAuth.VerifyChallenge(
    challenge.Id,
    new MultiFactorAuthVerifyChallengeOptions
    {
        Code = "123456",
    });
```

## 6. Migrate Per-Request Idempotency to `RequestOptions`

V2 commonly exposed an `idempotencyKey` argument on individual methods. V3 replaces that with a shared `RequestOptions` object.

### Before

```csharp
await organizations.CreateOrganization(
    new CreateOrganizationOptions
    {
        Name = "Example, Inc.",
    },
    idempotencyKey: "org-create-123");
```

### After

```csharp
await client.Organizations.Create(
    new OrganizationsCreateOptions
    {
        Name = "Example, Inc.",
    },
    new RequestOptions
    {
        IdempotencyKey = "org-create-123",
    });
```

`RequestOptions` also supports:

- `ApiKey` for per-request API key overrides
- `MaxRetries` for per-request retry overrides

## 7. Update Error Handling

In v2, service methods generally passed whatever response body came back into deserialization.

In v3, non-2xx responses are mapped to typed exceptions:

- `AuthenticationException`
- `NotFoundException`
- `UnprocessableEntityException`
- `RateLimitExceededException`
- `ServerException`
- `ApiException`

### Before

```csharp
var org = await organizations.GetOrganization("org_missing");
```

### After

```csharp
try
{
    var org = await client.Organizations.Get("org_missing");
}
catch (NotFoundException)
{
    // handle missing organization
}
catch (ApiException ex)
{
    // handle other API failures
    throw;
}
```

## 8. Account for Automatic Retries and Idempotency Headers

The v3 runtime now:

- retries retryable failures by default
- uses `MaxRetries` from `WorkOSOptions` or `RequestOptions`
- automatically adds an `Idempotency-Key` header to `POST` requests when one is not provided

If your v2 integration depended on one-shot request behavior, configure retries explicitly:

```csharp
var client = new WorkOSClient(
    new WorkOSOptions
    {
        ApiKey = "sk_test_123",
        MaxRetries = 0,
    });
```

## 9. Watch for Renamed Services and Return Types

Several older services were renamed or reshaped:

| V2                                  | V3                                                      |
| ----------------------------------- | ------------------------------------------------------- |
| `AuditTrailService`                 | split across `AuditLogsService` and `EventsService`     |
| `PortalService`                     | `AdminPortalService`                                    |
| `MfaService`                        | `MultiFactorAuthService`                                |
| `GetProfileAndTokenResponse`        | `SSOTokenResponse`                                      |
| `AuthenticationResponse`            | `AuthenticateResponse`                                  |
| `GenerateLinkResponse` / string URL | `PortalLinkResponse`                                    |
| `Factor`                            | `AuthenticationFactor` / `AuthenticationFactorEnrolled` |
| `Challenge`                         | `AuthenticationChallenge`                               |
| `VerifyChallengeResponse`           | `AuthenticationChallengeVerifyResponse`                 |
| `Group`                             | `DirectoryGroup`                                        |

One concrete behavior change here is Admin Portal link generation:

### Before

```csharp
var portal = new PortalService();
string url = await portal.GenerateLink(new GenerateLinkOptions
{
    Organization = "org_123",
    Intent = GenerateLinkIntent.SSO,
});
```

### After

```csharp
var response = await client.AdminPortal.GenerateLink(new AdminPortalGenerateLinkOptions
{
    Organization = "org_123",
    Intent = GenerateLinkIntent.SSO,
});

string url = response.Link;
```

If you used `AuditTrailService`, the closest replacements are:

- `AuditTrailService.CreateEvent(...)` -> `AuditLogsService.CreateEvent(...)`
- `AuditTrailService.ListEvents(...)` -> `EventsService.List(...)`

## 10. Low-Level Client Code Needs `RequestOptions`

If you build `WorkOSRequest` objects directly, migrate any custom idempotency/API-key overrides to `RequestOptions`.

### Before

```csharp
var request = new WorkOSRequest
{
    Method = HttpMethod.Post,
    Path = "/organizations",
    Options = options,
    WorkOSHeaders = new Dictionary<string, string>
    {
        ["Idempotency-Key"] = "org-create-123",
    },
};
```

### After

```csharp
var request = new WorkOSRequest
{
    Method = HttpMethod.Post,
    Path = "/organizations",
    Options = options,
    RequestOptions = new RequestOptions
    {
        IdempotencyKey = "org-create-123",
    },
};
```
