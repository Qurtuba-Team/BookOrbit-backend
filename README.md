# BookOrbit Backend

BookOrbit is a highly robust, scalable backend system built on **.NET 9**, implementing strict **Clean Architecture** principles and **Domain-Driven Design (DDD)**. It leverages **CQRS**, advanced observability, and policy-based authorization to deliver an enterprise-grade REST API.

---

## 🏛 System Architecture

The architecture enforces strict dependency flow inwards: **Presentation ➔ Infrastructure ➔ Application ➔ Domain**.

### 1. Domain Layer (`BookOrbit.Domain`)
The core of the system. It has absolutely **no dependencies** on external frameworks (other than basic .NET primitives).
*   **Rich Domain Model:** Encapsulates business rules using Entities, Value Objects, and Domain Events.
*   **Result Pattern (`Result<T>`, `Error`, `ErrorKind`):** Business failures are modeled as strongly-typed errors rather than exceptions, avoiding expensive stack trace unwinding for expected logic flows.

### 2. Application Layer (`BookOrbit.Application`)
Orchestrates business use cases using **CQRS** (Command Query Responsibility Segregation).
*   **MediatR Pipelines:** Every request goes through `IRequest<Result<T>>`.
*   **Pipeline Behaviors:** Cross-cutting concerns are handled via MediatR `IPipelineBehavior`:
    *   `ValidationBehaviour`: Uses **FluentValidation** to fail fast on invalid payloads.
    *   `LoggingBehaviour` & `PerformanceBehaviour`: Tracks slow commands.
    *   `CachingBehaviour`: Intercepts queries to return cached results.
    *   `UnhandledExceptionBehaviour`: Fallback catch for unexpected failures.

### 3. Infrastructure Layer (`BookOrbit.Infrastructure`)
Contains all external communication, data access, and security policies.
*   **EF Core 9:** Uses SqlServer with `EnableRetryOnFailure` for transient fault tolerance.
*   **Domain Event Dispatching:** Utilizes `ISaveChangesInterceptor` (`AuditableEntityInterceptor`) to automatically dispatch domain events when entities are saved.
*   **Complex Authorization:** Implements robust policy-based authorization handlers (e.g., `StudentOwnerOfBookCopyHandler`, `BorrowingTransactionLendingStudentHandler`) keeping security logic out of the controllers.

### 4. Presentation Layer (`BookOrbit.Api`)
A thin API surface area.
*   **Controllers:** Minimal logic; they simply map HTTP requests to MediatR commands and map the resulting `Result<T>` to `IResult` (e.g., 200 OK, 400 Bad Request, 404 Not Found).
*   **Global Exception Handling:** Uses `IExceptionHandler` (`GlobalExceptionHandler`) to translate unhandled exceptions into RFC 7807 `ProblemDetails`.

---

## ⚙️ Key Engineering Features

*   **Advanced Observability:** Completely instrumented using **OpenTelemetry**. Traces and metrics are exported natively via OTLP gRPC to **Jaeger** and **Prometheus**. Use Grafana for dashboards and Jaeger for distributed tracing.
*   **Structured Logging:** Uses **Serilog** configured to enrich logs with Span Context (`.Enrich.WithSpan()`) and pushed to **Seq** for centralized log visualization.
*   **Resiliency & Rate Limiting:** Implements `Microsoft.AspNetCore.RateLimiting` with sliding window policies (`Normal`, `Sensitive`, `OnceAMinute`) to prevent abuse.
*   **Multi-Tier Caching:** Combines ASP.NET `OutputCache` (up to 100MB configured) for raw HTTP responses and `HybridCache` for resilient, localized data caching within the Application layer.
*   **API Versioning:** Cleanly implemented using `Asp.Versioning.Mvc` with URL segment parsing.

---

## 🚦 Getting Started

### Prerequisites
*   [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.
*   .NET 9 SDK (If running locally without Docker)

### Environment Configuration
Create a `.env` file in the root `Code` directory based on the following template:

```env
JWT_KEY=super-secret-key-that-is-at-least-32-chars-long
EMAIL=youremail@gmail.com
EMAIL_PASSWORD=super-secret-password

SA_PASSWORD=VeryStrongPass123!

SEQ_ADMIN_PASSWORD=AnotherStrongPass
GRAFANA_ADMIN_PASSWORD=StrongGrafanaPass

DB_CONNECTION=Server=sqlserver,1433;Database=BookOrbitDb;User=sa;Password=VeryStrongPass123!;TrustServerCertificate=True;MultipleActiveResultSets=True
```

### Quick Start (Docker - Recommended)

1.  **Clone the repository**:
    ```bash
    git clone https://github.com/Qurtuba-Team/BookOrbit-backend.git
    ```
2.  **Navigate to the Code directory**:
    ```bash
    cd BookOrbit-backend/Code
    ```
3.  **Launch the complete stack** (API, DB, Observability tools):
    ```bash
    docker-compose up -d --build
    ```
4.  **Verify Health**: Visit `http://localhost:7240/health`. It should return a `Healthy` status.

### Running Locally (Without Docker)
1. Ensure your local `appsettings.Development.json` has a valid SQL Server connection string pointing to a local instance.
2. Run the Entity Framework migrations to build the schema:
   ```bash
   dotnet ef database update --project BookOrbit.Infrastructure --startup-project BookOrbit.Api
   ```
3. Run the API:
   ```bash
   cd BookOrbit.Api
   dotnet run
   ```

---

## 📊 Service Registry & Credentials

When running via Docker Compose, the following services are available on your local machine:

| Service | URL | Credentials (User / Pass) |
| :--- | :--- | :--- |
| **API (HTTP)** | `http://localhost:7240` | - |
| **Swagger UI** | `http://localhost:7240/swagger` | - |
| **Grafana (Metrics)** | `http://localhost:3000` | `admin` / From `.env` File (`GRAFANA_ADMIN_PASSWORD`) |
| **Seq (Logs)** | `http://localhost:8081` | `admin` / From `.env` File (`SEQ_ADMIN_PASSWORD`) |
| **Prometheus** | `http://localhost:9090` | - |
| **Jaeger (Traces UI)**| `http://localhost:16686` | - |
| **SQL Server** | `localhost, 1433` | `sa` / From `.env` File (`SA_PASSWORD`) |

### 🔑 Seeded Test Users

The database is seeded with initial data for testing purposes. You can authenticate using these credentials:

*   **Admin User**: 
    *   Email: `admin@bookorbit.com`
    *   Password: `Admin@123456`
*   **Student User**: 
    *   Email: `student1@std.mans.edu.eg`
    *   Password: `sa123456`

---

## 📝 Development Notes

### API Testing
A comprehensive Postman collection is provided in the repository at `Tests/BookOrbit API.postman_collection.json`. Import this into Postman to easily interact with the API endpoints.

### Managing State & Assets
*   **Database Reset**: To completely reset the environment and wipe all database data, volumes, and logs, run:
    ```bash
    docker compose down -v
    ```
*   **Default Assets**: A default student placeholder image is bundled and served from `BookOrbit.Api/uploads/Students/DefaultStudentImage.png`. The `uploads` directory is mapped as a Docker volume to persist user-uploaded images across container restarts.

### Docker Configuration Note

It is important when running the application using Docker to access the container files and open the appsettings.json file located inside the app folder.

Make sure to review and adjust the configuration settings **especially the URLs** so they match your hosting environment and deployment requirements.
