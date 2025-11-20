# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

### Changed

### Fixed

## [0.1.17] - 2025-11-20

### Added

-   **Unit Testing Project**: Complete test suite implementation
    -   Added `tests/` project using NUnit, Moq, and FluentAssertions
    -   Implemented tests for Domain, Application, Infrastructure, and API layers
    -   Added test scripts in `tests/scripts/` for running tests and generating coverage reports
-   **Documentation Updates**:
    -   Updated README.md with testing instructions and current version
    -   Updated OpenTelemetry configuration version

### Changed

-   Updated project version to 0.1.17
-   Refined build and deployment scripts

## [0.0.10] - 2025-10-27

### Added

-   **Complete E-Commerce Domain Entities**: Comprehensive database schema for e-commerce operations
    -   **Orders Management**:
        -   `OrderEntity` with order tracking, status management, and multi-address support
        -   `OrderItemEntity` for line items with pricing, discounts, and tax calculations
        -   Order statuses: Pending, Processing, Shipped, Delivered, Cancelled, Returned, Refunded
        -   Payment methods: CreditCard, DebitCard, PayPal, BankTransfer, Cash, Pix
        -   Shipping methods: Standard, Express, Overnight, Pickup
    -   **Cart System**:
        -   `CartEntity` with session support and coupon integration
        -   `CartItemEntity` for cart line items
        -   Cart expiration and discount tracking
    -   **Category Management**:
        -   `CategoryEntity` with hierarchical structure support
        -   SEO optimization (slug, meta tags)
        -   Display order and soft delete capabilities
    -   **Inventory Tracking**:
        -   `InventoryEntity` with multi-location support
        -   Stock tracking: in stock, reserved, available
        -   Reorder level and quantity management
        -   Last stock received and inventory count timestamps
    -   **Payment Processing**:
        -   `PaymentEntity` with transaction tracking
        -   Payment statuses: Pending, Authorized, Captured, Failed, Refunded, Cancelled
        -   Support for multiple payment providers
        -   Refund management with reasons
    -   **Product Reviews**:
        -   `ReviewEntity` with rating system (1-5 stars)
        -   Verified purchase badges
        -   Approval and flagging system
        -   Helpful/not helpful voting
        -   Admin response capability
    -   **Wishlist System**:
        -   `WishlistEntity` with public/private lists
        -   `WishlistItemEntity` with priority and notes
        -   Multiple wishlists per customer
    -   **Address Management**:
        -   `AddressEntity` for shipping and billing addresses
        -   Full address details with phone number
        -   Default address designation
    -   **Coupon System**:
        -   `CouponEntity` with percentage and fixed amount discounts
        -   Usage limits (total and per customer)
        -   Product and category applicability
        -   Validity period management
        -   Minimum order amount and maximum discount constraints

### Database Schema

-   10 new tables created: `Orders`, `OrderItems`, `Carts`, `CartItems`, `Categories`, `Inventories`, `Payments`, `Reviews`, `Wishlists`, `WishlistItems`, `Addresses`, `Coupons`
-   Comprehensive entity relationships and foreign keys
-   Soft delete support across all entities
-   Audit trail with created/updated timestamps

## [0.0.9] - 2025-10-27

### Added

