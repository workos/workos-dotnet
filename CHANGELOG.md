# Changelog

## [5.3.0](https://github.com/workos/workos-dotnet/compare/v5.2.0...v5.3.0) (2026-06-30)


### Features

* **generated:** OrganizationMembership (batch 4a353f07) ([#271](https://github.com/workos/workos-dotnet/issues/271)) ([72020c5](https://github.com/workos/workos-dotnet/commit/72020c5eb3d314983278b5e407cc41b9e4031612))

## [5.2.0](https://github.com/workos/workos-dotnet/compare/v5.1.0...v5.2.0) (2026-06-17)

- [#268](https://github.com/workos/workos-dotnet/pull/268) feat(generated)!: regenerate from spec (12 changes)
  **⚠️ Breaking**
  - **[organization_membership](https://workos.com/docs/reference/authkit/organization-membership)**:
    - Changed response of `UserManagementOrganizationMembership.list` from `UserOrganizationMembership` to `UserOrganizationMembershipList`
  - **[pipes](https://workos.com/docs/reference/pipes)**:
    - SDK surface change: Type changed for "DataIntegrationAccessTokenResponseAccessToken.ExpiresAt" from "string?" to "DateTimeOffset?"
  - **[user_management](https://workos.com/docs/reference/authkit/user)**:
    - Changed response of `UserManagementInvitations.list` from `UserInvite` to `UserInviteList`
  - **[widgets](https://workos.com/docs/reference/widgets)**:
    - SDK surface change: Symbol "WidgetSessionTokenResponse" was removed

  **Features**
  - **[authorization](https://workos.com/docs/reference/fga)**:
    - Added model `ReplaceGroupRoleAssignmentEntry`
    - Added model `ReplaceGroupRoleAssignments`
    - Added model `DeleteGroupRoleAssignmentsByCriteria`
    - Added endpoint `POST /authorization/groups/{group_id}/role_assignments`
    - Added endpoint `PUT /authorization/groups/{group_id}/role_assignments`
    - Added endpoint `DELETE /authorization/groups/{group_id}/role_assignments`
    - Added endpoint `GET /authorization/groups/{group_id}/role_assignments/{role_assignment_id}`
    - Added endpoint `DELETE /authorization/groups/{group_id}/role_assignments/{role_assignment_id}`
  - **[client](https://workos.com/docs/reference)**:
    - Added model `ClientApiToken`
    - Added model `ClientApiTokenResponse`
    - Added service `Client`
  - **[connect](https://workos.com/docs/reference/workos-connect/standalone)**:
    - Added `auth_method` to `ConnectedAccount`
    - Added `api_key_last_4` to `ConnectedAccount`
    - Added enum `ConnectedAccountAuthMethod`
  - **[groups](https://workos.com/docs/reference/groups)**:
    - Added model `CreateGroupRoleAssignment`
    - Added model `GroupRoleAssignment`
    - Added model `GroupRoleAssignmentList`
    - Added model `GroupRoleAssignmentResource`
  - **[organization_membership](https://workos.com/docs/reference/authkit/organization-membership)**:
    - Added model `UserOrganizationMembershipList`
    - Added model `UserOrganizationMembershipListListMetadata`
  - **[pipes](https://workos.com/docs/reference/pipes)**:
    - Added model `DataIntegrationCredentials`
    - Added model `DataIntegrationConfigurationResponse`
    - Added model `DataIntegrationConfigurationListResponse`
    - Added model `ConfigureDataIntegrationBody`
    - Added `auth_methods` to `DataIntegrationsListResponseData`
    - Added `auth_method` to `DataIntegrationsListResponseDataConnectedAccount`
    - Added `api_key_last_4` to `DataIntegrationsListResponseDataConnectedAccount`
    - Added enum `DataIntegrationCredentialsCredentialsType`
    - Added enum `DataIntegrationsListResponseDataAuthMethods`
    - Added enum `DataIntegrationsListResponseDataConnectedAccountAuthMethod`
    - Added service `PipesProvider`
  - **[user_management](https://workos.com/docs/reference/authkit/user)**:
    - Added model `UserInviteList`
    - Added model `UserInviteListListMetadata`
    - Made `AuthorizationCodeSessionAuthenticateRequest.client_secret` optional
    - Made `RefreshTokenSessionAuthenticateRequest.client_secret` optional
  - **[widgets](https://workos.com/docs/reference/widgets)**:
    - Added `widgets:pipes:manage` to `WidgetSessionTokenScopes`

## [5.1.0](https://github.com/workos/workos-dotnet/compare/v5.0.0...v5.1.0) (2026-06-16)

### Bug Fixes

* **renovate:** explicitly enable minor and patch updates ([#261](https://github.com/workos/workos-dotnet/issues/261)) ([a8dd75f](https://github.com/workos/workos-dotnet/commit/a8dd75fb7b90840610bb2ea4dba40fab12e47426))
* use raw JWKS retriever instead of OIDC discovery retriever ([#267](https://github.com/workos/workos-dotnet/issues/267)) ([03c3b3d](https://github.com/workos/workos-dotnet/commit/03c3b3d248dbf5187c99d9b49828782e459efd20))

- [#263](https://github.com/workos/workos-dotnet/pull/263) feat(generated): regenerate from spec (9 changes)

**Features**

- **[api_keys](https://workos.com/docs/reference/authkit/api-keys)**:
  - Added model `ExpireApiKey`
  - Added model `ApiKeyUpdated`
  - Added model `ApiKeyUpdatedData`
  - Added model `ApiKeyUpdatedDataOwner`
  - Added model `UserApiKeyUpdatedDataOwner`
  - Added model `ApiKeyUpdatedDataPreviousAttribute`
  - Added endpoint `POST /api_keys/{id}/expire`
- **[audit_logs](https://workos.com/docs/reference/audit-logs)**:
  - Added `Snowflake` to `AuditLogConfigurationLogStreamType`
- **[connect](https://workos.com/docs/reference/workos-connect/standalone)**:
  - Added `name` to `UserObject`
- **[directory_sync](https://workos.com/docs/reference/directory-sync)**:
  - Added model `DsyncTokenCreated`
  - Added model `DsyncTokenCreatedData`
  - Added model `DsyncTokenRevoked`
  - Added model `DsyncTokenRevokedData`
- **[user_management](https://workos.com/docs/reference/authkit/user)**:
  - Added `name` to user management models
- **[webhooks](https://workos.com/docs/reference/webhooks)**:
  - Added `api_key.updated` to `CreateWebhookEndpointEvents`
  - Added `api_key.updated` to `UpdateWebhookEndpointEvents`

## [5.0.0](https://github.com/workos/workos-dotnet/compare/v4.0.1...v5.0.0) (2026-05-26)


### ⚠ BREAKING CHANGES

* **application_credentials:** Change LastUsedAt type from string to DateTimeOffset ([#259](https://github.com/workos/workos-dotnet/issues/259))
* **organization_membership:** Relocate organization membership operations to dedicated service ([#259](https://github.com/workos/workos-dotnet/issues/259))
* **radar:** Remove device_fingerprint and bot_score from Radar assessment ([#259](https://github.com/workos/workos-dotnet/issues/259))
* **user_management:** Change ScreenHint type from custom enum to RadarStandaloneAssessRequestAction ([#259](https://github.com/workos/workos-dotnet/issues/259))
* **api_keys:** Add expires_at field to API key models ([#259](https://github.com/workos/workos-dotnet/issues/259))

### Features

* **api_keys:** Add expires_at field to API key models ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))
* **application_credentials:** Change LastUsedAt type from string to DateTimeOffset ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))
* **organization_membership:** Relocate organization membership operations to dedicated service ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))
* **radar:** Remove device_fingerprint and bot_score from Radar assessment ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))
* **user_management:** Change ScreenHint type from custom enum to RadarStandaloneAssessRequestAction ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))
* **vault:** Add new Vault service for encrypted object and key management ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))
* **webhooks:** Add pipes connected account events to webhook endpoint events ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))


### Bug Fixes

* **authorization:** Update role assignment listing options with filter parameters ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))
* **sso:** Update loginHint documentation for custom SAML support ([#259](https://github.com/workos/workos-dotnet/issues/259)) ([eb608f5](https://github.com/workos/workos-dotnet/commit/eb608f55a346ec58665480cce25c2d05f8e454a9))

## [4.0.1](https://github.com/workos/workos-dotnet/compare/v4.0.0...v4.0.1) (2026-05-11)


### Bug Fixes

* **docs:** use Name FileNameFactory to avoid PathTooLongException ([a09ddde](https://github.com/workos/workos-dotnet/commit/a09ddde1d3508db689c498ae672e2adacabe96a1))
* hardening across webhooks, session, client, vault, passwordless ([#253](https://github.com/workos/workos-dotnet/issues/253)) ([6e4570c](https://github.com/workos/workos-dotnet/commit/6e4570ceecaa07ae6bd85dd21a844abcffb5d911))

## [4.0.0](https://github.com/workos/workos-dotnet/compare/v3.1.0...v4.0.0) (2026-05-06)


### ⚠ BREAKING CHANGES

* **user_management:** add user API key management and update models
* **api_keys:** refactor API key models for organization and user ownership
* **authorization:** replace RoleAssignment with UserRoleAssignment

### Features

* **api_keys:** refactor API key models for organization and user ownership ([b6f8e6e](https://github.com/workos/workos-dotnet/commit/b6f8e6e26927b161c51a3885a4e2ede833cadfe5))
* **authorization:** replace RoleAssignment with UserRoleAssignment ([b6f8e6e](https://github.com/workos/workos-dotnet/commit/b6f8e6e26927b161c51a3885a4e2ede833cadfe5))
* **docs:** add DocFX-based API reference site ([#251](https://github.com/workos/workos-dotnet/issues/251)) ([e19ff24](https://github.com/workos/workos-dotnet/commit/e19ff242a85d929107b47231fce1e5c48e2f4cd1))
* **events:** add admin_portal event actor source ([b6f8e6e](https://github.com/workos/workos-dotnet/commit/b6f8e6e26927b161c51a3885a4e2ede833cadfe5))
* **user_management:** add user API key management and update models ([b6f8e6e](https://github.com/workos/workos-dotnet/commit/b6f8e6e26927b161c51a3885a4e2ede833cadfe5))
* **vault:** add BYOK key deleted event and consolidate key provider enum ([b6f8e6e](https://github.com/workos/workos-dotnet/commit/b6f8e6e26927b161c51a3885a4e2ede833cadfe5))

## [3.1.0](https://github.com/workos/workos-dotnet/compare/v3.0.2...v3.1.0) (2026-04-28)


### Features

* **generated:** Add Groups and Waitlist APIs, improve event deserialization ([#245](https://github.com/workos/workos-dotnet/issues/245)) ([63e939d](https://github.com/workos/workos-dotnet/commit/63e939d243150812f2a94a0602e298ea1e919b31))


### Bug Fixes

* Honor EnumMember attribute in query-string serialization ([#241](https://github.com/workos/workos-dotnet/issues/241)) ([6c2789a](https://github.com/workos/workos-dotnet/commit/6c2789ae005b9668368b36523f5e30196742e911))
* Update test to match renamed DeleteResourceByExternalId method ([4ceb3de](https://github.com/workos/workos-dotnet/commit/4ceb3dea35a7886cbcdb48b9dcfb74865b79000e))

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
