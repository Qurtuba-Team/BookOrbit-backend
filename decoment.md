# BookOrbit-backend: Comprehensive Documentation & Recreation Blueprint

This document is the ultimate guide to understanding, maintaining, and completely recreating the **BookOrbit-backend** project from scratch. It is designed so that even a developer who knows nothing about this specific project can understand its every component and rebuild it step-by-step.

---

## 1. Project Overview & Architecture
**BookOrbit** is a Book Sharing application backend. It is built using **.NET 9.0 (ASP.NET Core)** and strictly follows the **Clean Architecture** pattern to separate concerns, making it scalable and easy to test.

### The Clean Architecture Layers:
1. **BookOrbit.Domain**: The core heart of the project. Contains Entities (e.g., users, books), Enums, Value Objects, and core interfaces. *Zero external dependencies.*
2. **BookOrbit.Application**: Contains Business Logic. Uses **CQRS** (Command Query Responsibility Segregation) via the MediatR library. It defines the Use Cases (Features), Contracts (Interfaces), and Validation using FluentValidation.
3. **BookOrbit.Infrastructure**: Connects to the outside world. Implements Database access via **Entity Framework Core**, external API calls, Email services (SMTP), and Identity configuration.
4. **BookOrbit.Api**: The Presentation Layer. Contains Controllers, Middlewares, OpenAPI (Swagger) setups, and the `Program.cs` startup configurations.

### 1.1 Complete Project Structure Tree

```text
BookOrbit-backend/
├── Code/
│   ├── BookOrbit.Api/                     # Web API Layer
│   │   ├── Common/
│   │   │   ├── Constants/
│   │   │   ├── Helpers/
│   │   │   └── Mappers/
│   │   ├── Contracts/
│   │   │   └── Requests/
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── OpenApi/
│   │   ├── Properties/
│   │   └── Services/
│   ├── BookOrbit.Application/             # Business Logic Layer
│   │   ├── Common/
│   │   │   ├── Behaviours/
│   │   │   ├── Constants/
│   │   │   ├── Errors/
│   │   │   ├── Exceptions/
│   │   │   ├── Interfaces/
│   │   │   └── Models/
│   │   └── Features/
│   │       ├── BookCopies/
│   │       ├── Books/
│   │       ├── Identity/
│   │       ├── LendingListings/
│   │       └── Students/
│   ├── BookOrbit.Domain/                  # Core Layer
│   │   ├── BookCopies/
│   │   ├── Books/
│   │   ├── BorrowingRequests/
│   │   ├── BorrowingTransactions/
│   │   ├── Common/
│   │   ├── Identity/
│   │   ├── LendingListings/
│   │   ├── PointTransactions/
│   │   └── Students/
│   ├── BookOrbit.Infrastructure/          # External Connections Layer
│   │   ├── Data/
│   │   │   ├── Configurations/
│   │   │   ├── Interceptors/
│   │   │   └── Migrations/
│   │   ├── Identity/
│   │   ├── Services/
│   │   └── Settings/
│   ├── docker-compose.yml
│   ├── Dockerfile
│   └── Directory.Packages.props
├── Docs/
└── Tests/
```

---

## 2. Step-by-Step Project Recreation Guide

If you need to build this project from absolutely nothing, run the following commands and follow these configurations.

### Step 2.1: Creating the Solution and Projects
Open your terminal and run the following `.NET CLI` commands:

```bash
# 1. Create the Solution
dotnet new sln -n BookOrbit.Api

# 2. Create the Clean Architecture layers
dotnet new classlib -n BookOrbit.Domain
dotnet new classlib -n BookOrbit.Application
dotnet new classlib -n BookOrbit.Infrastructure
dotnet new webapi -n BookOrbit.Api
dotnet new xunit -n BookOrbit.Domain.UnitTests

# 3. Add projects to the solution
dotnet sln add BookOrbit.Domain/BookOrbit.Domain.csproj
dotnet sln add BookOrbit.Application/BookOrbit.Application.csproj
dotnet sln add BookOrbit.Infrastructure/BookOrbit.Infrastructure.csproj
dotnet sln add BookOrbit.Api/BookOrbit.Api.csproj
dotnet sln add BookOrbit.Domain.UnitTests/BookOrbit.Domain.UnitTests.csproj

# 4. Set Dependencies between layers
# Api depends on Application and Infrastructure
dotnet add BookOrbit.Api/BookOrbit.Api.csproj reference BookOrbit.Application/BookOrbit.Application.csproj
dotnet add BookOrbit.Api/BookOrbit.Api.csproj reference BookOrbit.Infrastructure/BookOrbit.Infrastructure.csproj
# Infrastructure depends on Application and Domain
dotnet add BookOrbit.Infrastructure/BookOrbit.Infrastructure.csproj reference BookOrbit.Application/BookOrbit.Application.csproj
dotnet add BookOrbit.Infrastructure/BookOrbit.Infrastructure.csproj reference BookOrbit.Domain/BookOrbit.Domain.csproj
# Application depends on Domain
dotnet add BookOrbit.Application/BookOrbit.Application.csproj reference BookOrbit.Domain/BookOrbit.Domain.csproj
```

### Step 2.2: Centralized Package Management
Instead of adding Nuget packages per-project, this project uses a `Directory.Packages.props` file at the root for Centralized Package Management (CPM). This ensures all projects use the exact same versions.

**Create `Directory.Packages.props` in the root folder:**
```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <!-- ASP.NET Core & Versioning -->
    <PackageVersion Include="Asp.Versioning.Mvc" Version="8.1.1" />
    <PackageVersion Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.1" />
    <PackageVersion Include="Microsoft.AspNetCore.OpenApi" Version="9.0.14" />
    
    <!-- Entity Framework Core & Identity -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="9.0.14" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.14" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.14" />
    <PackageVersion Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.14" />
    <PackageVersion Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.14" />
    
    <!-- CQRS & Validation -->
    <PackageVersion Include="MediatR" Version="14.1.0" />
    <PackageVersion Include="FluentValidation" Version="12.1.1" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="12.1.1" />
    
    <!-- Caching -->
    <PackageVersion Include="Microsoft.Extensions.Caching.Memory" Version="10.0.5" />
    <PackageVersion Include="Microsoft.Extensions.Caching.Hybrid" Version="10.4.0" />
    
    <!-- Logging & Observability -->
    <PackageVersion Include="Serilog.AspNetCore" Version="10.0.0" />
    <PackageVersion Include="Serilog.Sinks.Seq" Version="9.0.0" />
    <PackageVersion Include="Serilog.Enrichers.Span" Version="3.1.0" />
    <PackageVersion Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.12.0" />
    <PackageVersion Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.12.0-beta.1" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.12.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.Http" Version="1.12.0" />
    <PackageVersion Include="OpenTelemetry.Instrumentation.EntityFrameworkCore" Version="1.10.0-beta.1" />
  </ItemGroup>
</Project>
```

---

## 3. Core Implementation Details

### 3.1 Setup Dependency Injection (DI)
In each project, we set up an extension method to register its services locally.

In `BookOrbit.Application/DependencyInjection.cs`:
```csharp
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    // Auto-Register MediatR (CQRS) and FluentValidation from the assembly
    services.AddMediatR(config => {
        config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        // Register MediatR Pipeline Behavior for Validation here
    });
    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    return services;
}
```

In `BookOrbit.Api/Program.cs` (Entry Point):
```csharp
var builder = WebApplication.CreateBuilder(args);

// Registering the layers
builder.Services
    .AddPresentation(builder.Configuration)  // API Config (Cors, OpenAPI)
    .AddApplication()                        // MediatR, Validation
    .AddInfrastructure(builder.Configuration); // DB Context, Identity

// Setup Serilog
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithSpan();
});

var app = builder.Build();

// Run Database migrations automatically on startup
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    await app.InitialiseDatabaseAsync();
}

app.UseCoreMiddlewares(builder.Configuration);
app.MapControllers();
app.Run();
```

---

## 4. Observability & Infrastructure (Docker & Tools)
BookOrbit is an enterprise-grade backend. To handle tracing, metrics, and logs, it relies on a powerful telemetry stack.