-   **📊 Integrated Accounting System**: Complete accounting system following Brazilian standards (NBC TG)
    -   **Chart of Accounts** (Plano de Contas):
        -   `ChartOfAccountsEntity` with hierarchical account structure
        -   Support for 5 account types: Asset, Liability, Equity, Revenue, Expense
        -   Analytic and synthetic accounts
        -   Real-time balance tracking
        -   SQL initialization script with standard accounts for e-commerce
    -   **Journal Entries** (Lançamentos Contábeis):
        -   `JournalEntryEntity` for double-entry bookkeeping
        -   `AccountingEntryEntity` for individual debits and credits
        -   Automatic journal entry generation for all inventory transactions
        -   Post/unpost functionality for accounting period control
        -   Support for cost centers
    -   **Inventory Transactions** (Movimentações de Estoque):
        -   `InventoryTransactionEntity` with full traceability
        -   10 transaction types: Purchase, Sale, SaleReturn, PurchaseReturn, Adjustment, Transfer, Loss, Reservation, ReservationRelease, Fulfillment
        -   Automatic cost calculation (unit cost × quantity)
        -   Document number tracking for fiscal compliance
        -   Links to products, orders, and journal entries
    -   **Accounting Service** (`IAccountingService`):
        -   `RecordPurchaseAsync()` - Debit: Inventory, Credit: Suppliers
        -   `RecordSaleAsync()` - Debit: CMV, Credit: Inventory
        -   `RecordSaleReturnAsync()` - Debit: Inventory, Credit: CMV
        -   `RecordPurchaseReturnAsync()` - Debit: Suppliers, Credit: Inventory
        -   `RecordAdjustmentAsync()` - Handles positive/negative adjustments
        -   `RecordLossAsync()` - Debit: Loss Expense, Credit: Inventory
        -   Automatic account creation with standard chart of accounts
        -   Real-time balance updates
    -   **Inventory Transaction Service** (`IInventoryTransactionService`):
        -   `RecordTransactionAsync()` - Records transaction and generates accounting entry
        -   `GetProductTransactionsAsync()` - Historical transactions per product
        -   `GetTransactionsByPeriodAsync()` - Period-based reports
        -   Sequential transaction numbering with prefixes
        -   Automatic cost center assignment
    -   **Entity Framework Configurations**:
        -   `ChartOfAccountsConfiguration` with indexes and constraints
        -   `JournalEntryConfiguration` with proper relationships
        -   `AccountingEntryConfiguration` with cascade/restrict delete behaviors
        -   `InventoryTransactionConfiguration` with full referential integrity
        -   Updated `PostgresqlContext` with new DbSets
    -   **Enumerations**:
        -   `AccountType` - Asset, Liability, Equity, Revenue, Expense
        -   `EntryType` - Debit, Credit
        -   `InventoryTransactionType` - All transaction types
    -   **Documentation**:
        -   `ACCOUNTING_SYSTEM.md` - Complete system overview, principles, and examples
        -   `ACCOUNTING_INTEGRATION_GUIDE.md` - Integration guide with code examples
        -   SQL initialization script with standard Brazilian chart of accounts
        -   Updated README.md with accounting features

### Features

-   **Double-Entry Bookkeeping**: Every transaction creates balanced debits and credits
-   **Compliance**: Follows NBC TG 16 (Inventory), NBC TG 26 (Financial Statements), Lei 6.404/76
-   **Audit Trail**: Complete traceability with user, timestamp, and document references
-   **Automatic Journal Entries**: No manual accounting entries needed
-   **Flexible Chart of Accounts**: Hierarchical structure with custom accounts support
-   **Cost Tracking**: Unit cost and total cost calculation for all transactions
-   **Multi-Location**: Support for multiple warehouse locations
-   **Document References**: Links to invoices, orders, and fiscal documents
-   **Reconciliation Ready**: Easy reconciliation between physical and accounting inventory

### Database Schema

-   4 new tables created: `ChartOfAccounts`, `JournalEntries`, `AccountingEntries`, `InventoryTransactions`
-   **ChartOfAccounts Table**:
    -   Hierarchical account structure with parent-child relationships
    -   Unique account codes with indexing
    -   Balance tracking with 18,2 decimal precision
    -   Support for analytic and synthetic accounts
-   **JournalEntries Table**:
    -   Unique entry numbers for audit trail
    -   Document type and number tracking
    -   Post/unpost functionality with timestamps
    -   Links to orders, products, and inventory transactions
-   **AccountingEntries Table**:
    -   Individual debit/credit entries
    -   Cost center support for management accounting
    -   Foreign keys with cascade and restrict behaviors
-   **InventoryTransactions Table**:
    -   10 transaction types with unique transaction numbers
    -   Multi-location support (from/to locations)
    -   Automatic cost calculation (unit cost × quantity)
    -   Links to products, orders, and journal entries
-   Comprehensive indexes for performance optimization
-   Full referential integrity with proper foreign key constraints

### Changed

### Fixed

## [0.0.8] - 2025-10-27

### Added

-   **Comprehensive Logging System**: Complete logging infrastructure across the entire application
    -   `ILoggingService` interface in `Application/Interfaces` for centralized logging operations
    -   `LoggingService<T>` implementation in `Application/Services` using Microsoft.Extensions.Logging
    -   Support for all log levels: Debug, Information, Warning, Error, Critical
