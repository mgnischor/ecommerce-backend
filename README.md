# E-Commerce Backend API# E-Commerce Backend API

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)

[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)

[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-blue.svg)](https://www.postgresql.org/)[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-blue.svg)](https://www.postgresql.org/)

[![Version](https://img.shields.io/badge/Version-0.0.11-green.svg)](https://github.com/mgnischor/ecommerce-backend)[![Version](https://img.shields.io/badge/Version-0.0.11-green.svg)](https://github.com/mgnischor/ecommerce-backend)

A robust backend API for an e-commerce platform built with ASP.NET Core 9.0, following Clean Architecture principles. This project provides a comprehensive solution for online retail applications with advanced features including user authentication, product management, inventory tracking, accounting system, and full observability.A robust backend API for an e-commerce platform built with ASP.NET Core 9.0, following Clean Architecture principles. This project provides a comprehensive solution for online retail applications with advanced features including user authentication, product management, inventory tracking, accounting system, and full observability.

> **Note:** This repository is currently under active development. Features and APIs may change.> **Note:** This repository is currently under active development. Features and APIs may change.

## 🚀 Features## 🚀 Features

### Core Functionality- **Clean Architecture**: Organized into Domain, Application, Infrastructure, and API layers

-   **Clean Architecture**: Organized into Domain, Application, Infrastructure, and API layers- **JWT Authentication**: Secure user authentication and authorization

-   **RESTful API**: Well-structured endpoints following REST best practices- **PostgreSQL Database**: Reliable data persistence with Entity Framework Core

-   **OpenAPI Documentation**: Interactive API documentation with Scalar UI- **RESTful API**: Well-structured endpoints for e-commerce operations

-   **Docker Support**: Complete containerization with Docker Compose- **OpenAPI Documentation**: Interactive API documentation with Scalar

-   **Migration Support**: Database schema versioning with EF Core migrations- **Docker Support**: Containerized deployment ready

-   **Migration Support**: Database schema versioning with EF Core migrations

### Authentication & Security- **� OpenTelemetry Integration**: Full observability with distributed tracing and metrics

-   **JWT Authentication**: Secure user authentication and authorization - Automatic instrumentation for HTTP requests, database queries, and exceptions

-   **Role-Based Access Control**: Multiple access levels (Guest, Customer, Company, Admin, Manager, Developer) - Custom tracing support for business operations

-   **Password Security**: Bcrypt password hashing with industry-standard practices - OTLP export to Jaeger, Grafana Tempo, or other backends
    -   Runtime metrics (GC, thread pool, etc.)

### User Management- **�📊 Accounting System**: Integrated accounting system following Brazilian standards (NBC TG)

-   **Complete CRUD Operations**: Create, read, update, and delete users - Automatic journal entries for all inventory transactions

-   **User Profiles**: Comprehensive user information including address, favorites, and groups - Chart of accounts management

-   **Email Verification**: Support for email verification workflow - Double-entry bookkeeping

-   **User Status Management**: Active, banned, and soft delete capabilities - Full audit trail and traceability
    -   Support for purchases, sales, returns, adjustments, and losses

### Product Management

-   **Product Catalog**: Full product information with SKU, pricing, and descriptions## 🛠 Tech Stack

-   **Category System**: 18 predefined categories (Electronics, Clothing, Books, Home & Garden, etc.)

-   **Inventory Tracking**: Stock quantity, minimum stock levels, and max order quantity- **Framework**: ASP.NET Core 9.0

-   **Product Status**: Draft, Active, Inactive, OutOfStock, Discontinued- **Language**: C# 12

-   **Featured Products**: Mark products as featured or on sale- **Database**: PostgreSQL

-   **Multiple Images & Tags**: Support for product image arrays and tagging- **ORM**: Entity Framework Core 9.0

-   **Search & Filters**: Search by name, category, SKU, featured status, and sale status- **Authentication**: JWT Bearer Tokens

-   **Soft Delete**: Safe product deletion without losing historical data- **API Documentation**: OpenAPI/Swagger with Scalar

-   **Observability**: OpenTelemetry with OTLP exporter

### E-Commerce Domain- **Containerization**: Docker

-   **Orders Management**: Complete order lifecycle with status tracking

-   **Shopping Cart**: Session-based cart with coupon support## 📋 Prerequisites

-   **Payment Processing**: Multiple payment methods (Credit Card, PayPal, Pix, etc.) and transaction tracking

-   **Product Reviews**: Customer reviews with 1-5 star rating system and verificationBefore running this application, make sure you have the following installed:

-   **Wishlist System**: Multiple wishlists per customer with priorities and notes

-   **Address Management**: Shipping and billing address support- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

-   **Coupon System**: Percentage and fixed amount discounts with usage limits- [PostgreSQL](https://www.postgresql.org/download/) (version 15 or higher)

-   [Docker](https://www.docker.com/) (optional, for containerized deployment)

### Inventory & Transactions

-   **Multi-Location Inventory**: Track stock across multiple warehouse locations## 🔧 Installation

-   **Transaction Types**: Purchase, Sale, Returns (Sale/Purchase), Adjustments, Transfers, Losses, Reservations, Fulfillment

-   **Stock Movements**: Complete audit trail of all inventory changes1. **Clone the repository:**

-   **Automatic Cost Calculation**: Unit cost and total cost tracking per transaction

-   **Document Tracking**: Link transactions to invoices and fiscal documents ```bash

    git clone https://github.com/mgnischor/ecommerce-backend.git

### 📊 Accounting System cd ecommerce-backend

-   **Chart of Accounts**: Hierarchical account structure following Brazilian standards (NBC TG) ```

-   **Double-Entry Bookkeeping**: Automatic journal entries for all inventory transactions

-   **Account Types**: Assets, Liabilities, Equity, Revenue, Expenses2. **Restore dependencies:**

-   **Journal Entries**: Complete audit trail with post/unpost functionality

-   **Cost Tracking**: Automatic COGS (Cost of Goods Sold) calculations ```bash

-   **Inventory Integration**: Seamless accounting entries for all inventory movements dotnet restore

-   **Compliance**: Follows NBC TG 16 (Inventory), NBC TG 26 (Financial Statements), Lei 6.404/76 ```

-   **Real-Time Balances**: Automatic balance updates for all affected accounts

3. **Set up the database:**

### 🔭 Observability & Monitoring

-   **OpenTelemetry Integration**: Full distributed tracing and metrics - Create a PostgreSQL database named `ecommerce`

    -   Automatic instrumentation for HTTP requests and database queries - Update the connection string in `appsettings.json` or set environment variables

    -   Custom tracing support for business operations

    -   OTLP export to Jaeger, Grafana Tempo, or other backends4. **Configure JWT settings:**

    -   Runtime metrics (GC, thread pool, memory, etc.) Update the JWT configuration in `appsettings.json`:

-   **Comprehensive Logging**: Structured logging throughout the application ```json

    -   Request/response logging in all controllers {

    -   Error and warning logging in services and repositories "Jwt": {

    -   Database operation logging "SecretKey": "3zNarwviomHljbSBbFRvx42GNTw3lkCRS0PUg58Lf43GtMFZ2H",

    -   Environment-specific log levels (Debug for Development, Info for Production) "Issuer": "your-issuer",

                "Audience": "your-audience"

### Data Persistence }

-   **PostgreSQL Database**: Reliable data persistence with Entity Framework Core 9.0 }

-   **16 Database Tables**: Complete schema for e-commerce operations ```

-   **Soft Delete**: Safe deletion across all entities

-   **Audit Trail**: Created/Updated timestamps and user tracking on all entities## ⚙️ Configuration

-   **Indexes & Constraints**: Optimized database performance with proper indexing

The application uses the following configuration settings:

## 🛠 Tech Stack

### Database

-   **Framework**: ASP.NET Core 9.0

-   **Language**: C# 12```json

-   **Database**: PostgreSQL 15+{

-   **ORM**: Entity Framework Core 9.0 "ConnectionStrings": {

-   **Authentication**: JWT Bearer Tokens "DefaultConnection": "Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce"

-   **API Documentation**: OpenAPI/Swagger with Scalar UI (v2.9.0) }

-   **Observability**: OpenTelemetry (v1.13.1) with OTLP exporter}

-   **Containerization**: Docker & Docker Compose```

-   **Version**: 0.0.11

### JWT Authentication

## 📋 Prerequisites

```json

Before running this application, make sure you have the following installed:{

    "Jwt": {

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)        "SecretKey": "3zNarwviomHljbSBbFRvx42GNTw3lkCRS0PUg58Lf43GtMFZ2H",

-   [PostgreSQL](https://www.postgresql.org/download/) (version 15 or higher)        "Issuer": "ECommerce.Backend",

-   [Docker](https://www.docker.com/) (optional, for containerized deployment)        "Audience": "ECommerce.Client"

    }

## 🔧 Installation}

```

1. **Clone the repository:**

## 🚀 Running the Application

    ```bash

    git clone https://github.com/mgnischor/ecommerce-backend.git### Development Mode

    cd ecommerce-backend

    ``````bash

dotnet run --project ecommerce-backend.csproj

2. **Restore dependencies:**```

    ```bashThe API will be available at `http://localhost:5049`.

    dotnet restore

    ```### Using Docker

    ```

3. **Set up the database:**```bash

# Build the Docker image

    - Create a PostgreSQL database named `ecommerce`docker build -t ecommerce-backend .

    - Update the connection string in `appsettings.json` or set environment variables

# Run the container (Production mode on port 80)

4. **Configure JWT settings:**docker run -p 80:80 ecommerce-backend

    ````

    Update the JWT configuration in `appsettings.json`:

    ### Database Migrations

     ```json

     {```bash

         "Jwt": {# Add a new migration

             "SecretKey": "your-secret-key-at-least-32-characters-long",dotnet ef migrations add MigrationName

             "Issuer": "ECommerce.Backend",

             "Audience": "ECommerce.Client"# Update the database

         }dotnet ef database update

     }```

    ````

### With OpenTelemetry and Jaeger

5. **Run database migrations:**

Run with full observability stack:

    ```bash

    dotnet ef database update```powershell

    ```# Start all services (API, PostgreSQL, Jaeger)

docker-compose up -d

## ⚙️ Configuration

# Access API documentation

The application uses the following configuration settings:# http://localhost/docs

### Database# Access Jaeger UI for distributed tracing

# http://localhost:16686

`json`

{

    "ConnectionStrings": {For more details, see [Running with Jaeger](docs/RUNNING_WITH_JAEGER.md).

        "DefaultConnection": "Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce"

    }## 📚 API Documentation

}

```````When running in development mode, access the interactive API documentation at:



### JWT Authentication-   **Scalar UI**: `http://localhost:5049/docs`

-   **OpenAPI JSON**: `http://localhost:5049/openapi/v1.json`

```json

{### Additional Documentation

    "Jwt": {

        "SecretKey": "3zNarwviomHljbSBbFRvx42GNTw3lkCRS0PUg58Lf43GtMFZ2H",-   **[OpenTelemetry Guide](docs/OPENTELEMETRY_GUIDE.md)** - Complete guide for distributed tracing and observability

        "Issuer": "ECommerce.Backend",-   **[Running with Jaeger](docs/RUNNING_WITH_JAEGER.md)** - How to run the application with Jaeger for tracing

        "Audience": "ECommerce.Client"-   **[Accounting System](docs/ACCOUNTING_SYSTEM.md)** - Detailed documentation about the integrated accounting system

    }-   **[Accounting Integration Guide](docs/ACCOUNTING_INTEGRATION_GUIDE.md)** - How to use the accounting features in your code

}

```## 🏗 Building and Deployment



### OpenTelemetry (Optional)### Local Build



```jsonUse the provided PowerShell script:

{

    "OpenTelemetry": {```powershell

        "ServiceName": "ECommerce.Backend",.\scripts\build-local.ps1

        "ServiceVersion": "0.0.11",```

        "OtlpEndpoint": "http://localhost:4317"

    }### Docker Build

}

``````bash

.\scripts\build-docker.cmd

## 🚀 Running the Application```



### Development Mode## 🤝 Contributing



```bash1. Fork the repository

dotnet run --project ecommerce-backend.csproj2. Create a feature branch (`git checkout -b feature/amazing-feature`)

```3. Commit your changes (`git commit -m 'Add some amazing feature'`)

4. Push to the branch (`git push origin feature/amazing-feature`)

The API will be available at `http://localhost:5049`.5. Open a Pull Request



### Using Docker Compose (Recommended)## 📄 License



Run with full stack (API, PostgreSQL, Jaeger):This project is licensed under the GNU General Public License v3.0 - see the [LICENSE.md](LICENSE.md) file for details.



```powershell## 👤 Author

# Start all services

docker-compose up -d**Miguel Nischor**



# View logs-   GitHub: [@mgnischor](https://github.com/mgnischor)

docker-compose logs -f

## 📞 Support

# Stop all services

docker-compose downIf you have any questions or need help, please open an issue on GitHub.

```````

---

Access points:

-   **API Documentation**: `http://localhost/docs`**Repository**: [https://github.com/mgnischor/ecommerce-backend](https://github.com/mgnischor/ecommerce-backend)

-   **Jaeger UI**: `http://localhost:16686`
-   **API**: `http://localhost`

### Production Build

```bash
# Build for production
dotnet build -c Release

# Run in production mode
dotnet run -c Release
```

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Update the database
dotnet ef database update

# Generate SQL script
dotnet ef migrations script

# Revert last migration
dotnet ef migrations remove
```

## 📚 API Endpoints

### Authentication

-   `POST /api/v1/login` - Authenticate user and get JWT token
-   `GET /api/v1/endpoints` - List available endpoints
-   `OPTIONS /api/v1` - Get allowed HTTP methods

### Users

-   `GET /api/v1/users` - List all users (paginated)
-   `GET /api/v1/users/{id}` - Get user by ID
-   `POST /api/v1/users` - Create new user
-   `PUT /api/v1/users/{id}` - Update user
-   `DELETE /api/v1/users/{id}` - Delete user (soft delete)

### Products

-   `GET /api/v1/products` - List all products (paginated)
-   `GET /api/v1/products/{id}` - Get product by ID
-   `GET /api/v1/products/sku/{sku}` - Get product by SKU
-   `GET /api/v1/products/category/{category}` - Filter by category
-   `GET /api/v1/products/featured` - Get featured products
-   `GET /api/v1/products/on-sale` - Get products on sale
-   `GET /api/v1/products/search?name={name}` - Search by name
-   `POST /api/v1/products` - Create product (Admin/Manager)
-   `PUT /api/v1/products/{id}` - Update product (Admin/Manager)
-   `DELETE /api/v1/products/{id}` - Delete product (Admin)
-   `PATCH /api/v1/products/{id}/soft-delete` - Soft delete (Admin/Manager)

### Inventory Transactions

-   `POST /api/v1/inventory-transactions` - Record new transaction
-   `GET /api/v1/inventory-transactions/{id}` - Get transaction by ID
-   `GET /api/v1/inventory-transactions/product/{productId}` - Get product transaction history

### Accounting

-   `GET /api/v1/accounting/chart-of-accounts` - List all accounts
-   `GET /api/v1/accounting/chart-of-accounts/{id}` - Get account by ID
-   `POST /api/v1/accounting/chart-of-accounts` - Create new account
-   `GET /api/v1/accounting/journal-entries` - List all journal entries
-   `GET /api/v1/accounting/journal-entries/{id}` - Get entry by ID

## 📖 API Documentation

When running in development mode, access the interactive API documentation:

-   **Scalar UI**: `http://localhost:5049/docs`
-   **OpenAPI JSON**: `http://localhost:5049/openapi/v1.json`

### Additional Documentation

-   **[OpenTelemetry Guide](docs/OPENTELEMETRY_GUIDE.md)** - Complete guide for distributed tracing and observability
-   **[Running with Jaeger](docs/RUNNING_WITH_JAEGER.md)** - How to run the application with Jaeger for tracing
-   **[Accounting System](docs/ACCOUNTING_SYSTEM.md)** - Detailed documentation about the integrated accounting system
-   **[Accounting Integration Guide](docs/ACCOUNTING_INTEGRATION_GUIDE.md)** - How to use the accounting features in your code
-   **[Business Rules](docs/BUSINESS_RULES.md)** - Business rules and validation logic
-   **[Security Review](docs/SECURITY_REVIEW.md)** - Security considerations and best practices
-   **[Changelog](CHANGELOG.md)** - Version history and changes

## 🏗 Building and Deployment

### Local Build

Use the provided PowerShell script:

```powershell
.\scripts\build-local.ps1
```

Or manually:

```bash
dotnet build -c Release
dotnet publish -c Release -o ./publish
```

### Docker Build

```bash
# Using script
.\scripts\build-docker.cmd

# Or manually
docker build -t ecommerce-backend:latest .
docker run -p 80:80 ecommerce-backend:latest
```

### Version Management

Update version in `version.txt` and the build script will automatically update:

-   `ecommerce-backend.csproj`
-   Assembly versions
-   Docker image tags

## 🗄️ Database Schema

The application includes the following tables:

### Core Tables

-   **users** - User accounts and profiles
-   **products** - Product catalog
-   **categories** - Product categories (hierarchical)

### E-Commerce Tables

-   **orders** - Customer orders
-   **order_items** - Order line items
-   **carts** - Shopping carts
-   **cart_items** - Cart line items
-   **payments** - Payment transactions
-   **reviews** - Product reviews
-   **wishlists** - Customer wishlists
-   **wishlist_items** - Wishlist items
-   **addresses** - Shipping/billing addresses
-   **coupons** - Discount coupons
-   **inventories** - Inventory tracking

### Accounting Tables

-   **chart_of_accounts** - Chart of accounts
-   **journal_entries** - Journal entries
-   **accounting_entries** - Individual debit/credit entries
-   **inventory_transactions** - Inventory movements with accounting integration

## 🤝 Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for details on:

-   Code of Conduct
-   Development workflow
-   Pull request process
-   Coding standards

### Development Workflow

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Run tests and ensure code quality
5. Commit your changes (`git commit -m 'Add some amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

## 📄 License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE.md](LICENSE.md) file for details.

### Summary

-   ✅ You can use this software for commercial purposes
-   ✅ You can modify and distribute it
-   ✅ You can use it privately
-   ⚠️ You must disclose the source code
-   ⚠️ You must include the license and copyright notice
-   ⚠️ Changes must be documented

## 👤 Author

**Miguel Nischor**

-   GitHub: [@mgnischor](https://github.com/mgnischor)
-   Repository: [ecommerce-backend](https://github.com/mgnischor/ecommerce-backend)

## 📞 Support

If you have any questions or need help:

-   Open an [issue](https://github.com/mgnischor/ecommerce-backend/issues) on GitHub
-   Check the [documentation](docs/)
-   Review the [changelog](CHANGELOG.md)

## 🗺️ Roadmap

### Planned Features

-   [ ] Shopping cart API endpoints
-   [ ] Order processing and management
-   [ ] Payment gateway integration
-   [ ] Email notifications
-   [ ] File upload for product images
-   [ ] Real-time inventory updates
-   [ ] Advanced search with filters
-   [ ] Caching layer
-   [ ] Rate limiting
-   [ ] API versioning

### Completed

-   [x] User management with authentication
-   [x] Product catalog with categories
-   [x] JWT authentication & authorization
-   [x] Integrated accounting system
-   [x] Inventory tracking with transactions
-   [x] OpenTelemetry integration
-   [x] Comprehensive logging
-   [x] Docker containerization

## 📊 Project Statistics

-   **Version**: 0.0.11
-   **Last Updated**: October 2025
-   **Database Tables**: 16
-   **API Endpoints**: 25+
-   **Controllers**: 5
-   **Architecture**: Clean Architecture
-   **License**: GPL-3.0

---

**Repository**: [https://github.com/mgnischor/ecommerce-backend](https://github.com/mgnischor/ecommerce-backend)

**Documentation**: [docs/](docs/)

**License**: [GPL-3.0](LICENSE.md)
