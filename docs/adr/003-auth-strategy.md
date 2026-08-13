# ADR-003: JWT access + rotating refresh tokens, tenant claim, permissive-when-anonymous query filter

## Status

Accepted and implemented — this is the one ADR describing shipped, tested behavior rather
than a plan.

## Context

NexaFlow needs stateless, horizontally-scalable auth (no server-side session store) with
role-based access control (Admin/Manager/Member) and strict multi-tenant data isolation,
while still allowing a single global login (`POST /auth/login` takes only email + password,
not a tenant identifier).

## Decisions

### 1. Access + refresh token pair, refresh tokens stored as hashes only

`JwtTokenService` issues a short-lived (15 min default) HMAC-SHA256-signed access token
carrying `sub`, `email`, `tenant_id`, and `role` claims, plus an opaque refresh token.
Only `SHA256(rawToken)` is persisted in `RefreshTokens.TokenHash` — never the raw value —
so a database read alone can't be replayed as a valid token. `POST /auth/refresh` rotates:
the old token is marked `RevokedAtUtc` + `ReplacedByTokenHash`, a new pair is issued, and
reusing a revoked/expired token throws `AuthenticationException` (401). This is exercised
in `NexaFlow.Tests/Application/AuthServiceTests.cs`.

### 2. Email is globally unique, not unique-per-tenant

Login takes only `{ email, password }` — no organization/tenant selector — so a user must
be locatable by email alone. `Users.Email` has a unique index at the database level (see
[ERD](../erd.md)), enforced regardless of tenant.

### 3. EF Core global query filter, permissive when there's no authenticated caller

`NexaFlowDbContext` filters every `ITenantScoped` entity to
`ICurrentUserService.TenantId`. The naive filter (`e.TenantId == currentUserService.TenantId`)
breaks decision #2: during registration's duplicate-email check and during login's
by-email lookup, there is no authenticated caller yet, so `TenantId` is `null` — and
`Guid == null` is always false, which would make the filter silently match **zero** rows
and break both flows. The filter is written as:

```csharp
e => !currentUserService.TenantId.HasValue || e.TenantId == currentUserService.TenantId
```

i.e. "no restriction when unauthenticated, strict tenant match when authenticated." This
keeps the filter as a real defense-in-depth mechanism for every authenticated request
(a bug elsewhere can't leak cross-tenant data) without breaking the two flows that
legitimately need to see across tenants before a JWT exists. Pinned down by
`NexaFlow.Tests/Infrastructure/TenantQueryFilterTests.cs`.

### 4. Role claim as a plain string, not `ClaimTypes.Role`

Tokens carry a short `role` claim (not the long `ClaimTypes.Role` URI), and
`Program.cs` sets `RoleClaimType = "role"` with `MapInboundClaims = false` on the JWT
bearer handler. This keeps the token payload smaller and the claims predictable —
`[Authorize(Roles = nameof(Role.Admin))]` works directly against the enum's `ToString()`
value with no ASP.NET Core claim-type remapping surprises.

## Consequences

- Revoking a single refresh token (e.g. suspected compromise) doesn't invalidate other
  active sessions for the same user — there's no "revoke all sessions" endpoint yet
  (Phase 2 candidate).
- Access tokens are stateless: a role change or deactivation doesn't take effect until the
  current access token expires (≤15 minutes) or the client refreshes. Acceptable for
  Phase 1; a Phase 3 candidate is a short-lived denylist in Redis for immediate revocation.
- Rate limiting on `/auth/*` (fixed-window, in-process) exists specifically because
  unauthenticated endpoints are unauthenticated by necessity — see the roadmap in the
  root README for moving this to a Redis-backed, per-tenant limiter in Phase 3.