### 4.1 Required Infrastructure (Docker-Compose)
A `docker-compose.yml` spins up the entire infrastructure locally, linking the backend to the databases and observation tools using a custom internal network `bookorbit-net`.

**Services running inside the compose file:**
1. **bookorbit-api**: The .NET 9 API (Exposed on Port `7240`).
2. **sqlserver**: Microsoft SQL Server 2022 (Exposed heavily mapping Volumes for DB data mapping).
3. **seq**: The Seq dashboard for receiving and analyzing Serilog structured logs (Port `8081`).
4. **prometheus**: Scrapes metrics from the API through the OpenTelemetry framework (Port `9090`).
5. **grafana**: Dashboard visualization platform that monitors metrics retrieved by Prometheus (Port `3000`).
6. **jaeger**: Handles and displays Distributed Tracing logs via OTLP (Port `16686`).

### 4.2 `.env` Configuration File
You must create a `.env` file at the root to handle Docker Compose Secrets:
```env
JWT_KEY=super-secret-key-that-is-very-long-and-secure
EMAIL=your-system-email@gmail.com
EMAIL_PASSWORD=super-secret-app-password
SA_PASSWORD=VeryStrongPass123!
SEQ_ADMIN_PASSWORD=AnotherStrongPass
GRAFANA_ADMIN_PASSWORD=StrongGrafanaPass
DB_CONNECTION=Server=sqlserver,1433;Database=BookOrbitDb;User=sa;Password=VeryStrongPass123!;TrustServerCertificate=True;MultipleActiveResultSets=True
```

---

## 5. Security & Authentication
- **ASP.NET Core Identity**: Used to handle Users, Passwords (hashed automatically), Roles, and Claims. Managed completely within `Infrastructure`.
- **JWT (JSON Web Tokens)**: Used for stateless API authentication. Validated using `JwtBearer` middleware and securely handled via the `.env` secret key.
- **SMTP Functionality**: Implemented using basic `MailKit` or .NET's `SmtpClient` pulling Google SMTP settings to send real email verifications.

---

## 6. How To Run & Test
### 6.1 Startup
1. Ensure **Docker Desktop** is installed and running.
2. In your terminal, navigate to the `Code` folder where `docker-compose.yml` exists.
3. Run the complete stack:
   ```bash
   docker compose up --build -d
   ```
4. View the health of the container by visiting `http://localhost:7240/health`. It will return "Healthy".

### 6.2 Seeded Data Testing (Out of the Box)
When the application starts, it performs auto-migration and populates the Database with two default users:
- **Admin**: `admin@bookorbit.com` | Password: `Admin@123456`
- **Student**: `student1@std.mans.edu.eg` | Password: `sa123456`

### 6.3 Postman Testing
A complete postman collection is provided: `Tests/BookOrbit API.postman_collection.json`.
Import this file into Postman. Hit the `Login` route with the seeded data above, retrieve your Bearer Token, edit your Authorization Header globally in Postman, and proceed to test all routes natively.

---

## 7. Development Guide: Adding New Features
If you want to add a fresh "Entity" (e.g., `Review` for books), you **MUST** strictly follow this execution flow:
1. **Domain Layer**: Create `Review.cs` Entity class inheriting from your `BaseEntity`. Add your `Id`, `Text`, and relationships.
2. **Application Layer**: 
   - Create a Folder structure `Features/Reviews/Commands` and `Features/Reviews/Queries`.
   - Setup a Command class (e.g. `CreateReviewCommand : IRequest<bool>`).
   - Setup its corresponding Handler class (`CreateReviewCommandHandler : IRequestHandler<...>`).
   - Setup its Validator class (`CreateReviewValidator : AbstractValidator<CreateReviewCommand>`).
3. **Infrastructure Layer**: 
   - Add `public DbSet<Review> Reviews { get; set; }` inside `ApplicationDbContext.cs`.
   - Navigate to Api directory and run the Entity Framework CLI: `dotnet ef migrations add AddedReviewTable --project ../BookOrbit.Infrastructure`.
4. **Api Layer**: 
   - Create a `ReviewsController.cs` inherited from your base controller.
   - Inject `IMediator` in its constructor.
   - Setup `[HttpPost]` that runs `await _mediator.Send(command);`.
