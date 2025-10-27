# E-Commerce Backend API

[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15+-blue.svg)](https://www.postgresql.org/)

A robust backend API for an e-commerce platform built with ASP.NET Core, following Clean Architecture principles. This project provides a scalable foundation for online retail applications with user authentication, product management, and order processing capabilities.

> **Note:** This repository is currently under active development. Features and APIs may change.

## 🚀 Features

-   **Clean Architecture**: Organized into Domain, Application, Infrastructure, and API layers
-   **JWT Authentication**: Secure user authentication and authorization
-   **PostgreSQL Database**: Reliable data persistence with Entity Framework Core
-   **RESTful API**: Well-structured endpoints for e-commerce operations
-   **OpenAPI Documentation**: Interactive API documentation with Scalar
-   **Docker Support**: Containerized deployment ready
-   **Migration Support**: Database schema versioning with EF Core migrations
-   **📊 Accounting System**: Integrated accounting system following Brazilian standards (NBC TG)
    -   Automatic journal entries for all inventory transactions
    -   Chart of accounts management
    -   Double-entry bookkeeping
    -   Full audit trail and traceability
    -   Support for purchases, sales, returns, adjustments, and losses

## 🛠 Tech Stack

-   **Framework**: ASP.NET Core 9.0
-   **Language**: C# 12
-   **Database**: PostgreSQL
-   **ORM**: Entity Framework Core 9.0
-   **Authentication**: JWT Bearer Tokens
-   **API Documentation**: OpenAPI/Swagger with Scalar
-   **Containerization**: Docker

## 📋 Prerequisites

Before running this application, make sure you have the following installed:

-   [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
-   [PostgreSQL](https://www.postgresql.org/download/) (version 15 or higher)
-   [Docker](https://www.docker.com/) (optional, for containerized deployment)

## 🔧 Installation

1. **Clone the repository:**

    ```bash
    git clone https://github.com/mgnischor/ecommerce-backend.git
    cd ecommerce-backend
    ```

2. **Restore dependencies:**

    ```bash
    dotnet restore
    ```

3. **Set up the database:**

    - Create a PostgreSQL database named `ecommerce`
    - Update the connection string in `appsettings.json` or set environment variables

4. **Configure JWT settings:**
   Update the JWT configuration in `appsettings.json`:
    ```json
    {
        "Jwt": {
            "SecretKey": "3zNarwviomHljbSBbFRvx42GNTw3lkCRS0PUg58Lf43GtMFZ2H",
            "Issuer": "your-issuer",
            "Audience": "your-audience"
        }
    }
    ```

## ⚙️ Configuration

The application uses the following configuration settings:

### Database

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Host=localhost;Database=ecommerce;Username=ecommerce;Password=ecommerce"
    }
}
```

### JWT Authentication

```json
{
    "Jwt": {
        "SecretKey": "3zNarwviomHljbSBbFRvx42GNTw3lkCRS0PUg58Lf43GtMFZ2H",
        "Issuer": "ECommerce.Backend",
        "Audience": "ECommerce.Client"
    }
}
```

## 🚀 Running the Application

### Development Mode

```bash
dotnet run --project ecommerce-backend.csproj
```

The API will be available at `https://localhost:5050` (HTTPS) and `http://localhost:5049` (HTTP).

### Using Docker

```bash
# Build the Docker image
docker build -t ecommerce-backend .

# Run the container
docker run -p 80:80 ecommerce-backend
```

### Database Migrations

```bash
# Add a new migration
dotnet ef migrations add MigrationName

# Update the database
dotnet ef database update
```

## 📚 API Documentation

When running in development mode, access the interactive API documentation at:

-   **Scalar UI**: `https://localhost:5049/docs`
-   **OpenAPI JSON**: `https://localhost:5049/openapi/v1.json`

### Additional Documentation

-   **[Accounting System](docs/ACCOUNTING_SYSTEM.md)** - Detailed documentation about the integrated accounting system
-   **[Accounting Integration Guide](docs/ACCOUNTING_INTEGRATION_GUIDE.md)** - How to use the accounting features in your code

## 🏗 Building and Deployment

### Local Build

Use the provided PowerShell script:

```powershell
.\scripts\build-local.ps1
```

### Docker Build

```bash
.\scripts\build-docker.cmd
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE.md](LICENSE.md) file for details.

## 👤 Author

**Miguel Nischor**

-   GitHub: [@mgnischor](https://github.com/mgnischor)

## 📞 Support

If you have any questions or need help, please open an issue on GitHub.

---

**Repository**: [https://github.com/mgnischor/ecommerce-backend](https://github.com/mgnischor/ecommerce-backend)
