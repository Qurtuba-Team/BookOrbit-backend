# BookOrbit Backend

A production-grade ASP.NET Core 9 backend for a university book-sharing platform, built on **Clean Architecture**, **Domain-Driven Design (DDD)**, and **CQRS** with a full observability stack (OpenTelemetry + Serilog + Jaeger + Prometheus + Grafana).

Students lend and borrow physical books through a points-based economy. The system manages the complete lifecycle: book cataloging → copy registration → lending listings → borrowing requests → transactions → reviews → point settlement. It also includes real-time student chat, in-app notifications, and OTP-based delivery/return confirmations.

---

## Table of Contents

- [System Architecture](#system-architecture)
- [Engineering Strengths](#engineering-strengths)
- [Project Structure](#project-structure)
- [Setup & Running](#setup--running)
- [Runtime Details](#runtime-details)
- [Testing](#testing)
- [Developer Guide](#developer-guide)

---

## System Architecture

### Clean Architecture Implementation

The solution enforces strict dependency inversion through four .NET projects, each representing a concentric ring:

```
┌──────────────────────────────────────────────────┐
│                 BookOrbit.Api                    │  ← Presentation (Controllers, Middlewares, OpenAPI)
│    references: Application, Infrastructure       │
├──────────────────────────────────────────────────┤
│             BookOrbit.Infrastructure             │  ← Infrastructure (EF Core, Identity, SMTP, Caching)
│    references: Application, Domain               │
├──────────────────────────────────────────────────┤
│             BookOrbit.Application                │  ← Application (CQRS Handlers, Behaviours, Validators)
│    references: Domain                            │
├──────────────────────────────────────────────────┤
│               BookOrbit.Domain                   │  ← Domain (Entities, Value Objects, Domain Events)
│    references: MediatR only                      │
└──────────────────────────────────────────────────┘
```

**Dependency direction is strictly inward.** The Domain project has zero framework dependencies beyond MediatR (for `INotification`). The Application layer defines interfaces (`IAppDbContext`, `IIdentityService`, `ITokenProvider`, `IEmailService`, etc.) that the Infrastructure layer implements—classic Dependency Inversion Principle.

### CQRS via MediatR

Every use case is modeled as either a **Command** or a **Query** dispatched through MediatR. Each feature folder contains up to three files:

| File | Role |
|---|---|
| `*Command.cs` / `*Query.cs` | Immutable record defining the request contract |
| `*Handler.cs` | Single-responsibility handler with injected dependencies |
| `*Validator.cs` | FluentValidation rules, executed automatically via pipeline behaviour |

**Example — `CreateBookCommand`:**

```csharp
public record CreateBookCommand(
    string Title, string ISBN, string Publisher,
    BookCategory Category, string Author,
    string CoverImageFileName) : IRequest<Result<BookDto>>;
```

The handler (`CreateBookCommandHandler`) constructs value objects via factory methods, performs uniqueness checks against `IAppDbContext`, delegates entity creation to `Book.Create(...)`, persists via EF Core, invalidates the cache tag, and returns a DTO—all without coupling to HTTP or database specifics.

### Domain-Driven Design

**Aggregate Roots** — Each domain concept is a self-contained aggregate with private constructors, static `Create()` factory methods that return `Result<T>`, and explicit state-machine transitions:

| Aggregate | Key Behaviours |
|---|---|
| `Student` | State machine (Pending → Approved → Active → Banned/UnBanned), point balance management (`DeductPoints`, `AddPoints`) |
| `Book` | Status workflow (Pending → Available / Rejected) with `CanTransitionToStatus` guard |
| `BookCopy` | Tracks condition and availability state (Available ↔ Borrowed ↔ Lost) |
| `LendingListRecord` | Listing lifecycle (Available → Reserved → Borrowed / Expired) with cost and duration constraints |
| `BorrowingRequest` | Request lifecycle (Pending → Accepted/Rejected/Cancelled/Expired) with configurable expiration |
| `BorrowingTransaction` | Full borrow-return lifecycle including overdue detection and loss reporting |
| `PointTransaction` | Directional point ledger (Add/Deduct) with reason-based categorization |
| `ChatGroup` | One-to-one conversation container between students |
| `ChatMessage` | Message thread with read-state tracking |
| `Notification` | System notification inbox with read-state tracking |
| `Otp` | Time-bound OTP for delivery/return confirmation flows |

**Value Objects** — Immutable, self-validating types built on `ValueObject<T>`:

- `PhoneNumber` — Egyptian phone number normalization and regex validation
- `ISBN`, `BookTitle`, `BookPublisher`, `BookAuthor` — Domain-specific string wrappers
- `UniversityMail`, `StudentName` — Student-specific validated strings
- `Point` — Numeric value object for the points economy
- `TelegramUserId`, `Url` — Communication channel identifiers

Each value object exposes a `Create()` factory that returns `Result<T>`, making invalid state unrepresentable.

**Domain Events** — Aggregates raise events (`StudentCreatedEvent`, `BorrowingRequestAcceptedEvent`, `BorrowingRequestCreatedEvent`) via the `Entity` base class. Events are dispatched during `SaveChangesAsync` in `AppDbContext.DispatchDomainEventsAsync()` using MediatR's `Publish`.

### Entity Hierarchy

```
Entity (Id, DomainEvents)
  └── AuditableEntity (CreatedAtUtc, CreatedBy, LastModifiedUtc, LastModifiedBy)
        └── ExpirableEntity (ExpirationDateUtc)
```

Audit fields are populated automatically by `AuditableEntityInterceptor`, an EF Core `SaveChangesInterceptor` that stamps `CreatedBy`/`LastModifiedBy` from `ICurrentUser` and timestamps from `TimeProvider`.

### Result Pattern

A custom `Result<T>` monad replaces exceptions for domain/application error flow:

```csharp
public sealed class Result<TValue> : IResult<TValue>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public TValue Value { get; }           // throws if failure
    public List<Error> Errors { get; }     // empty if success
    public Error TopError { get; }

    // Implicit conversions from TValue, Error, and List<Error>
    // Match<T>(onValue, onError) for railway-oriented programming
}
```

`Error` is a readonly record struct with `Code`, `Description`, and `ErrorKind` (Failure, Unexpected, Validation, Conflict, NotFound, Unauthorized, Forbidden). This enables the presentation layer's `ErrorToProblemMapper` to deterministically map domain errors to RFC 7807 ProblemDetails with correct HTTP status codes.

---

## Engineering Strengths

### 1. Observability Stack

The system ships with a **four-pillar observability stack**, all wired via Docker Compose:

| Pillar | Technology | Purpose |
|---|---|---|
| **Structured Logging** | Serilog → Seq | Structured logs with correlation IDs, enriched with `SpanId`/`TraceId` from OpenTelemetry |
| **Distributed Tracing** | OpenTelemetry → Jaeger (OTLP/gRPC) | End-to-end request tracing across ASP.NET Core, HttpClient, and EF Core |
| **Metrics** | OpenTelemetry → Prometheus | ASP.NET Core and HTTP client metrics scraped every 5s |
| **Dashboards** | Grafana | Visualization layer connected to both Prometheus and Jaeger |

**Implementation details:**

- `RequestLogContextMiddleware` pushes `CorrelationId` (from `HttpContext.TraceIdentifier`) into Serilog's `LogContext`, so every log emitted during a request lifecycle carries the same correlation ID.
- Serilog is enriched with `WithSpan()` to bridge log entries with OpenTelemetry trace/span IDs.
- OpenTelemetry is configured with `AlwaysOnSampler` (dev) and instruments ASP.NET Core, HttpClient, and Entity Framework Core.
- Prometheus scraping endpoint is exposed at `/metrics` via `UseOpenTelemetryPrometheusScrapingEndpoint()`.
- Console output template includes `[CorrelationId: {CorrelationId}]` for local debugging.

### 2. MediatR Pipeline Behaviours

Four cross-cutting concerns are implemented as open-generic `IPipelineBehavior<,>`, registered in order:

| Behaviour | Responsibility |
|---|---|
| `UnhandledExceptionBehaviour` | Catches and logs unhandled exceptions with request name and payload, then re-throws |
| `PerformanceBehaviour` | Logs a warning for any request exceeding 500ms, including user identity context |
| `ValidationBehaviour` | Runs `IValidator<TRequest>` (if registered) before the handler; short-circuits with `Error.Validation` on failure |
| `CachingBehaviour` | Intercepts queries implementing `ICachedQuery`; serves from `HybridCache` or populates on miss; skips caching on handler failure via `CacheFailureException` |

The `CachingBehaviour` is notable: it uses a `CacheFailureException<TResponse>` to abort caching without storing failed results—a pattern that prevents negative caching while staying compatible with HybridCache's factory API.

### 3. Caching Strategy

A multi-tier caching architecture:

- **HybridCache** — Combines local (in-memory) and remote cache with configurable TTLs (`LocalCachExpirationInSeconds`, `RemoteCachExpirationInMinutes`).
- **Output Caching** — Response-level caching with a configurable default policy (`OutputCachExpirationInSeconds`).
- **Tag-based Invalidation** — Each entity type defines cache tags (e.g., `BookCachingConstants.BookTag`). Mutations call `cache.RemoveByTagAsync(tag)` to surgically invalidate all related cached entries.
- **Deterministic Cache Keys** — `ICachedQuery` implementations compute composite keys from all query parameters (page, pageSize, searchTerm, sort, filters), ensuring distinct cache entries for each unique query shape.

### 4. Authorization Architecture

A granular, policy-based authorization system with **16 named policies** and corresponding `IAuthorizationHandler` implementations:

- **Role-based** — `AdminOnlyPolicy`, `StudentOnlyPolicy`
- **State-based** — `ActiveUserPolicy` (email confirmed), `ActiveStudentPolicy` (student in Active state)
- **Ownership-based** — `StudentOwnershipPolicy`, `StudentOwnerOfBookCopyPolicy`, `StudentOwnerOfLendingListRecordPolicy`
- **Relationship-based** — `BorrowingRequestBorrowingStudentPolicy`, `BorrowingRequestLendingStudentPolicy`, `BorrowingTransactionRelatedStudentPolicy`

Each policy composes multiple `IAuthorizationRequirement` instances (e.g., `ActiveStudentPolicy` = `RegisteredUserRequirement` + `ActiveStudentRequirement`), enabling fine-grained, declarative access control at the controller action level.

### 5. Error Handling Pipeline

Errors flow through three layers:

1. **Domain** — Factory methods return `Result<T>` with typed `Error` values (e.g., `BookErrors.TitleRequired`). State transition guards return `DomainCommonErrors.InvalidStateTransition`.
2. **Application** — `ValidationBehaviour` converts FluentValidation failures to `Error.Validation`. Application-level errors (e.g., `BookApplicationErrors.IsbnAlreadyExists`) use the same `Error` type.
3. **Presentation** — `ErrorToProblemMapper` maps `ErrorKind` → HTTP status code (Validation→400, NotFound→404, Conflict→409, Unauthorized→401). `GlobalExceptionHandler` catches unhandled exceptions and returns sanitized ProblemDetails (no stack traces leak to clients).

### 6. Rate Limiting

Three sliding-window policies protect different endpoint categories:

| Policy | Max Requests | Window | Segments | Queue |
|---|---|---|---|---|
| `NormalRateLimit` | 60 | 1 min | 6 | 10 |
| `SensitiveRateLimit` | 5 | 1 min | 2 | 0 |
| `OnceAMinuteRateLimit` | 1 | 1 min | 1 | 0 |

Sensitive endpoints (token generation, password reset) use the strictest policy. Email-sending endpoints use `OnceAMinuteRateLimit` to prevent abuse. All return `429 Too Many Requests` when exhausted.

### 7. JWT Authentication with Refresh Token Rotation

- Access tokens are short-lived (15 min default), signed with HMAC-SHA256, zero clock skew.
- Refresh tokens are cryptographically random (32 bytes, Base64), stored in the database.
- On refresh, the old refresh token is deleted before a new one is issued (**rotation**), preventing token reuse.
- `GetPrincipalFromExpiredToken` validates the expired access token's signature without checking lifetime, extracting claims for the refresh flow.

### 8. Data Masking

`MaskingService` masks PII (emails, phone numbers, Telegram IDs) for safe logging and API responses, exposing only the first and last characters.

### 9. API Versioning

URL-segment versioning (`/api/v1/...`) via `Asp.Versioning.Mvc`, with `SubstituteApiVersionInUrl` for OpenAPI doc generation.

### 10. Centralized Package Management

`Directory.Packages.props` enforces a single version for every NuGet dependency across all projects, eliminating version drift.

### 11. Database Resilience

SQL Server connection is configured with `EnableRetryOnFailure` (5 retries, 10s max delay), providing transient fault tolerance.

### 12. Real-Time Chat

One-to-one chat is backed by `ChatGroup` and `ChatMessage` aggregates with real-time updates via SignalR (`/chat-hub`) and REST endpoints for history, read receipts, and group listing.

### 13. Notification Center

Students receive in-app notifications with pagination, filtering, and read tracking via the `Notification` aggregate and `/api/v1/notifications` endpoints.

### 14. OTP Confirmation Flow

Delivery and return confirmations use time-bound OTPs sent via email, backed by the `Otp` aggregate and dedicated OTP services for borrowing requests and transactions.

---

## Project Structure

```
BookOrbit-backend/
├── Code/
│   ├── BookOrbit.Domain/              # Domain Layer
│   │   ├── Common/
│   │   │   ├── Entities/              # Entity, AuditableEntity, ExpirableEntity
│   │   │   ├── Results/               # Result<T>, Error, ErrorKind, IResult
│   │   │   └── ValueObjects/          # PhoneNumber, TelegramUserId, Url, ValueObject<T>
│   │   ├── Books/                     # Book aggregate, value objects, enums, errors
│   │   ├── BookCopies/                # BookCopy aggregate
│   │   ├── Students/                  # Student aggregate, domain events
│   │   ├── LendingListings/           # LendingListRecord aggregate
│   │   ├── BorrowingRequests/         # BorrowingRequest aggregate, domain events
│   │   ├── BorrowingTransactions/     # BorrowingTransaction, BorrowingReviews
│   │   ├── PointTransactions/         # PointTransaction aggregate
│   │   ├── ChatGroups/                # ChatGroup aggregate
│   │   ├── ChatMessages/              # ChatMessage aggregate
│   │   ├── Notifications/             # Notification aggregate
│   │   ├── Otps/                      # OTP aggregate
│   │   └── Identity/                  # RefreshToken entity
│   │
│   ├── BookOrbit.Application/         # Application Layer
│   │   ├── Common/
│   │   │   ├── Behaviours/            # Validation, Caching, Performance, Exception pipelines
│   │   │   ├── Constants/             # Policy names, caching constants per entity
│   │   │   ├── Interfaces/            # Ports: IAppDbContext, IIdentityService, IEmailService, etc.
│   │   │   ├── Models/                # PaginatedList<T>
│   │   │   ├── Errors/                # ApplicationCommonErrors
│   │   │   ├── Exceptions/            # CacheFailureException
│   │   │   └── Helpers/               # ListQueryHelper, MathHelper
│   │   ├── Features/
│   │   │   ├── Books/                 # Commands (Create, Update, Delete, StateMachine), Queries, DTOs
│   │   │   ├── BookCopies/            # CRUD + state management
│   │   │   ├── Students/              # CRUD + state machine (Approve, Activate, Ban, etc.)
│   │   │   ├── LendingListings/       # Listing management
│   │   │   ├── BorrowingRequests/     # Request lifecycle
│   │   │   ├── BorrowingTransactions/ # Transaction lifecycle (MarkAsReturned, MarkAsLost)
│   │   │   ├── BorrowingReviews/      # Post-transaction reviews
│   │   │   ├── BorrowingTransactionEvents/ # Audit events for transactions
│   │   │   ├── PointTransactions/     # Points ledger queries
│   │   │   ├── Chat/                  # Chat groups and messages
│   │   │   ├── Notifications/         # Notification inbox
│   │   │   ├── OTPs/                  # Delivery/return OTP workflows
│   │   │   └── Identity/             # Auth commands (ConfirmEmail, ResetPassword, ChangePassword)
│   │   └── DependencyInjection.cs     # MediatR + FluentValidation + Behaviours registration
│   │
│   ├── BookOrbit.Infrastructure/      # Infrastructure Layer
│   │   ├── Data/
│   │   │   ├── AppDbContext.cs         # EF Core context + domain event dispatching
│   │   │   ├── AppDbContextInitialiser.cs # Seed data + migration runner
│   │   │   ├── Configurations/        # 11 EF Core entity configurations (Fluent API)
│   │   │   └── Interceptors/          # AuditableEntityInterceptor
│   │   ├── Identity/
│   │   │   ├── IdentityService.cs     # ASP.NET Identity wrapper
│   │   │   ├── TokenProvider.cs       # JWT + Refresh token generation
│   │   │   └── Policies/             # 16+ IAuthorizationHandler implementations
│   │   ├── Services/                  # Email, Masking, Caching, Image, Route, Chat, OTP services
│   │   ├── Services/ChatServices/     # SignalR hub + real-time chat services
│   │   ├── Services/OtpServices/      # OTP generation/verification services
│   │   ├── Services/SystemNotificationServices/ # Notification service
│   │   ├── Settings/                  # Strongly-typed settings (AppSettings, CacheSettings, etc.)
│   │   ├── Common/EmailTemplates/     # HTML email templates (Confirm, Reset, BookRequested)
│   │   └── DependencyInjection.cs     # EF Core, Identity, Services, Policies registration
│   │
│   ├── BookOrbit.Api/                 # Presentation Layer
│   │   ├── Controllers/               # REST controllers per feature + base ApiController
│   │   ├── Contracts/Requests/        # Request DTOs per feature
│   │   ├── Middlewares/               # GlobalExceptionHandler, RequestLogContextMiddleware
│   │   ├── OpenApi/Transformers/      # Version + Bearer security scheme transformers
│   │   ├── Services/                  # CurrentUser, ApiDataService, RouteParameterService
│   │   ├── Common/
│   │   │   ├── Constants/             # Rate limit constants, cache durations
│   │   │   ├── Helpers/               # ImageHelper, RateLimitHelper
│   │   │   └── Mappers/              # ErrorToProblemMapper
│   │   ├── Program.cs                 # Composition root
│   │   └── DependencyInjection.cs     # Full presentation wiring (Auth, CORS, OTel, Caching, etc.)
│   │
│   ├── docker-compose.yml             # 6-service composition
│   ├── Dockerfile                     # Multi-stage .NET 9 build
│   ├── prometheus.yml                 # Prometheus scrape config
│   └── .env                           # Environment variables template
│
├── Tests/
│   ├── BookOrbit.Domain.UnitTests/          # Unit tests for all aggregates and value objects
│   ├── BookOrbit.Application.UnitTests/     # Mapper unit tests
│   └── BookOrbit.Application.SubcutaneousTests/ # Integration tests across all features
│
├── Docs/
│   ├── Diagrams/                      # Architecture/flow diagrams
│   └── PRD (Arabic + English)         # Product Requirements Documents
│
└── BookOrbit.Api.sln                  # Solution file
```

---

## Setup & Running

### Prerequisites

| Tool | Version |
|---|---|
| Docker & Docker Compose | Latest |
| .NET SDK | 9.0+ (local development only) |
| SQL Server | 2022 (provided via Docker) |

### Clone

```bash
git clone https://github.com/Qurtuba-Team/BookOrbit-backend.git
cd BookOrbit-backend
```

### Environment Variables

The `Code/.env` file contains all required secrets. Create or update it before running:

### File Template
```
JWT_KEY=super-secret-key-that-is-at-least-32-chars-long
EMAIL=youremail@gmail.com
EMAIL_PASSWORD=super-secret-password

SA_PASSWORD=VeryStrongPass123!

SEQ_ADMIN_PASSWORD=AnotherStrongPass
GRAFANA_ADMIN_PASSWORD=StrongGrafanaPass

DB_CONNECTION=Server=sqlserver,1433;Database=BookOrbitDb;User=sa;Password=VeryStrongPass123!;TrustServerCertificate=True;MultipleActiveResultSets=True
```

| Variable | Purpose |
|---|---|
| `JWT_KEY` | HMAC-SHA256 signing key for JWT access tokens (64-char hex recommended) |
| `EMAIL` | SMTP sender email address (Gmail) |
| `EMAIL_PASSWORD` | Gmail App Password for SMTP authentication |
| `SA_PASSWORD` | SQL Server SA password (must meet SQL Server complexity requirements) |
| `SEQ_ADMIN_PASSWORD` | Admin password for Seq log viewer UI |
| `GRAFANA_ADMIN_PASSWORD` | Admin password for Grafana dashboard UI |
| `DB_CONNECTION` | Full SQL Server connection string used by the API container |

### Run with Docker (Recommended)

```bash
cd Code
docker compose up --build -d
```

This starts **6 services**: API, SQL Server, Seq, Prometheus, Grafana, and Jaeger.

On first startup, the API waits for SQL Server's health check (up to 12 retries) before applying migrations and seeding data automatically via `AppDbContextInitialiser`.

### Run Locally (Without Docker)

1. Ensure a local SQL Server instance is available.
2. Update `ConnectionStrings:DefaultConnection` in `Code/BookOrbit.Api/appsettings.json`.
3. Set JWT and Email settings in `appsettings.json` or via environment variables.

```bash
cd Code/BookOrbit.Api
dotnet run
```

> **Note:** Local runs will not have Seq, Jaeger, Prometheus, or Grafana unless started separately.

---

## Runtime Details

### Service URLs (Docker)

| Service | URL | Purpose |
|---|---|---|
| **BookOrbit API** | `http://localhost:7240` | REST API |
| **Seq** | `http://localhost:8081` | Structured log viewer |
| **Jaeger** | `http://localhost:16686` | Distributed tracing UI |
| **Prometheus** | `http://localhost:9090` | Metrics query UI |
| **Grafana** | `http://localhost:3000` | Dashboards (metrics + traces) |
| **SQL Server** | `localhost,1433` | Database (internal) |

### API Endpoints

| Base Path | Description |
|---|---|
| `/api/v1/identity` | Authentication, token refresh, email confirmation, password management |
| `/api/v1/students` | Student CRUD + state machine operations |
| `/api/v1/books` | Book catalog management + approval workflow |
| `/api/v1/book-copies` | Physical copy registration and state tracking |
| `/api/v1/lending-list` | Lending listing lifecycle |
| `/api/v1/borrowing-requests` | Borrow request creation and response |
| `/api/v1/borrowing-transactions` | Active transaction tracking |
| `/api/v1/borrowing-reviews` | Post-return reviews |
| `/api/v1/point-transactions` | Points ledger |
| `/api/v1/chat` | One-to-one student chat (groups, history, read receipts) |
| `/api/v1/notifications` | In-app notification inbox |
| `/api/v1/images` | Image serving (book covers, student photos) |
| `/health` | Health check endpoint |
| `/metrics` | Prometheus scraping endpoint |
| `/openapi/v1.json` | OpenAPI 3.x specification (Development only) |

### Real-Time Hub

SignalR hub endpoint for chat: `http://localhost:7240/chat-hub`

### Default Credentials (Docker)

| Service | Username | Password |
|---|---|---|
| SQL Server | `sa` | Value of `SA_PASSWORD` in `.env` |
| Seq | `admin` | Value of `SEQ_ADMIN_PASSWORD` in `.env` |
| Grafana | `admin` | Value of `GRAFANA_ADMIN_PASSWORD` in `.env` |

---

## Testing

The solution includes three test projects:

| Project | Scope | Coverage |
|---|---|---|
| `BookOrbit.Domain.UnitTests` | All aggregates, value objects, state machines, entities | Domain invariants, state transitions, factory validation |
| `BookOrbit.Application.UnitTests` | DTO mappers | Mapping correctness |
| `BookOrbit.Application.SubcutaneousTests` | Full feature integration (Books, Students, Identity, BookCopies, BorrowingRequests, Transactions, Reviews, Events, PointTransactions, LendingListings) | End-to-end through MediatR pipeline |

```bash
dotnet test BookOrbit.Api.sln
```

---

## Developer Guide

### Adding a New Feature

Follow this pattern, demonstrated with a hypothetical "Wishlist" feature:

**1. Domain Layer** — Create the aggregate:

```
BookOrbit.Domain/Wishlists/
├── Wishlist.cs              # Entity with private ctor, static Create(), state methods
├── WishlistErrors.cs        # Static error constants
└── Enums/
    └── WishlistState.cs     # State enum
```

- Inherit from `AuditableEntity` (or `ExpirableEntity` if time-bound).
- Use private constructors + `static Result<Wishlist> Create(...)`.
- Validate all inputs in the factory; return `Error` on failure.
- Define state transitions with `CanTransitionToState()` guard.

**2. Application Layer** — Create the feature folder:

```
BookOrbit.Application/Features/Wishlists/
├── Dtos/
│   └── WishlistDto.cs                       # DTO with static FromEntity() mapper
├── Commands/
│   └── CreateWishlist/
│       ├── CreateWishlistCommand.cs          # record : IRequest<Result<WishlistDto>>
│       ├── CreateWishlistCommandHandler.cs   # IRequestHandler implementation
│       └── CreateWishlistCommandValidator.cs # AbstractValidator<CreateWishlistCommand>
├── Queries/
│   └── GetWishlists/
│       ├── GetWishlistsQuery.cs             # record : ICachedQuery<Result<PaginatedList<WishlistDto>>>
│       └── GetWishlistsQueryHandler.cs
└── WishlistApplicationErrors.cs
```

- Add `DbSet<Wishlist>` to `IAppDbContext` and `AppDbContext`.
- Add an EF Core configuration in `Infrastructure/Data/Configurations/`.
- Add caching constants in `Application/Common/Constants/`.
- For cached queries, implement `ICachedQuery` with `CacheKey`, `Tags`, and `Expiration`.
- Invalidate cache tags in command handlers: `await cache.RemoveByTagAsync(tag, ct)`.

**3. Infrastructure Layer** — Register any new services in `DependencyInjection.cs`.

**4. Presentation Layer:**

```
BookOrbit.Api/Controllers/Wishlists/
└── WishlistController.cs     # Inherits ApiController, injects ISender
```

- Use `result.Match(Ok, e => Problem(e, HttpContext))` for consistent response mapping.
- Apply `[Authorize(Policy = PoliciesNames.XxxPolicy)]` for authorization.
- Apply `[EnableRateLimiting(ApiConstants.XxxPolicyName)]` for rate limiting.
- Add request contracts in `Contracts/Requests/Wishlists/`.

### Key Conventions to Follow

1. **Never throw exceptions for business logic** — Return `Result<T>` with typed `Error` values.
2. **All entity creation goes through factory methods** — Constructors are private.
3. **State changes go through named methods** — `MarkAsApproved()`, not raw property setters.
4. **Validators run in the pipeline** — Don't validate in handlers; let `ValidationBehaviour` handle it.
5. **Cache invalidation is tag-based** — Every mutation must invalidate its entity's cache tag.
6. **Logging is structured** — Use `LogWarning`/`LogInformation` with named placeholders, never string interpolation.
7. **DTOs use `FromEntity()` static mappers** — No AutoMapper; explicit, testable mapping.

---

## License

MIT License — Copyright (c) 2026 Eyad-Dawood
