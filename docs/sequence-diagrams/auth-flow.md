# Auth flow (implemented)

Register → Login → Refresh → Me, as implemented in `AuthController` / `AuthService` and
exercised by the xUnit v3 suite and a manual smoke test against SQL Server LocalDB.

```mermaid
sequenceDiagram
    actor Client
    participant Api as AuthController
    participant Svc as AuthService
    participant Db as NexaFlowDbContext (SQL Server)
    participant Jwt as JwtTokenService

    Client->>Api: POST /auth/register {org, email, password, fullName}
    Api->>Svc: RegisterAsync(request)
    Svc->>Db: check email uniqueness (unauthenticated -> filter is a no-op)
    Svc->>Db: insert Tenant + User(Role=Admin)
    Svc->>Jwt: GenerateAccessToken(user) / GenerateRefreshToken()
    Svc->>Db: insert RefreshToken (hash only)
    Svc-->>Api: AuthResponse(accessToken, refreshToken, user)
    Api-->>Client: 200 OK

    Client->>Api: POST /auth/login {email, password}
    Api->>Svc: LoginAsync(request)
    Svc->>Db: find User by email (unauthenticated -> filter is a no-op)
    Svc->>Svc: PasswordHasher.Verify(password, hash)
    alt invalid credentials
        Svc-->>Api: throws AuthenticationException
        Api-->>Client: 401 Unauthorized (ProblemDetails)
    else valid
        Svc->>Jwt: issue new access + refresh token pair
        Svc-->>Api: AuthResponse
        Api-->>Client: 200 OK
    end

    Client->>Api: POST /auth/refresh {refreshToken}
    Api->>Svc: RefreshAsync(request)
    Svc->>Jwt: HashRefreshToken(raw)
    Svc->>Db: find RefreshToken by hash, check IsActive
    alt invalid/expired/reused
        Svc-->>Api: throws AuthenticationException
        Api-->>Client: 401 Unauthorized
    else valid
        Svc->>Db: revoke old token, insert new RefreshToken
        Svc-->>Api: new AuthResponse
        Api-->>Client: 200 OK
    end

    Client->>Api: GET /auth/me (Bearer access token)
    Api->>Svc: GetCurrentUserAsync() using ICurrentUserService (JWT claims)
    Svc->>Db: load User + Tenant by claims (filter now restricts to caller's tenant)
    Svc-->>Api: UserResponse
    Api-->>Client: 200 OK
```
