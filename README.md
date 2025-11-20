# E-Commerce Backend API

<div align="center">

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-GPL--3.0-blue.svg)](LICENSE.md)
[![OpenTelemetry](https://img.shields.io/badge/OpenTelemetry-Enabled-FF6F00?logo=opentelemetry)](https://opentelemetry.io/)

A production-ready, enterprise-grade e-commerce backend API built with ASP.NET Core 9, featuring clean architecture, double-entry accounting, and comprehensive observability.

[Features](#-features) • [Quick Start](#-quick-start) • [Architecture](#-architecture) • [Documentation](#-documentation) • [API Reference](#-api-reference)

</div>

---

## 📋 Table of Contents

-   [Overview](#-overview)
-   [Features](#-features)
-   [Architecture](#-architecture)
-   [Tech Stack](#-tech-stack)
-   [Quick Start](#-quick-start)
    -   [Prerequisites](#prerequisites)
    -   [Local Development](#local-development)
    -   [Docker Compose](#docker-compose)
-   [Configuration](#-configuration)
-   [Database Management](#-database-management)
-   [API Reference](#-api-reference)
-   [Observability](#-observability)
-   [Testing](#-testing)
-   [Documentation](#-documentation)
-   [Security](#-security)
-   [Contributing](#-contributing)
-   [License](#-license)

---

## 🎯 Overview

The **E-Commerce Backend API** is a robust, scalable solution designed for modern e-commerce platforms. Built with clean architecture principles, it provides a solid foundation for managing products, orders, inventory, and customer data while maintaining full accounting traceability through double-entry bookkeeping.

### Key Highlights

✅ **Clean Architecture** - Layered design with clear separation of concerns (API, Application, Domain, Infrastructure)
✅ **Domain-Driven Design** - Rich domain model with aggregates, value objects, policies, specifications, and domain events
✅ **Automated Accounting** - Double-entry bookkeeping system compliant with Brazilian GAAP (NBC TG)
✅ **Enterprise Observability** - Full OpenTelemetry integration for distributed tracing and metrics
✅ **Production Ready** - Docker support, health checks, graceful shutdown, structured logging
✅ **Developer Experience** - Interactive API documentation with Scalar UI, automated database seeding
✅ **Security First** - JWT authentication, role-based authorization, password hashing with BCrypt

---

## ✨ Features

### Core Functionality

-   **Authentication & Authorization**

    -   JWT-based authentication with configurable expiration
    -   Role-based access control (Admin, Manager, Customer)
    -   Password hashing using BCrypt
    -   Token refresh and validation

-   **Product Management**

    -   Full CRUD operations with pagination
    -   Advanced search and filtering (category, price, rating)
    -   Featured products and sales management
    -   SKU-based inventory tracking
    -   Product specifications with value objects

-   **Inventory & Accounting**

    -   Real-time inventory tracking across multiple locations
    -   Automatic double-entry accounting for all inventory movements
    -   Transaction types: Purchase, Sale, Return, Adjustment, Loss, Transfer
    -   Full audit trail with traceability
    -   Chart of accounts seeded with 40+ predefined accounts
    -   Financial reports support (COGS, Trial Balance, Income Statement)

-   **Business Rules Engine**

    -   Pricing policies with discount validation
    -   Stock management with low-stock alerts
    -   Order validation and lifecycle management
    -   Coupon validation with usage limits
    -   Review moderation policies
    -   Fraud detection scoring

-   **Domain-Driven Design**
    -   Value Objects: Money, Discount, EmailAddress, SKU
    -   Policies: Pricing, Stock Management, Order Validation, Coupon Validation
    -   Specifications: Product and Order filtering criteria
    -   Domain Services: Discount Calculation, Order Lifecycle, Product Pricing, Fraud Detection
    -   Domain Events: Order Placed, Stock Alert, Product Price Changed, etc.

---

## 🏗️ Architecture

The solution follows **Clean Architecture** principles with four distinct layers:

```
┌─────────────────────────────────────────────────────────────┐
│                       Presentation Layer                     │
│                         (src/API)                            │
│  • Controllers (Auth, Products, Users, Accounting)          │
│  • Middlewares (Exception Handling)                         │
│  • OpenAPI/Scalar Documentation                             │
│  • OpenTelemetry Configuration                              │
└──────────────────┬──────────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────────┐
│                     Application Layer                        │
│                    (src/Application)                         │
│  • Application Services (JWT, Password, Accounting)         │
│  • DTOs (Data Transfer Objects)                             │
│  • Interfaces (Service Contracts)                           │
│  • Mappings (AutoMapper profiles)                           │
└──────────────────┬──────────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────────┐
│                       Domain Layer                           │
│                      (src/Domain)                            │
│  • Entities & Aggregates (User, Product, Order, etc.)      │
│  • Value Objects (Money, Discount, Email, SKU)              │
│  • Domain Services (Pricing, Order Lifecycle, Fraud)        │
│  • Policies (Pricing, Stock, Order, Coupon, Review)         │
│  • Specifications (Product, Order filtering)                │
│  • Domain Events (OrderPlaced, StockAlert, etc.)            │
│  • Domain Exceptions (Business rule violations)             │
└──────────────────┬──────────────────────────────────────────┘
                   │
┌──────────────────▼──────────────────────────────────────────┐
│                   Infrastructure Layer                       │
│                   (src/Infrastructure)                       │
│  • DbContext (PostgresqlContext - EF Core)                  │
│  • Entity Configurations (Fluent API)                       │
│  • Repositories (Generic + Specialized)                     │
│  • Migrations (Code-First Database)                         │
│  • Database Seeders (Admin User, Chart of Accounts)         │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Flow

-   **API** → Application → Domain ← Infrastructure
-   Dependencies flow inward; Domain has no external dependencies
-   Infrastructure implements interfaces defined in Domain/Application

### Key Design Patterns

-   **Repository Pattern** - Data access abstraction
-   **Unit of Work** - EF Core DbContext
-   **Specification Pattern** - Reusable query criteria
-   **Domain Events** - Decoupled business logic
-   **Value Objects** - Immutable, self-validating domain concepts
-   **Aggregate Root** - Consistency boundaries

---

## 🛠️ Tech Stack

### Backend Framework

-   **ASP.NET Core 9.0** - High-performance web framework
-   **C# 12** - Latest language features

### Data & Persistence

-   **PostgreSQL 16** - Robust relational database
-   **Entity Framework Core 9.0** - ORM with code-first migrations
-   **Npgsql 9.0** - PostgreSQL provider for EF Core

### Authentication & Security

-   **JWT Bearer Authentication** - Stateless authentication
-   **BCrypt** - Password hashing
-   **Role-Based Authorization** - Fine-grained access control

### API Documentation

-   **Microsoft.AspNetCore.OpenApi** - OpenAPI 3.0 specification
-   **Scalar.AspNetCore 2.9** - Modern, interactive API documentation UI

### Observability & Monitoring

-   **OpenTelemetry 1.13** - Distributed tracing and metrics
-   **OpenTelemetry.Instrumentation.AspNetCore** - HTTP request tracing
-   **OpenTelemetry.Instrumentation.EntityFrameworkCore** - Database query tracing
-   **OpenTelemetry.Instrumentation.Http** - HTTP client tracing
-   **OpenTelemetry.Instrumentation.Runtime** - .NET runtime metrics
-   **OpenTelemetry.Exporter.OpenTelemetryProtocol** - OTLP/gRPC exporter
-   **Jaeger** - Distributed tracing UI (Docker image)

### Containerization & Orchestration

-   **Docker** - Application containerization
-   **Docker Compose** - Multi-container orchestration
-   **Alpine Linux** - Minimal base images for security and size

### Development Tools

-   **.NET CLI** - Command-line interface for .NET
-   **EF Core CLI** - Database migrations and scaffolding

---

## 🚀 Quick Start

### Prerequisites

Ensure you have the following installed:

-   [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
-   [Docker Desktop](https://www.docker.com/products/docker-desktop) (for Docker-based development)
-   [PostgreSQL 16+](https://www.postgresql.org/download/) (for local development without Docker)
-   [Git](https://git-scm.com/downloads)

### Build and Run Options

You have **two ways** to build and run the application:

#### Option 1: Local Development (PostgreSQL on localhost)

Uses your **local PostgreSQL** installation. Ideal for rapid development with hot reload.

**Windows CMD:**

```cmd
cd scripts
build-local.cmd
```

**PowerShell:**

```powershell
cd scripts
.\build-local.ps1
```

**Requirements:**

-   PostgreSQL running locally on port 5432
-   Database credentials configured in `appsettings.json`

**What it does:**

1. ✅ Increments project version
2. ✅ Creates new EF Core migration
3. ✅ Builds project (Debug & Release)
4. ✅ Publishes artifacts
5. ✅ Exports SQL migration scripts
6. ✅ Applies migrations to local database

#### Option 2: Docker Build (PostgreSQL via container)

Runs everything in **Docker containers** with isolated PostgreSQL. Ideal for testing production-like environments.

**Windows CMD:**

```cmd
cd scripts
build-docker.cmd
```

**PowerShell:**

```powershell
cd scripts
.\build-docker.ps1
```

**Requirements:**

-   Docker Desktop installed and running
-   No local PostgreSQL needed

**What it does:**

1. ✅ Creates Docker network
2. ✅ Starts PostgreSQL container
3. ✅ Builds development image (port 5049)
4. ✅ Builds production image (port 8080)
5. ✅ Starts both containers with database connectivity

**Containers created:**

-   `ecommerce-postgres` - PostgreSQL 16 Alpine
-   `ecommerce-backend-dev` - Development mode
-   `ecommerce-backend-prod` - Production mode

**Cleanup Docker containers:**

```powershell
cd scripts
.\cleanup-docker.ps1  # PowerShell
cleanup-docker.cmd    # CMD
```

### Quick Setup (First Time)

#### 1. Clone the Repository

```powershell
git clone https://github.com/mgnischor/ecommerce-backend.git
cd ecommerce-backend
```

#### 2. Choose Your Approach

**For Local Development:**

```powershell
# Start local PostgreSQL (if not already running)
# Then run:
cd scripts
.\build-local.ps1
```

**For Docker Development:**

```powershell
cd scripts
.\build-docker.ps1
```

#### 3. Access the Application

**Local Build:**

-   **API**: `https://localhost:5049`
-   **API Docs**: `https://localhost:5049/docs`

**Docker Build:**

-   **Development API**: `http://localhost:5049`
-   **Production API**: `http://localhost:8080`
-   **PostgreSQL**: `localhost:5432`
-   **API Docs (Dev)**: `http://localhost:5049/docs`
-   **API Docs (Prod)**: `http://localhost:8080/docs`

#### 4. Default Credentials

On first run, an admin user is automatically seeded:

-   **Email**: `admin@ecommerce.com.br`
-   **Password**: `admin`

> ⚠️ **Important**: Change these credentials immediately in production!

### Docker Compose (Alternative)

For a complete environment with Jaeger tracing:

#### Development Mode (with Jaeger)

```powershell
docker-compose -f docker-compose.dev.yml up -d
```

**Services:**

-   API: `http://localhost:5049`
-   API Docs: `http://localhost:5049/docs`
-   PostgreSQL: `localhost:5432`
-   Jaeger UI: `http://localhost:16686`

#### Production Mode

```powershell
docker-compose up -d
```

**Services:**

-   API: `http://localhost`
-   PostgreSQL: `localhost:5432`
-   Jaeger UI: `http://localhost:16686`

#### Stop Services

```powershell
# Development
docker-compose -f docker-compose.dev.yml down

# Production
docker-compose down

# Remove volumes (database data)
docker-compose down -v
```

### Script Reference

For detailed information about all build scripts, see **[scripts/README.md](scripts/README.md)**.

---

## ⚙️ Configuration

---

## ⚙️ Configuration

### Application Settings

Configuration is managed through `appsettings.json` and `appsettings.Development.json`. Settings can be overridden via environment variables using the double-underscore syntax.

#### Connection Strings

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432"
    }
}
```

**Environment Variable:**

```powershell
$env:ConnectionStrings__DefaultConnection = "Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce;Port=5432"
```

#### JWT Configuration

```json
{
    "Jwt": {
        "SecretKey": "your-secret-key-min-32-chars",
        "Issuer": "ECommerceBackend",
        "Audience": "ECommerceClient",
        "ExpirationMinutes": "60"
    }
}
```

**Environment Variables:**

```powershell
$env:Jwt__SecretKey = "your-strong-secret-key-here"
$env:Jwt__Issuer = "ECommerceBackend"
$env:Jwt__Audience = "ECommerceClient"
$env:Jwt__ExpirationMinutes = "60"
```

#### OpenTelemetry Configuration

```json
{
    "OpenTelemetry": {
        "ServiceName": "ECommerce.Backend",
        "ServiceVersion": "0.1.17",
        "EnableConsoleExporter": false,
        "OtlpEndpoint": ""
    }
}
```

**Environment Variables:**

```powershell
$env:OpenTelemetry__ServiceName = "ECommerce.Backend"
$env:OpenTelemetry__ServiceVersion = "0.1.17"
$env:OpenTelemetry__EnableConsoleExporter = "true"
$env:OpenTelemetry__OtlpEndpoint = "http://localhost:4317"
```

#### Logging Configuration

```json
{
    "Logging": {
        "LogLevel": {
            "Default": "Information",
            "Microsoft.AspNetCore": "Warning",
            "Microsoft.EntityFrameworkCore": "Warning"
        }
    }
}
```

---

## 💾 Database Management

### Migrations

The project uses **Entity Framework Core Code-First Migrations** for database schema management.

#### Apply Latest Migrations

```powershell
dotnet ef database update
```

#### Create a New Migration

```powershell
dotnet ef migrations add <MigrationName> --output-dir src/Infrastructure/Migrations --context PostgresqlContext
```

#### Generate SQL Script

```powershell
dotnet ef migrations script --output migrations.sql
```

#### Remove Last Migration

```powershell
dotnet ef migrations remove
```

### Database Seeding

The application automatically seeds the database on first run:

1. **Admin User**

    - Email: `admin@ecommerce.com.br`
    - Password: `admin`
    - Role: Admin

2. **Chart of Accounts** (40+ accounts)
    - Assets (Cash, Bank, Inventory, Receivables)
    - Liabilities (Payables, Loans, Taxes)
    - Equity (Capital, Retained Earnings)
    - Revenue (Sales, Services, Other Income)
    - Expenses (COGS, Operating, Financial)

Seeding logic: `src/Infrastructure/Persistence/DatabaseSeeder.cs`

### Helper Scripts

PowerShell scripts are available in the `scripts/` directory:

-   **`build-local.ps1`** - Complete build pipeline (version bump, migration, build, publish, SQL export)
-   **`migration-script.ps1`** - Export migration SQL scripts
-   **`update-version.ps1`** - Bump project version

---

## 📡 API Reference

### Base URL

-   **Development**: `https://localhost:5049/api/v1`
-   **Docker (Dev)**: `http://localhost:5049/api/v1`
-   **Docker (Prod)**: `http://localhost/api/v1`

### Authentication Endpoints

#### Login

```http
POST /api/v1/login
Content-Type: application/json

{
  "email": "admin@ecommerce.com.br",
  "password": "admin"
}
```

**Response:**

```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiresIn": 3600,
    "tokenType": "Bearer",
    "userId": "...",
    "email": "admin@ecommerce.com.br",
    "accessLevel": "Admin"
}
```

### Product Endpoints

| Method | Endpoint                             | Description                   | Auth          |
| ------ | ------------------------------------ | ----------------------------- | ------------- |
| GET    | `/products`                          | List all products (paginated) | No            |
| GET    | `/products/{id}`                     | Get product by ID             | No            |
| GET    | `/products/sku/{sku}`                | Get product by SKU            | No            |
| GET    | `/products/category/{id}`            | Get products by category      | No            |
| GET    | `/products/featured`                 | Get featured products         | No            |
| GET    | `/products/on-sale`                  | Get products on sale          | No            |
| GET    | `/products/search?searchTerm={term}` | Search products               | No            |
| POST   | `/products`                          | Create product                | Admin/Manager |
| PUT    | `/products/{id}`                     | Update product                | Admin/Manager |
| PATCH  | `/products/{id}/soft-delete`         | Soft delete product           | Admin/Manager |
| DELETE | `/products/{id}`                     | Hard delete product           | Admin         |

### User Endpoints

| Method | Endpoint      | Description                | Auth |
| ------ | ------------- | -------------------------- | ---- |
| GET    | `/users`      | List all users (paginated) | Yes  |
| GET    | `/users/{id}` | Get user by ID             | Yes  |
| POST   | `/users`      | Create user                | Yes  |
| PUT    | `/users/{id}` | Update user                | Yes  |
| DELETE | `/users/{id}` | Delete user                | Yes  |

### Inventory Transaction Endpoints

| Method | Endpoint                                                 | Description                | Auth |
| ------ | -------------------------------------------------------- | -------------------------- | ---- |
| POST   | `/inventory-transactions`                                | Record transaction         | Yes  |
| GET    | `/inventory-transactions/{id}`                           | Get transaction by ID      | Yes  |
| GET    | `/inventory-transactions/product/{id}`                   | Get product transactions   | Yes  |
| GET    | `/inventory-transactions/period?startDate={}&endDate={}` | Get transactions by period | Yes  |

### Accounting Endpoints

| Method | Endpoint                                   | Description                      | Auth |
| ------ | ------------------------------------------ | -------------------------------- | ---- |
| GET    | `/accounting/chart-of-accounts`            | List all accounts                | Yes  |
| GET    | `/accounting/chart-of-accounts/{id}`       | Get account by ID                | Yes  |
| GET    | `/accounting/journal-entries`              | List journal entries (paginated) | Yes  |
| GET    | `/accounting/journal-entries/{id}`         | Get journal entry by ID          | Yes  |
| GET    | `/accounting/journal-entries/product/{id}` | Get entries by product           | Yes  |

### Authentication

Protected endpoints require a JWT token in the `Authorization` header:

```http
Authorization: Bearer <your-jwt-token>
```

### Interactive API Documentation

Access the **Scalar UI** at `/docs` for interactive API exploration:

-   Try out endpoints directly from the browser
-   View request/response schemas
-   Download OpenAPI specification (JSON/YAML)
-   Explore all available operations

---

## 📊 Observability

---

## 📊 Observability

The application is fully instrumented with **OpenTelemetry** for distributed tracing, metrics, and logging.

### Instrumentation

Automatic instrumentation is enabled for:

-   **ASP.NET Core** - HTTP request/response tracking with status codes, durations, and exceptions
-   **Entity Framework Core** - Database queries with SQL statements and execution times
-   **HTTP Client** - Outgoing HTTP requests with URIs and response codes
-   **.NET Runtime** - Garbage collection, thread pool, and exception metrics

### Distributed Tracing

#### Jaeger UI

When running with Docker Compose, Jaeger is available at:

**URL**: `http://localhost:16686`

**Features:**

-   View end-to-end request traces
-   Analyze service dependencies
-   Identify performance bottlenecks
-   Troubleshoot errors with full stack traces
-   Monitor request latency percentiles (p50, p75, p95, p99)

#### Custom Tracing

Add custom spans to your code:

```csharp
using ECommerce.API.Extensions;

public async Task<Order> ProcessOrderAsync(Guid orderId)
{
    using var activity = ActivityHelper.StartActivity("ProcessOrder");
    activity?.SetTag("order.id", orderId);

    try
    {
        // Your business logic
        var order = await _orderRepository.GetByIdAsync(orderId);

        ActivityHelper.AddEvent("OrderRetrieved");
        ActivityHelper.SetStatus(ActivityStatusCode.Ok);

        return order;
    }
    catch (Exception ex)
    {
        ActivityHelper.RecordException(ex);
        throw;
    }
}
```

### Metrics

Available metrics include:

-   **HTTP Metrics**

    -   `http.server.request.duration` - Request duration histogram
    -   `http.server.active_requests` - Active requests gauge

-   **Database Metrics**

    -   Query execution times
    -   Connection pool statistics

-   **Runtime Metrics**
    -   `process.runtime.dotnet.gc.collections.count` - GC collections
    -   `process.runtime.dotnet.thread_pool.threads.count` - Thread pool size
    -   `process.runtime.dotnet.exceptions.count` - Exception count

### Exporters

#### Console Exporter (Development)

Enable in `appsettings.Development.json`:

```json
{
    "OpenTelemetry": {
        "EnableConsoleExporter": true
    }
}
```

Traces and metrics will be printed to the console.

#### OTLP Exporter (Production)

Configure the OTLP endpoint for your observability backend:

**Jaeger:**

```json
{
    "OpenTelemetry": {
        "OtlpEndpoint": "http://jaeger:4317"
    }
}
```

**Grafana Cloud / Tempo:**

```json
{
    "OpenTelemetry": {
        "OtlpEndpoint": "https://otlp-gateway-prod-us-central-0.grafana.net/otlp"
    }
}
```

**Honeycomb:**

```json
{
    "OpenTelemetry": {
        "OtlpEndpoint": "https://api.honeycomb.io:443"
    }
}
```

### Structured Logging

All logs include contextual information:

```csharp
_logger.LogInformation(
    "Order created: OrderId={OrderId}, CustomerId={CustomerId}, Total={Total}",
    order.Id,
    order.CustomerId,
    order.TotalAmount
);
```

**Log Levels:**

-   `Trace` - Very detailed diagnostic information
-   `Debug` - Debugging information
-   `Information` - General informational messages
-   `Warning` - Warnings that don't prevent execution
-   `Error` - Errors that stop the current operation
-   `Critical` - Critical errors that require immediate attention

---

## 🧪 Testing

### Unit Tests

The project includes a comprehensive unit test suite located in the `tests/` directory.

**Tech Stack:**

-   **NUnit** - Testing framework
-   **FluentAssertions** - Fluent assertion library
-   **Moq** - Mocking library
-   **EF Core InMemory** - For database testing

**Structure:**

```
tests/
├── API/              # Controller and Middleware tests
├── Application/      # Service and Validator tests
├── Domain/           # Entity and Value Object tests
└── Infrastructure/   # Repository tests
```

### Running Tests

You can run the tests using the .NET CLI or the provided PowerShell scripts.

**Using .NET CLI:**

```powershell
dotnet test
```

**Using Scripts:**

```powershell
cd tests/scripts
.\run-tests.ps1
```

**With Coverage:**

```powershell
cd tests/scripts
.\run-tests-with-coverage.ps1
```

### Manual Testing

Use the interactive **Scalar UI** at `/docs` to manually test endpoints.

---

## 📚 Documentation

Comprehensive documentation is available in the `docs/` directory:

### Business & Architecture

-   **[BUSINESS_RULES.md](docs/BUSINESS_RULES.md)** - Domain policies, specifications, value objects, and business logic
-   **[ACCOUNTING_SYSTEM.md](docs/ACCOUNTING_SYSTEM.md)** - Double-entry accounting implementation following NBC TG
-   **[ACCOUNTING_INTEGRATION_GUIDE.md](docs/ACCOUNTING_INTEGRATION_GUIDE.md)** - How to integrate accounting in your code

### Observability

-   **[OPENTELEMETRY_GUIDE.md](docs/OPENTELEMETRY_GUIDE.md)** - Complete OpenTelemetry setup and custom instrumentation
-   **[RUNNING_WITH_JAEGER.md](docs/RUNNING_WITH_JAEGER.md)** - Running and troubleshooting with Jaeger

### Project Management

-   **[CHANGELOG.md](CHANGELOG.md)** - Version history and changes
-   **[CONTRIBUTING.md](CONTRIBUTING.md)** - Contribution guidelines
-   **[SECURITY_REVIEW.md](docs/SECURITY_REVIEW.md)** - Security considerations and best practices

---

## 🔒 Security

### Authentication

-   **JWT Bearer Tokens** with configurable expiration
-   **BCrypt password hashing** with salt
-   **Role-based authorization** (Admin, Manager, Customer)

### Best Practices

✅ **Never commit secrets** - Use environment variables or secure vaults
✅ **Strong JWT secret** - Minimum 32 characters, randomly generated
✅ **HTTPS in production** - Enable SSL/TLS certificates
✅ **Input validation** - All DTOs use data annotations
✅ **SQL injection protection** - EF Core parameterized queries
✅ **CORS configuration** - Configure allowed origins in production
✅ **Rate limiting** - Implement rate limiting middleware (planned)
✅ **Security headers** - Add HSTS, CSP, X-Frame-Options (planned)

### Production Checklist

Before deploying to production:

-   [ ] Change default admin credentials
-   [ ] Use strong, randomly generated JWT secret key
-   [ ] Enable HTTPS with valid SSL certificates
-   [ ] Configure CORS for specific origins
-   [ ] Set up database backups and disaster recovery
-   [ ] Enable application insights and monitoring
-   [ ] Review and implement security headers
-   [ ] Configure rate limiting and throttling
-   [ ] Set up Web Application Firewall (WAF)
-   [ ] Perform security audit and penetration testing

See **[SECURITY_REVIEW.md](docs/SECURITY_REVIEW.md)** for detailed security guidelines.

---

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/amazing-feature`)
3. **Commit your changes** (`git commit -m 'Add amazing feature'`)
4. **Push to the branch** (`git push origin feature/amazing-feature`)
5. **Open a Pull Request**

### Code Standards

-   Follow **Clean Architecture** principles
-   Write **meaningful commit messages**
-   Add **XML documentation** to public APIs
-   Follow **.NET coding conventions**
-   Ensure **no breaking changes** without discussion
-   Update documentation for new features

Read **[CONTRIBUTING.md](CONTRIBUTING.md)** for detailed contribution guidelines.

---

## 📄 License

This project is licensed under the **GNU General Public License v3.0** (GPL-3.0-only).

See the [LICENSE.md](LICENSE.md) file for full license text.

### Key Points

-   ✅ You can use, modify, and distribute this software
-   ✅ You must disclose source code when distributing
-   ✅ You must use the same GPL-3.0 license for derivative works
-   ✅ You must state changes made to the code
-   ❌ No warranty or liability is provided

---

## 👨‍💻 Author

**Miguel Nischor**

-   GitHub: [@mgnischor](https://github.com/mgnischor)
-   Repository: [ecommerce-backend](https://github.com/mgnischor/ecommerce-backend)

---

## 🙏 Acknowledgments

-   **ASP.NET Core Team** - For the excellent framework
-   **EF Core Team** - For the powerful ORM
-   **OpenTelemetry Community** - For observability standards
-   **PostgreSQL Team** - For the robust database
-   **Scalar Team** - For the beautiful API documentation UI

---

<div align="center">

**⭐ If you find this project useful, please consider giving it a star! ⭐**

Made with ❤️ using ASP.NET Core 9

</div>
