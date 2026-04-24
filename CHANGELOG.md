# Changelog

## [3.0.2](https://github.com/workos/workos-dotnet/compare/v3.0.1...v3.0.2) (2026-04-24)


### Bug Fixes

* Correct request param placement for POST endpoints ([#237](https://github.com/workos/workos-dotnet/issues/237)) ([c6e307a](https://github.com/workos/workos-dotnet/commit/c6e307a3db6e93a77bd29f36ad6fa34f6b9412a3))

## [3.0.1](https://github.com/workos/workos-dotnet/compare/v3.0.0...v3.0.1) (2026-04-21)


### Bug Fixes

* adjust type definitions ([f119a1d](https://github.com/workos/workos-dotnet/commit/f119a1d34a84de50e65483c92727461f3e8c65de))

## [3.0.0](https://github.com/workos/workos-dotnet/compare/v2.12.0...v3.0.0) (2026-04-20)

This is the next major release of the WorkOS .NET SDK (`v3`), rebuilt on top of our OpenAPI specifications for a more consistent and maintainable API surface.

### High-Level Changes

- **Runtime target raised to .NET 8** — `netstandard2.0` and `net461` are no longer supported
- **Global configuration renamed** — `WorkOS` → `WorkOSConfiguration`
- **`ClientId` configured once on the client** — no longer passed per-call for SSO/AuthKit/UserManagement auth helpers
- **Standardized method names** — services now expose `List`, `Create`, `Get`, `Update`, `Delete` instead of service-prefixed variants (e.g. `GetOrganization` → `Get`)
- **Option classes follow `{Service}{Action}Options` convention** — e.g. `CreateOrganizationOptions` → `OrganizationsCreateOptions`
- **Path identifiers moved to method arguments** — IDs are no longer embedded in option objects
- **Typed exceptions for API errors** — `ApiException`, `NotFoundException`, `RateLimitExceededException`, etc.
- **Automatic retries and idempotency headers** — the client runtime handles retries and adds `Idempotency-Key` to POST requests by default
- **`RequestOptions` replaces ad hoc idempotency arguments** — supports per-request API key and retry overrides
- **Service renames** — `PortalService` → `AdminPortalService`, `MfaService` → `MultiFactorAuthService`, `AuditTrailService` split into `AuditLogsService` + `EventsService`

For detailed before/after code examples and step-by-step upgrade instructions, see the [V3 Migration Guide](./docs/V3_MIGRATION_GUIDE.md).

## [2.12.0](https://github.com/workos/workos-dotnet/compare/v2.11.0...v2.12.0) (2026-03-30)


### Features

* Add Passkey authentication method and expand OAuth providers ([#227](https://github.com/workos/workos-dotnet/issues/227)) ([e7e236e](https://github.com/workos/workos-dotnet/commit/e7e236e01a12d752bb609c41c8d4f39326f19a1b))

## [2.11.0](https://github.com/workos/workos-dotnet/compare/v2.10.1...v2.11.0) (2026-03-05)


### Features

* Implement user management ([#199](https://github.com/workos/workos-dotnet/issues/199)) ([77843d0](https://github.com/workos/workos-dotnet/commit/77843d016e2e345fcbc198df66b17abb72c02665))


### Bug Fixes

* Add release-please for automated releases ([#216](https://github.com/workos/workos-dotnet/issues/216)) ([14b7298](https://github.com/workos/workos-dotnet/commit/14b7298f6fc4c4780bfff6be7cf3aa62530794fc))
* Update `Newtonsoft.Json` to fix CVE error ([#200](https://github.com/workos/workos-dotnet/issues/200)) ([d36e686](https://github.com/workos/workos-dotnet/commit/d36e6866633dd5e872b10f002506d1108dff5ba9))
* update renovate rules ([#213](https://github.com/workos/workos-dotnet/issues/213)) ([30a91d8](https://github.com/workos/workos-dotnet/commit/30a91d8b41b35ef7183cd1926a483a3a2da85971))