-   **Global Exception Handling Middleware**:
    -   `ExceptionHandlingMiddleware` in `API/Middlewares` for catching all unhandled exceptions
    -   Automatic logging of exceptions with contextual information (HTTP path, method, status code)
    -   Standardized error responses using ProblemDetails (RFC 7807)
    -   Intelligent exception-to-HTTP status code mapping:
        -   ArgumentNullException / ArgumentException / InvalidOperationException → 400 Bad Request
        -   UnauthorizedAccessException → 401 Unauthorized
        -   KeyNotFoundException → 404 Not Found
        -   Other exceptions → 500 Internal Server Error
-   **Logging in Controllers**:
    -   `AuthController`: Login attempts, authentication successes and failures, invalid credentials warnings
    -   `UserController`: All CRUD operations, validation errors, duplicate resource warnings, resource not found warnings
    -   `ProductController`: All CRUD operations, soft deletes, validation errors, SKU conflicts, resource not found warnings
-   **Logging in Application Services**:
    -   `JwtService`: Token generation operations, configuration initialization, critical configuration errors
    -   `PasswordService`: Password hashing operations, password verification, validation warnings
-   **Logging in Repositories**:
    -   `UserRepository`: Database CRUD operations, query operations, error logging for persistence failures
    -   `ProductRepository`: Database CRUD operations, soft delete operations, error logging for persistence failures
-   **Logging Configuration**:
    -   Console and Debug logging providers configured in `Program.cs`
    -   Environment-specific log levels (Debug for Development, Information for Production)
    -   Structured logging configuration in `appsettings.json` and `appsettings.Development.json`
    -   Timestamp formatting: "yyyy-MM-dd HH:mm:ss"
    -   Scope inclusion enabled for better traceability
    -   Namespace-specific log level configuration:
        -   Production: ECommerce._ → Information, Microsoft._ → Warning
        -   Development: ECommerce._ → Debug, Microsoft._ → Information

### Changed

-   Updated `Program.cs` to include comprehensive logging configuration and exception handling middleware
-   Enhanced all API controllers to inject `ILogger<T>` for request/response logging
-   Updated all application services to include error and warning logging
-   Enhanced all repositories with debug and error logging for database operations
-   Improved error handling with contextual information throughout the application

## [0.0.7] - 2025-10-27

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

## [0.0.6] - 2025-10-27

### Changed

-   Empty migration (no database changes)
-   Build version increment only

## [0.0.5] - 2025-10-27

### Changed

-   Empty migration (no database changes)
-   Build version increment only

## [0.0.4] - 2025-10-27

### Changed

-   Empty migration (no database changes)
-   Build version increment only

## [0.0.3] - 2025-10-27

### Changed

-   Empty migration (no database changes)
-   Build version increment only

## [0.0.2] - 2025-10-27

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

## [0.0.1] - 2025-10-27

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

-   **0.1.17** - Unit Tests & Documentation Updates
-   **0.0.10** - Complete E-Commerce Domain Entities
-   **0.0.9** - Integrated Accounting System
-   **0.0.8** - Comprehensive Logging System
-   **0.0.7** - Product Management System
-   **0.0.6** - Build version increment
-   **0.0.5** - Build version increment
-   **0.0.4** - Build version increment
-   **0.0.3** - Build version increment
-   **0.0.2** - User Management & Authentication
-   **0.0.1** - Initial Setup

[Unreleased]: https://github.com/mgnischor/ecommerce-backend/compare/v0.1.17...HEAD
[0.1.17]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.10...v0.1.17
[0.0.10]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.9...v0.0.10
[0.0.9]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.8...v0.0.9
[0.0.8]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.7...v0.0.8
[0.0.7]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.6...v0.0.7
[0.0.6]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.5...v0.0.6
[0.0.5]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.4...v0.0.5
[0.0.4]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.3...v0.0.4
[0.0.3]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.2...v0.0.3
[0.0.2]: https://github.com/mgnischor/ecommerce-backend/compare/v0.0.1...v0.0.2
[0.0.1]: https://github.com/mgnischor/ecommerce-backend/releases/tag/v0.0.1
