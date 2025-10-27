# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

-   Product management system with comprehensive CRUD operations
-   Product categories and status enums
-   Migration SQL script generation automation

## [0.0.7] - 2024-10-27

### Added

-   **Products Table**: Complete product management system
    -   Product entity with 24+ properties including pricing, inventory, and metadata
    -   SKU (Stock Keeping Unit) unique identifier
    -   Category system with 18 predefined categories (Electronics, Clothing, Books, etc.)
    -   Product status tracking (Draft, Active, Inactive, OutOfStock, Discontinued)
    -   Pricing with support for regular and discount prices
    -   Inventory management (stock quantity, min stock level, max order quantity)
    -   Product flags (is_active, is_deleted, is_featured, is_on_sale)
    -   Multiple images and tags support using PostgreSQL arrays
    -   Weight tracking for shipping calculations
-   **Database Indexes** for products:
    -   Unique index on SKU
    -   Indexes on: category, status, name, created_at, is_featured, is_on_sale
-   **Product Repository**: Full data access layer with methods:
    -   CRUD operations (Create, Read, Update, Delete)
    -   Search by ID, SKU, category, name
    -   Filter by featured and on-sale products
    -   Pagination support
    -   Soft delete functionality
-   **Product API Controller**: RESTful endpoints at `/api/v1/products`
    -   `GET /api/v1/products` - List all products (paginated)
    -   `GET /api/v1/products/{id}` - Get product by ID
    -   `GET /api/v1/products/sku/{sku}` - Get product by SKU
    -   `GET /api/v1/products/category/{category}` - Filter by category
    -   `GET /api/v1/products/featured` - Get featured products
    -   `GET /api/v1/products/on-sale` - Get products on sale
    -   `GET /api/v1/products/search` - Search by name
    -   `POST /api/v1/products` - Create product (Admin/Manager only)
    -   `PUT /api/v1/products/{id}` - Update product (Admin/Manager only)
    -   `DELETE /api/v1/products/{id}` - Delete product (Admin only)
    -   `PATCH /api/v1/products/{id}/soft-delete` - Soft delete (Admin/Manager only)
-   **Build Scripts Enhancement**:
    -   Automatic SQL migration script generation during build
    -   Creates individual SQL files for each migration
    -   Generates complete idempotent migration script
-   **Migration Scripts**:
    -   `migration-script.ps1` - PowerShell version
    -   `migration-script.cmd` - Windows CMD version
    -   Exports all migrations to `src/Infrastructure/SQL/`

### Database Schema

```sql
CREATE TABLE products (
  id uuid PRIMARY KEY,
  created_by uuid DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
  updated_by uuid DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
  category integer DEFAULT 0,
  status integer DEFAULT 1,
  name varchar(200) NOT NULL,
  description varchar(2000),
  sku varchar(50) UNIQUE NOT NULL,
  brand varchar(100),
  image_url varchar(500),
  price decimal(18,2) NOT NULL,
  discount_price decimal(18,2),
  weight decimal(10,2) DEFAULT 0,
  stock_quantity integer DEFAULT 0,
  min_stock_level integer DEFAULT 0,
  max_order_quantity integer DEFAULT 100,
  is_active boolean DEFAULT true,
  is_deleted boolean DEFAULT false,
  is_featured boolean DEFAULT false,
  is_on_sale boolean DEFAULT false,
  tags text[] DEFAULT '{}',
  images text[] DEFAULT '{}',
  created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
  updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);
```

## [0.0.6] - 2024-10-27

### Changed

-   Empty migration (no database changes)
-   Build version increment only

## [0.0.5] - 2024-10-27

### Changed

-   Empty migration (no database changes)
-   Build version increment only

## [0.0.4] - 2024-10-27

### Changed

-   Empty migration (no database changes)
-   Build version increment only

## [0.0.3] - 2024-10-27

### Changed

-   Empty migration (no database changes)
-   Build version increment only

## [0.0.2] - 2024-10-27

### Added

-   **Users Table**: Initial database schema creation
    -   User entity with authentication and profile management
    -   Email and username with unique constraints
    -   Password hashing support
    -   User access levels (Guest, Customer, Company, Admin, Manager, Developer)
    -   Address fields (address, city, country)
    -   User flags (is_active, is_banned, is_deleted, is_email_verified, is_debug_enabled)
    -   Groups and favorite products arrays
    -   Timestamps (created_at, updated_at) with automatic defaults
-   **Database Indexes**:
    -   Unique indexes on email and username
    -   Index on created_at for sorting
-   **User Repository**: Complete data access layer
-   **User API Controller**: RESTful endpoints at `/api/v1/users`
-   **Authentication System**:
    -   JWT-based authentication
    -   Password hashing with bcrypt
    -   Role-based authorization
-   **Database Seeding**:
    -   Automatic admin user creation on startup
    -   Default credentials for development

### Database Schema

```sql
CREATE TABLE users (
  id uuid PRIMARY KEY,
  created_by uuid DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
  updated_by uuid,
  access_level integer,
  address varchar(500),
  city varchar(100),
  country varchar(100),
  email varchar(255) UNIQUE NOT NULL,
  password_hash varchar(256) NOT NULL,
  username varchar(50) UNIQUE NOT NULL,
  is_active boolean,
  is_banned boolean,
  is_debug_enabled boolean,
  is_deleted boolean,
  is_email_verified boolean,
  groups uuid[],
  favorite_products uuid[],
  birth_date date,
  created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP,
  updated_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP
);
```

## [0.0.1] - 2024-10-27

### Added

-   Initial project setup
-   ASP.NET Core 9.0 backend
-   Clean Architecture structure (Domain, Application, Infrastructure, API)
-   PostgreSQL database integration with Entity Framework Core
-   OpenAPI/Swagger documentation with Scalar UI
-   Docker support
-   Build and deployment scripts
-   Project documentation (README.md, CONTRIBUTING.md, LICENSE.md)

### Project Structure

-   `src/Domain/` - Entities, Value Objects, Enums, Interfaces
-   `src/Application/` - Services, DTOs, Commands, Queries
-   `src/Infrastructure/` - Database context, Repositories, Configurations
-   `src/API/` - Controllers, Middlewares, Extensions

---

## Version History

-   **0.0.7** - Product Management System
-   **0.0.6** - Build version increment
-   **0.0.5** - Build version increment
-   **0.0.4** - Build version increment
-   **0.0.3** - Build version increment
-   **0.0.2** - User Management & Authentication
-   **0.0.1** - Initial Setup

[Unreleased]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.7...HEAD
[0.0.7]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.6...v0.0.7
[0.0.6]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.5...v0.0.6
[0.0.5]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.4...v0.0.5
[0.0.4]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.3...v0.0.4
[0.0.3]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.2...v0.0.3
[0.0.2]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.1...v0.0.2
[0.0.1]: https://github.com/mgnischor/ecommerce-backend/releases/tag/v0.0.1
