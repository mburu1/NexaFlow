# NexaFlow

[![CI](https://github.com/mburu1/NexaFlow/actions/workflows/ci.yml/badge.svg)](https://github.com/mburu1/NexaFlow/actions/workflows/ci.yml)
[![CD](https://github.com/mburu1/NexaFlow/actions/workflows/cd.yml/badge.svg)](https://github.com/mburu1/NexaFlow/actions/workflows/cd.yml)

> Enterprise multi-tenant task & workflow orchestration platform.

## 1. Project overview

NexaFlow is a multi-tenant SaaS platform where organizations create, assign, track, and
automate business workflows — onboarding pipelines, approval chains, IT ticket escalation,
project sprints. It exposes a REST API (ASP.NET Core) consumed by a React dashboard, backed
by SQL Server, with Redis, RabbitMQ, Kafka, and polyglot persistence provisioned for later
phases.

This repo is built in phases (see [Roadmap](#10-roadmap) below) rather than all at once —
**Phase 1 is fully implemented and tested**; later phases are honestly scaffolded, not
faked. That distinction is called out throughout this README and the ADRs.

## 2. Architecture overview

Domain-driven layering: `Domain` → `Application` → `Infrastructure`/`Messaging`/
`Notifications` → `Api`, with a `Tests` project exercising `Application`/`Infrastructure`
against an in-memory EF Core provider. Multi-tenancy is enforced by an EF Core global query
filter keyed off the caller's JWT `tenant_id` claim — see
[docs/architecture.md](docs/architecture.md) for the full diagram and
[docs/adr/003-auth-strategy.md](docs/adr/003-auth-strategy.md) for why the filter is a
no-op (not "match nothing") when there's no authenticated caller.

Further diagrams:

- [Class diagram](docs/class-diagram.md) — Domain entities and relationships
- [ERD](docs/erd.md) — the actual SQL Server schema from the EF Core migration
- [Auth flow](docs/sequence-diagrams/auth-flow.md) — register/login/refresh (implemented)
- [Task assignment](docs/sequence-diagrams/task-assignment.md) — planned Phase 2 flow
- [Kafka audit stream](docs/sequence-diagrams/kafka-audit-stream.md) — planned Phase 2 flow

## 3. Tech stack

| Layer | Choice | Why |
|---|---|---|
| API | ASP.NET Core 10, controller-based MVC | Mature, first-class OpenAPI + DI + testability |
| Auth | JWT access + refresh tokens, BCrypt hashing | Stateless access tokens, rotated refresh tokens stored as hashes only |
| Primary DB | SQL Server (EF Core, code-first migrations) | Relational integrity for tenants/users/workflows/audit — see [ADR-001](docs/adr/001-database-strategy.md) |
| Analytics DB | PostgreSQL (provisioned, unused) | Read-replica-style reporting store, Phase 2+ |
| Notifications DB | MySQL (provisioned, unused) | Polyglot persistence — notification service owns its own store, Phase 2+ |
| Audit store | MongoDB (provisioned, unused) | Schemaless activity/audit documents, Phase 2+ |
| Cache | Redis | Distributed cache registration is live; rate-limiting is in-process for now (Phase 3 moves it to Redis) |
| Messaging | RabbitMQ, Apache Kafka (provisioned, unused) | Task-assignment events / high-throughput audit stream — see [ADR-002](docs/adr/002-messaging-choice.md) |
| Email | MailKit → MailHog (dev) | Real SMTP client, not yet triggered by any workflow event |
| API docs | `Microsoft.AspNetCore.OpenApi` + Scalar | Native OpenAPI generation, modern interactive UI at `/scalar/v1` |
| Validation | FluentValidation | Explicit, testable request validation |
| Testing | xUnit.net v3, Moq, FluentAssertions, EF Core InMemory | See [Running tests](#6-running-tests) |
| Logging | Serilog (console + rolling file) | Structured logging |
| Frontend | React 19 + TypeScript + Vite | Shell scaffolded; dashboard is Phase 2 |
| Containers | Docker Compose (full stack), Kubernetes + Helm (scaffold) | Local dev today, cluster deploy is Phase 4 |
| CI/CD | GitHub Actions | Build+test on every push/PR, image build+push to GHCR on `main` |

## 4. Getting started

**Fastest path — everything in Docker:**

```bash
cp .env.example .env      # optional, defaults work out of the box
docker compose up -d
```

This brings up SQL Server, Postgres, MySQL, MongoDB, Redis, RabbitMQ, Kafka, MailHog, and
the API. The API is on `http://localhost:8080` once containers are healthy
(`docker compose ps`); apply migrations once against it (see below).

**Local .NET dev (no Docker for the API):**

```bash
cd backend/src
dotnet restore NexaFlow.slnx
dotnet ef database update --project NexaFlow.Infrastructure --startup-project NexaFlow.Infrastructure
dotnet run --project NexaFlow.Api
```

- HTTP: <http://localhost:5080>
- HTTPS: <https://localhost:7080>
- Interactive API docs (Scalar): `/scalar/v1` on either URL above
- Health check: `/health`

Requires SQL Server LocalDB (Windows) or a reachable SQL Server instance — connection
strings live in `backend/src/NexaFlow.Api/appsettings.Development.json`. Postgres/MySQL/
Mongo connection strings are also present there (matching the values given at project
kickoff) but nothing reads them yet — see [ADR-001](docs/adr/001-database-strategy.md).

**Frontend:**

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

## 5. API documentation

- Interactive OpenAPI UI (Scalar): `/scalar/v1` — e.g. <https://localhost:7080/scalar/v1>
  when running locally, or `http://localhost:8080/scalar/v1` under Docker Compose
- Postman collection: [`docs/postman/nexaflow.postman_collection.json`](docs/postman/nexaflow.postman_collection.json)
  (import it, set the `baseUrl` variable, `POST /auth/login` auto-populates `accessToken`
  for the rest of the collection via a test script)

### Endpoints (Phase 1)

| Method | Route | Auth | Notes |
|---|---|---|---|
| POST | `/auth/register` | none | Creates a tenant + Admin user |
| POST | `/auth/login` | none | Returns access + refresh tokens |
| POST | `/auth/refresh` | none | Rotates the refresh token |
| GET | `/auth/me` | any | Verifies the bearer token |
| GET/PUT | `/tenants/current` | any / Admin | View or rename the current tenant |
| GET/POST/PUT | `/users` | any / Admin,Manager | Provision teammates |
| GET/POST/PUT/DELETE | `/workflows` | any / Admin,Manager / Admin | Workflow CRUD |
| GET/POST/PUT/DELETE | `/workflows/{id}/tasks` | any / Admin,Manager | Task CRUD (any authenticated user can update a task's status) |

All non-auth routes require a `Bearer` JWT and are automatically scoped to the caller's
tenant.

## 6. Running tests

```bash
cd backend/src
dotnet test NexaFlow.Tests/NexaFlow.Tests.csproj
```

12 xUnit.net v3 tests: `AuthService` register/login/refresh-rotation behavior (against EF
Core InMemory), the tenant query-filter isolation guarantee, and `RefreshToken` domain
logic. No external services required.

```bash
cd frontend && npm run lint && npm run build
```

## 7. CI/CD pipeline

- **CI** (`.github/workflows/ci.yml`) — on every push/PR to `main`: restores, builds, and
  tests the backend; lints and builds the frontend. Badge above.
- **CD** (`.github/workflows/cd.yml`) — on push to `main`: builds the API's Docker image
  and pushes it to `ghcr.io/mburu1/nexaflow-api` using the built-in `GITHUB_TOKEN` (no
  extra secrets to configure). Badge above.

## 8. Kubernetes deployment

Manifests in [`k8s/`](k8s) and a mirroring Helm chart in [`helm/nexaflow/`](helm/nexaflow)
are **scaffolded, not yet applied to a live cluster** — that's Phase 4. They validate
client-side (`kubectl apply --dry-run=client`) but haven't been exercised against a real
cluster. See [`k8s/README.md`](k8s/README.md) for the intended apply order and how secrets
are meant to be supplied out-of-band.

## 9. Contributing guide

- **Branching**: feature branches off `main`, named `feature/<short-description>` or
  `fix/<short-description>`. PRs target `main`.
- **Commits**: small, incremental, imperative mood (`feat(auth): add refresh rotation`).
- **PRs**: use the [PR template](.github/pull_request_template.md) — describe the change,
  check off the test plan. CI must be green before merge.
- **Issues**: use the bug report / feature request templates under
  [`.github/ISSUE_TEMPLATE`](.github/ISSUE_TEMPLATE).
- **Architecture decisions**: significant choices get an ADR under
  [`docs/adr/`](docs/adr) — see the existing ones for the format.

## 10. Roadmap

**Phase 1 — Foundation — ✅ Done**
- [x] Multi-tenant JWT auth (access + rotating refresh tokens)
- [x] Role-based access control (Admin / Manager / Member)
- [x] CRUD: Tenants, Users, Workflows, WorkflowTasks
- [x] EF Core global query filter for tenant isolation
- [x] xUnit v3 test suite
- [x] Docker Compose full stack, CI + CD pipelines

**Phase 2 — Real-time & messaging — ⏳ Scaffolded, not implemented**
- [ ] RabbitMQ: task-assignment → email notification pipeline
- [ ] Kafka: audit event stream → MongoDB persistence
- [ ] SignalR live dashboard updates
- [ ] React dashboard (auth/workflows/tasks features)

**Phase 3 — Infrastructure & observability — ⏳ Partially scaffolded**
- [x] Redis distributed cache registration
- [x] Health checks endpoint (`/health`)
- [x] Structured logging (Serilog)
- [ ] Redis-backed per-tenant rate limiting (in-process limiter exists on `/auth/*` today)

**Phase 4 — DevOps — ⏳ Scaffolded, not deployed**
- [x] GitHub Actions CI/CD
- [ ] Kubernetes manifests validated against a live cluster
- [ ] Helm chart lint-tested and installed
