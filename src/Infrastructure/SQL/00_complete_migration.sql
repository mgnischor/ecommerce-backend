CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027025613_Migration_v0_0_2') THEN
    CREATE TABLE public.users (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        "UpdatedBy" uuid NOT NULL,
        "AccessLevel" integer NOT NULL,
        address character varying(500) NOT NULL,
        city character varying(100) NOT NULL,
        country character varying(100) NOT NULL,
        email character varying(255) NOT NULL,
        password_hash character varying(256) NOT NULL,
        username character varying(50) NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsBanned" boolean NOT NULL,
        "IsDebugEnabled" boolean NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "IsEmailVerified" boolean NOT NULL,
        "Groups" uuid[] NOT NULL,
        "FavoriteProducts" uuid[] NOT NULL,
        birth_date date NOT NULL,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_users" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027025613_Migration_v0_0_2') THEN
    CREATE INDEX ix_users_created_at ON public.users (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027025613_Migration_v0_0_2') THEN
    CREATE UNIQUE INDEX ix_users_email ON public.users (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027025613_Migration_v0_0_2') THEN
    CREATE UNIQUE INDEX ix_users_username ON public.users (username);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027025613_Migration_v0_0_2') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027025613_Migration_v0_0_2', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027032829_Migration_v0_0_3') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027032829_Migration_v0_0_3', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027033034_Migration_v0_0_4') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027033034_Migration_v0_0_4', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027033118_Migration_v0_0_5') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027033118_Migration_v0_0_5', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027034435_Migration_v0_0_6') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027034435_Migration_v0_0_6', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    CREATE TABLE public.products (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        category integer NOT NULL DEFAULT (0),
        status integer NOT NULL DEFAULT (1),
        name character varying(200) NOT NULL,
        description character varying(2000) NOT NULL,
        sku character varying(50) NOT NULL,
        brand character varying(100) NOT NULL,
        image_url character varying(500) NOT NULL,
        price numeric(18,2) NOT NULL,
        discount_price numeric(18,2),
        weight numeric(10,2) NOT NULL DEFAULT 0.0,
        stock_quantity integer NOT NULL DEFAULT 0,
        min_stock_level integer NOT NULL DEFAULT 0,
        max_order_quantity integer NOT NULL DEFAULT 100,
        is_active boolean NOT NULL DEFAULT TRUE,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        is_featured boolean NOT NULL DEFAULT FALSE,
        is_on_sale boolean NOT NULL DEFAULT FALSE,
        tags text[] NOT NULL DEFAULT ('{}'),
        images text[] NOT NULL DEFAULT ('{}'),
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_products" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    CREATE INDEX ix_products_category ON public.products (category);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    CREATE INDEX ix_products_created_at ON public.products (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    CREATE INDEX ix_products_is_featured ON public.products (is_featured);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    CREATE INDEX ix_products_is_on_sale ON public.products (is_on_sale);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    CREATE INDEX ix_products_name ON public.products (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    CREATE UNIQUE INDEX ix_products_sku ON public.products (sku);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    CREATE INDEX ix_products_status ON public.products (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027121552_Migration_v0_0_7') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027121552_Migration_v0_0_7', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027124850_Migration_v0_0_8') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027124850_Migration_v0_0_8', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE TABLE "ChartOfAccounts" (
        "Id" uuid NOT NULL,
        "AccountCode" character varying(20) NOT NULL,
        "AccountName" character varying(200) NOT NULL,
        "Description" character varying(500),
        "AccountType" integer NOT NULL,
        "ParentAccountId" uuid,
        "IsAnalytic" boolean NOT NULL DEFAULT TRUE,
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "Balance" numeric(18,2) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_ChartOfAccounts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE TABLE "JournalEntries" (
        "Id" uuid NOT NULL,
        "EntryNumber" character varying(50) NOT NULL,
        "EntryDate" timestamp with time zone NOT NULL,
        "DocumentType" character varying(50) NOT NULL,
        "DocumentNumber" character varying(100),
        "History" character varying(1000) NOT NULL,
        "TotalAmount" numeric(18,2) NOT NULL,
        "OrderId" uuid,
        "ProductId" uuid,
        "InventoryTransactionId" uuid,
        "IsPosted" boolean NOT NULL DEFAULT FALSE,
        "PostedAt" timestamp with time zone,
        "CreatedBy" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_JournalEntries" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE TABLE "AccountingEntries" (
        "Id" uuid NOT NULL,
        "JournalEntryId" uuid NOT NULL,
        "AccountId" uuid NOT NULL,
        "EntryType" integer NOT NULL,
        "Amount" numeric(18,2) NOT NULL,
        "Description" character varying(500),
        "CostCenter" character varying(100),
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AccountingEntries" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_AccountingEntries_ChartOfAccounts_AccountId" FOREIGN KEY ("AccountId") REFERENCES "ChartOfAccounts" ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_AccountingEntries_JournalEntries_JournalEntryId" FOREIGN KEY ("JournalEntryId") REFERENCES "JournalEntries" ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE TABLE "InventoryTransactions" (
        "Id" uuid NOT NULL,
        "TransactionNumber" character varying(50) NOT NULL,
        "TransactionDate" timestamp with time zone NOT NULL,
        "TransactionType" integer NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductSku" character varying(100) NOT NULL,
        "ProductName" character varying(200) NOT NULL,
        "FromLocation" character varying(100),
        "ToLocation" character varying(100) NOT NULL,
        "Quantity" integer NOT NULL,
        "UnitCost" numeric(18,2) NOT NULL,
        "TotalCost" numeric(18,2) NOT NULL,
        "OrderId" uuid,
        "DocumentNumber" character varying(100),
        "Notes" character varying(1000),
        "JournalEntryId" uuid,
        "CreatedBy" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_InventoryTransactions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_InventoryTransactions_JournalEntries_JournalEntryId" FOREIGN KEY ("JournalEntryId") REFERENCES "JournalEntries" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_InventoryTransactions_products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public.products (id) ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_AccountingEntries_AccountId" ON "AccountingEntries" ("AccountId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_AccountingEntries_EntryType" ON "AccountingEntries" ("EntryType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_AccountingEntries_JournalEntryId" ON "AccountingEntries" ("JournalEntryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE UNIQUE INDEX "IX_ChartOfAccounts_AccountCode" ON "ChartOfAccounts" ("AccountCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_ChartOfAccounts_AccountType" ON "ChartOfAccounts" ("AccountType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_ChartOfAccounts_IsActive" ON "ChartOfAccounts" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_InventoryTransactions_CreatedBy" ON "InventoryTransactions" ("CreatedBy");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_InventoryTransactions_JournalEntryId" ON "InventoryTransactions" ("JournalEntryId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_InventoryTransactions_OrderId" ON "InventoryTransactions" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_InventoryTransactions_ProductId" ON "InventoryTransactions" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_InventoryTransactions_TransactionDate" ON "InventoryTransactions" ("TransactionDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE UNIQUE INDEX "IX_InventoryTransactions_TransactionNumber" ON "InventoryTransactions" ("TransactionNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_InventoryTransactions_TransactionType" ON "InventoryTransactions" ("TransactionType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_JournalEntries_DocumentType" ON "JournalEntries" ("DocumentType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_JournalEntries_EntryDate" ON "JournalEntries" ("EntryDate");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE UNIQUE INDEX "IX_JournalEntries_EntryNumber" ON "JournalEntries" ("EntryNumber");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_JournalEntries_InventoryTransactionId" ON "JournalEntries" ("InventoryTransactionId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_JournalEntries_IsPosted" ON "JournalEntries" ("IsPosted");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_JournalEntries_OrderId" ON "JournalEntries" ("OrderId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    CREATE INDEX "IX_JournalEntries_ProductId" ON "JournalEntries" ("ProductId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027154509_Migration_v0_0_9') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027154509_Migration_v0_0_9', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Addresses" (
        "Id" uuid NOT NULL,
        "CustomerId" uuid NOT NULL,
        "AddressType" text NOT NULL,
        "FullName" text NOT NULL,
        "PhoneNumber" text NOT NULL,
        "AddressLine1" text NOT NULL,
        "AddressLine2" text,
        "City" text NOT NULL,
        "State" text NOT NULL,
        "PostalCode" text NOT NULL,
        "Country" text NOT NULL,
        "IsDefault" boolean NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Addresses" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "CartItems" (
        "Id" uuid NOT NULL,
        "CartId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Quantity" integer NOT NULL,
        "UnitPrice" numeric NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_CartItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Carts" (
        "Id" uuid NOT NULL,
        "CustomerId" uuid NOT NULL,
        "SessionId" text,
        "CouponCode" text,
        "DiscountAmount" numeric NOT NULL,
        "IsActive" boolean NOT NULL,
        "ExpiresAt" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Carts" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Categories" (
        "Id" uuid NOT NULL,
        "CreatedBy" uuid NOT NULL,
        "UpdatedBy" uuid,
        "ParentCategoryId" uuid,
        "Name" text NOT NULL,
        "Slug" text NOT NULL,
        "Description" text,
        "ImageUrl" text,
        "DisplayOrder" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "MetaTitle" text,
        "MetaDescription" text,
        "MetaKeywords" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Coupons" (
        "Id" uuid NOT NULL,
        "CreatedBy" uuid NOT NULL,
        "UpdatedBy" uuid,
        "Code" text NOT NULL,
        "Description" text NOT NULL,
        "DiscountType" text NOT NULL,
        "DiscountValue" numeric NOT NULL,
        "MinimumOrderAmount" numeric,
        "MaximumDiscountAmount" numeric,
        "MaxUsageCount" integer,
        "UsageCount" integer NOT NULL,
        "MaxUsagePerCustomer" integer,
        "ApplicableProductIds" uuid[],
        "ApplicableCategoryIds" integer[],
        "ValidFrom" timestamp with time zone,
        "ValidUntil" timestamp with time zone,
        "IsActive" boolean NOT NULL,
        "IsDeleted" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Coupons" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Inventories" (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Location" text NOT NULL,
        "QuantityInStock" integer NOT NULL,
        "QuantityReserved" integer NOT NULL,
        "QuantityAvailable" integer NOT NULL,
        "ReorderLevel" integer NOT NULL,
        "ReorderQuantity" integer NOT NULL,
        "LastStockReceived" timestamp with time zone,
        "LastInventoryCount" timestamp with time zone,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Inventories" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "OrderItems" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "ProductName" text NOT NULL,
        "ProductSku" text NOT NULL,
        "Quantity" integer NOT NULL,
        "UnitPrice" numeric NOT NULL,
        "DiscountAmount" numeric NOT NULL,
        "TaxAmount" numeric NOT NULL,
        "TotalPrice" numeric NOT NULL,
        "ProductImageUrl" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Orders" (
        "Id" uuid NOT NULL,
        "CreatedBy" uuid NOT NULL,
        "UpdatedBy" uuid,
        "CustomerId" uuid NOT NULL,
        "OrderNumber" text NOT NULL,
        "Status" integer NOT NULL,
        "SubTotal" numeric NOT NULL,
        "TaxAmount" numeric NOT NULL,
        "ShippingCost" numeric NOT NULL,
        "DiscountAmount" numeric NOT NULL,
        "TotalAmount" numeric NOT NULL,
        "PaymentMethod" integer NOT NULL,
        "ShippingMethod" integer NOT NULL,
        "ShippingAddressId" uuid,
        "BillingAddressId" uuid,
        "CouponCode" text,
        "CustomerNotes" text,
        "AdminNotes" text,
        "TrackingNumber" text,
        "ExpectedDeliveryDate" timestamp with time zone,
        "DeliveredAt" timestamp with time zone,
        "CancelledAt" timestamp with time zone,
        "CancellationReason" text,
        "IsDeleted" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Payments" (
        "Id" uuid NOT NULL,
        "OrderId" uuid NOT NULL,
        "TransactionId" text,
        "PaymentMethod" integer NOT NULL,
        "Status" integer NOT NULL,
        "Amount" numeric NOT NULL,
        "Currency" text NOT NULL,
        "PaymentProvider" text,
        "ProviderResponse" text,
        "ErrorMessage" text,
        "AuthorizedAt" timestamp with time zone,
        "CapturedAt" timestamp with time zone,
        "RefundedAt" timestamp with time zone,
        "RefundAmount" numeric,
        "RefundReason" text,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Payments" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Reviews" (
        "Id" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "CustomerId" uuid NOT NULL,
        "OrderId" uuid,
        "Rating" integer NOT NULL,
        "Title" text NOT NULL,
        "Comment" text NOT NULL,
        "IsVerifiedPurchase" boolean NOT NULL,
        "IsApproved" boolean NOT NULL,
        "IsFlagged" boolean NOT NULL,
        "HelpfulCount" integer NOT NULL,
        "NotHelpfulCount" integer NOT NULL,
        "AdminResponse" text,
        "AdminRespondedAt" timestamp with time zone,
        "IsDeleted" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Reviews" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "WishlistItems" (
        "Id" uuid NOT NULL,
        "WishlistId" uuid NOT NULL,
        "ProductId" uuid NOT NULL,
        "Notes" text,
        "Priority" integer NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_WishlistItems" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    CREATE TABLE "Wishlists" (
        "Id" uuid NOT NULL,
        "CustomerId" uuid NOT NULL,
        "Name" text NOT NULL,
        "IsPublic" boolean NOT NULL,
        "IsDefault" boolean NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_Wishlists" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027174620_Migration_v0_0_10') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027174620_Migration_v0_0_10', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027182346_Migration_v0_0_11') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027182346_Migration_v0_0_11', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027194648_Migration_v0_0_12') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027194648_Migration_v0_0_12', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251028011615_Migration_v0_0_13') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251028011615_Migration_v0_0_13', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.notifications (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        user_id uuid NOT NULL,
        type integer NOT NULL DEFAULT 0,
        title character varying(200) NOT NULL,
        message character varying(2000) NOT NULL,
        action_url character varying(500),
        icon character varying(100),
        related_entity_id uuid,
        related_entity_type character varying(50),
        is_read boolean NOT NULL DEFAULT FALSE,
        read_at timestamp with time zone,
        send_email boolean NOT NULL DEFAULT FALSE,
        email_sent boolean NOT NULL DEFAULT FALSE,
        email_sent_at timestamp with time zone,
        send_push boolean NOT NULL DEFAULT FALSE,
        push_sent boolean NOT NULL DEFAULT FALSE,
        push_sent_at timestamp with time zone,
        priority integer NOT NULL DEFAULT 5,
        expires_at timestamp with time zone,
        metadata jsonb,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_notifications" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.product_attributes (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        name character varying(100) NOT NULL,
        code character varying(50) NOT NULL,
        description character varying(1000),
        data_type character varying(50) NOT NULL DEFAULT 'Text',
        possible_values text[] NOT NULL DEFAULT ('{}'),
        default_value character varying(500),
        is_required boolean NOT NULL DEFAULT FALSE,
        is_variant_attribute boolean NOT NULL DEFAULT FALSE,
        is_filterable boolean NOT NULL DEFAULT TRUE,
        is_searchable boolean NOT NULL DEFAULT TRUE,
        is_visible_on_product_page boolean NOT NULL DEFAULT TRUE,
        display_order integer NOT NULL DEFAULT 0,
        unit character varying(20),
        validation_pattern character varying(500),
        applicable_category_ids uuid[] NOT NULL DEFAULT ('{}'),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_product_attributes" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.product_variants (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        product_id uuid NOT NULL,
        name character varying(200) NOT NULL,
        sku character varying(50) NOT NULL,
        barcode character varying(100),
        price numeric(18,2) NOT NULL,
        discount_price numeric(18,2),
        stock_quantity integer NOT NULL DEFAULT 0,
        weight numeric(10,2),
        image_url character varying(500),
        images text[] NOT NULL DEFAULT ('{}'),
        attributes jsonb NOT NULL DEFAULT ('{}'),
        is_default boolean NOT NULL DEFAULT FALSE,
        is_available boolean NOT NULL DEFAULT TRUE,
        display_order integer NOT NULL DEFAULT 0,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_product_variants" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.promotions (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        name character varying(200) NOT NULL,
        description character varying(2000) NOT NULL,
        type integer NOT NULL DEFAULT 0,
        code character varying(50),
        discount_percentage numeric(5,2),
        discount_amount numeric(18,2),
        minimum_order_amount numeric(18,2),
        maximum_discount_amount numeric(18,2),
        start_date timestamp with time zone NOT NULL,
        end_date timestamp with time zone NOT NULL,
        max_usage_count integer,
        usage_count integer NOT NULL DEFAULT 0,
        max_usage_per_user integer,
        eligible_product_ids uuid[] NOT NULL DEFAULT ('{}'),
        eligible_category_ids uuid[] NOT NULL DEFAULT ('{}'),
        eligible_user_ids uuid[] NOT NULL DEFAULT ('{}'),
        priority integer NOT NULL DEFAULT 0,
        is_combinable boolean NOT NULL DEFAULT TRUE,
        is_active boolean NOT NULL DEFAULT TRUE,
        is_featured boolean NOT NULL DEFAULT FALSE,
        banner_url character varying(500),
        terms_and_conditions character varying(5000),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_promotions" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.refunds (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        order_id uuid NOT NULL,
        customer_id uuid NOT NULL,
        payment_id uuid,
        refund_number character varying(50) NOT NULL,
        status integer NOT NULL DEFAULT 0,
        refund_amount numeric(18,2) NOT NULL,
        reason character varying(500) NOT NULL,
        customer_notes character varying(2000),
        admin_notes character varying(2000),
        order_item_ids uuid[] NOT NULL DEFAULT ('{}'),
        requires_return boolean NOT NULL DEFAULT TRUE,
        return_tracking_number character varying(100),
        returned_at timestamp with time zone,
        approved_at timestamp with time zone,
        approved_by uuid,
        processed_at timestamp with time zone,
        completed_at timestamp with time zone,
        rejection_reason character varying(500),
        transaction_id character varying(100),
        restocking_fee numeric(18,2),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_refunds" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.shipments (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        order_id uuid NOT NULL,
        shipping_address_id uuid NOT NULL,
        tracking_number character varying(100) NOT NULL,
        carrier character varying(100) NOT NULL,
        service_type character varying(100) NOT NULL,
        status integer NOT NULL DEFAULT 0,
        shipping_cost numeric(18,2) NOT NULL DEFAULT 0.0,
        weight numeric(10,2) NOT NULL DEFAULT 0.0,
        length numeric(10,2),
        width numeric(10,2),
        height numeric(10,2),
        expected_delivery_date timestamp with time zone,
        delivered_at timestamp with time zone,
        shipped_at timestamp with time zone,
        tracking_url character varying(500),
        notes character varying(1000),
        is_insured boolean NOT NULL DEFAULT FALSE,
        insurance_amount numeric(18,2),
        requires_signature boolean NOT NULL DEFAULT FALSE,
        received_by character varying(200),
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_shipments" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.shipping_zones (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        name character varying(200) NOT NULL,
        description character varying(1000),
        countries text[] NOT NULL DEFAULT ('{}'),
        states text[] NOT NULL DEFAULT ('{}'),
        postal_codes text[] NOT NULL DEFAULT ('{}'),
        base_rate numeric(18,2) NOT NULL,
        rate_per_kg numeric(18,2),
        rate_per_item numeric(18,2),
        free_shipping_threshold numeric(18,2),
        minimum_order_amount numeric(18,2),
        maximum_order_amount numeric(18,2),
        estimated_delivery_days_min integer NOT NULL DEFAULT 1,
        estimated_delivery_days_max integer NOT NULL DEFAULT 7,
        available_shipping_methods text[] NOT NULL DEFAULT ('{}'),
        tax_rate numeric(5,2),
        priority integer NOT NULL DEFAULT 0,
        is_active boolean NOT NULL DEFAULT TRUE,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_shipping_zones" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.stores (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        name character varying(200) NOT NULL,
        store_code character varying(50) NOT NULL,
        description character varying(2000),
        email character varying(256) NOT NULL,
        phone_number character varying(50) NOT NULL,
        address character varying(500) NOT NULL,
        city character varying(100) NOT NULL,
        state character varying(100) NOT NULL,
        postal_code character varying(20) NOT NULL,
        country character varying(100) NOT NULL,
        latitude numeric(10,7),
        longitude numeric(10,7),
        manager_id uuid,
        opening_hours jsonb,
        timezone character varying(50) NOT NULL DEFAULT 'UTC',
        currency character varying(3) NOT NULL DEFAULT 'USD',
        logo_url character varying(500),
        image_url character varying(500),
        is_default boolean NOT NULL DEFAULT FALSE,
        supports_pickup boolean NOT NULL DEFAULT TRUE,
        supports_delivery boolean NOT NULL DEFAULT TRUE,
        is_active boolean NOT NULL DEFAULT TRUE,
        display_order integer NOT NULL DEFAULT 0,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_stores" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.suppliers (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        company_name character varying(200) NOT NULL,
        supplier_code character varying(50) NOT NULL,
        contact_name character varying(200) NOT NULL,
        email character varying(256) NOT NULL,
        phone_number character varying(50) NOT NULL,
        alternate_phone character varying(50),
        fax_number character varying(50),
        website character varying(500),
        address character varying(500) NOT NULL,
        city character varying(100) NOT NULL,
        state character varying(100) NOT NULL,
        postal_code character varying(20) NOT NULL,
        country character varying(100) NOT NULL,
        tax_id character varying(50),
        registration_number character varying(50),
        bank_account_number character varying(50),
        bank_name character varying(100),
        bank_routing_number character varying(50),
        payment_terms character varying(100),
        credit_limit numeric(18,2),
        current_balance numeric(18,2) NOT NULL DEFAULT 0.0,
        rating numeric(3,2) NOT NULL DEFAULT 0.0,
        lead_time_days integer NOT NULL DEFAULT 0,
        minimum_order_amount numeric(18,2),
        notes character varying(2000),
        is_active boolean NOT NULL DEFAULT TRUE,
        is_preferred boolean NOT NULL DEFAULT FALSE,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_suppliers" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE TABLE public.vendors (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        user_id uuid NOT NULL,
        business_name character varying(200) NOT NULL,
        store_name character varying(200) NOT NULL,
        email character varying(256) NOT NULL,
        phone_number character varying(50) NOT NULL,
        description character varying(2000),
        logo_url character varying(500),
        banner_url character varying(500),
        address character varying(500) NOT NULL,
        city character varying(100) NOT NULL,
        state character varying(100) NOT NULL,
        postal_code character varying(20) NOT NULL,
        country character varying(100) NOT NULL,
        tax_id character varying(50),
        registration_number character varying(50),
        commission_rate numeric(5,2) NOT NULL DEFAULT 10.0,
        rating numeric(3,2) NOT NULL DEFAULT 0.0,
        total_ratings integer NOT NULL DEFAULT 0,
        total_sales numeric(18,2) NOT NULL DEFAULT 0.0,
        total_orders integer NOT NULL DEFAULT 0,
        status integer NOT NULL DEFAULT 0,
        bank_account_number character varying(50),
        bank_name character varying(100),
        bank_routing_number character varying(50),
        paypal_email character varying(256),
        is_verified boolean NOT NULL DEFAULT FALSE,
        verified_at timestamp with time zone,
        is_featured boolean NOT NULL DEFAULT FALSE,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_vendors" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_notifications_created_at ON public.notifications (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_notifications_is_read ON public.notifications (is_read);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_notifications_priority ON public.notifications (priority);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_notifications_type ON public.notifications (type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_notifications_user_id ON public.notifications (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_notifications_user_id_is_read ON public.notifications (user_id, is_read);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE UNIQUE INDEX ix_product_attributes_code ON public.product_attributes (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_product_attributes_display_order ON public.product_attributes (display_order);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_product_attributes_is_filterable ON public.product_attributes (is_filterable);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_product_attributes_is_variant_attribute ON public.product_attributes (is_variant_attribute);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_product_attributes_name ON public.product_attributes (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_product_variants_display_order ON public.product_variants (display_order);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_product_variants_is_available ON public.product_variants (is_available);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_product_variants_is_default ON public.product_variants (is_default);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_product_variants_product_id ON public.product_variants (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE UNIQUE INDEX ix_product_variants_sku ON public.product_variants (sku);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_promotions_active_dates ON public.promotions (start_date, end_date, is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_promotions_code ON public.promotions (code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_promotions_end_date ON public.promotions (end_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_promotions_is_active ON public.promotions (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_promotions_is_featured ON public.promotions (is_featured);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_promotions_start_date ON public.promotions (start_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_promotions_type ON public.promotions (type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_refunds_created_at ON public.refunds (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_refunds_customer_id ON public.refunds (customer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_refunds_order_id ON public.refunds (order_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE UNIQUE INDEX ix_refunds_refund_number ON public.refunds (refund_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_refunds_status ON public.refunds (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_shipments_created_at ON public.shipments (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_shipments_delivered_at ON public.shipments (delivered_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_shipments_order_id ON public.shipments (order_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_shipments_status ON public.shipments (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_shipments_tracking_number ON public.shipments (tracking_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_shipping_zones_is_active ON public.shipping_zones (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_shipping_zones_name ON public.shipping_zones (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_shipping_zones_priority ON public.shipping_zones (priority);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_stores_city ON public.stores (city);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_stores_country ON public.stores (country);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_stores_display_order ON public.stores (display_order);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_stores_is_active ON public.stores (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_stores_is_default ON public.stores (is_default);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_stores_name ON public.stores (name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE UNIQUE INDEX ix_stores_store_code ON public.stores (store_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_suppliers_company_name ON public.suppliers (company_name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_suppliers_email ON public.suppliers (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_suppliers_is_active ON public.suppliers (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_suppliers_is_preferred ON public.suppliers (is_preferred);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_suppliers_rating ON public.suppliers (rating);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE UNIQUE INDEX ix_suppliers_supplier_code ON public.suppliers (supplier_code);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_vendors_created_at ON public.vendors (created_at);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_vendors_email ON public.vendors (email);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_vendors_is_featured ON public.vendors (is_featured);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_vendors_rating ON public.vendors (rating);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_vendors_status ON public.vendors (status);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE INDEX ix_vendors_store_name ON public.vendors (store_name);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    CREATE UNIQUE INDEX ix_vendors_user_id ON public.vendors (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251103015250_Migration_v0_1_14') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251103015250_Migration_v0_1_14', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE TABLE financial_transactions (
        id uuid NOT NULL,
        transaction_number character varying(50) NOT NULL,
        transaction_type character varying(50) NOT NULL,
        amount numeric(18,2) NOT NULL,
        currency character varying(3) NOT NULL DEFAULT 'USD',
        transaction_date timestamp with time zone NOT NULL,
        description character varying(500) NOT NULL,
        order_id uuid,
        payment_id uuid,
        inventory_transaction_id uuid,
        journal_entry_id uuid,
        product_id uuid,
        counterparty character varying(200),
        reference_number character varying(100),
        is_reconciled boolean NOT NULL DEFAULT FALSE,
        reconciled_at timestamp with time zone,
        reconciled_by uuid,
        payment_method character varying(50),
        payment_provider character varying(100),
        status character varying(50) NOT NULL DEFAULT 'Pending',
        notes character varying(1000),
        tax_amount numeric(18,2) NOT NULL DEFAULT 0.0,
        fee_amount numeric(18,2) NOT NULL DEFAULT 0.0,
        net_amount numeric(18,2) NOT NULL,
        created_by uuid NOT NULL,
        created_at timestamp with time zone NOT NULL,
        updated_at timestamp with time zone NOT NULL,
        CONSTRAINT "PK_financial_transactions" PRIMARY KEY (id),
        CONSTRAINT "FK_financial_transactions_InventoryTransactions_inventory_tran~" FOREIGN KEY (inventory_transaction_id) REFERENCES "InventoryTransactions" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_financial_transactions_JournalEntries_journal_entry_id" FOREIGN KEY (journal_entry_id) REFERENCES "JournalEntries" ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_financial_transactions_Payments_payment_id" FOREIGN KEY (payment_id) REFERENCES "Payments" ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX ix_financial_transactions_counterparty ON financial_transactions (counterparty);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX ix_financial_transactions_date_type ON financial_transactions (transaction_date, transaction_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX ix_financial_transactions_inventory_transaction_id ON financial_transactions (inventory_transaction_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX ix_financial_transactions_is_reconciled ON financial_transactions (is_reconciled);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX "IX_financial_transactions_journal_entry_id" ON financial_transactions (journal_entry_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX ix_financial_transactions_order_id ON financial_transactions (order_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX ix_financial_transactions_payment_id ON financial_transactions (payment_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX ix_financial_transactions_transaction_date ON financial_transactions (transaction_date);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE UNIQUE INDEX ix_financial_transactions_transaction_number ON financial_transactions (transaction_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    CREATE INDEX ix_financial_transactions_transaction_type ON financial_transactions (transaction_type);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108215537_Migration_v0_1_15') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251108215537_Migration_v0_1_15', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251108220728_Migration_v0_1_16') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251108220728_Migration_v0_1_16', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE financial_transactions DROP CONSTRAINT "FK_financial_transactions_InventoryTransactions_inventory_tran~";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE financial_transactions DROP CONSTRAINT "FK_financial_transactions_JournalEntries_journal_entry_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE financial_transactions DROP CONSTRAINT "FK_financial_transactions_Payments_payment_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE "InventoryTransactions" DROP CONSTRAINT "FK_InventoryTransactions_products_ProductId";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.vendors DROP CONSTRAINT "PK_vendors";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.users DROP CONSTRAINT "PK_users";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.suppliers DROP CONSTRAINT "PK_suppliers";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.stores DROP CONSTRAINT "PK_stores";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.shipments DROP CONSTRAINT "PK_shipments";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.refunds DROP CONSTRAINT "PK_refunds";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.promotions DROP CONSTRAINT "PK_promotions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.products DROP CONSTRAINT "PK_products";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.notifications DROP CONSTRAINT "PK_notifications";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.shipping_zones DROP CONSTRAINT "PK_shipping_zones";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.product_variants DROP CONSTRAINT "PK_product_variants";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.product_attributes DROP CONSTRAINT "PK_product_attributes";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE financial_transactions DROP CONSTRAINT "PK_financial_transactions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.vendors RENAME TO "Vendors";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.users RENAME TO "Users";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.suppliers RENAME TO "Suppliers";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.stores RENAME TO "Stores";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.shipments RENAME TO "Shipments";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.refunds RENAME TO "Refunds";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.promotions RENAME TO "Promotions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.products RENAME TO "Products";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.notifications RENAME TO "Notifications";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.shipping_zones RENAME TO "ShippingZones";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.product_variants RENAME TO "ProductVariants";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public.product_attributes RENAME TO "ProductAttributes";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE financial_transactions RENAME TO "FinancialTransactions";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER INDEX "IX_financial_transactions_journal_entry_id" RENAME TO "IX_FinancialTransactions_journal_entry_id";
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Vendors" ADD CONSTRAINT "PK_Vendors" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Users" ADD CONSTRAINT "PK_Users" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Suppliers" ADD CONSTRAINT "PK_Suppliers" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Stores" ADD CONSTRAINT "PK_Stores" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Shipments" ADD CONSTRAINT "PK_Shipments" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Refunds" ADD CONSTRAINT "PK_Refunds" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Promotions" ADD CONSTRAINT "PK_Promotions" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Products" ADD CONSTRAINT "PK_Products" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."Notifications" ADD CONSTRAINT "PK_Notifications" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."ShippingZones" ADD CONSTRAINT "PK_ShippingZones" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."ProductVariants" ADD CONSTRAINT "PK_ProductVariants" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE public."ProductAttributes" ADD CONSTRAINT "PK_ProductAttributes" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "PK_FinancialTransactions" PRIMARY KEY (id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_InventoryTransactions_inventory_trans~" FOREIGN KEY (inventory_transaction_id) REFERENCES "InventoryTransactions" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_JournalEntries_journal_entry_id" FOREIGN KEY (journal_entry_id) REFERENCES "JournalEntries" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_Payments_payment_id" FOREIGN KEY (payment_id) REFERENCES "Payments" ("Id") ON DELETE SET NULL;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    ALTER TABLE "InventoryTransactions" ADD CONSTRAINT "FK_InventoryTransactions_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."Products" (id) ON DELETE RESTRICT;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251119001947_Migration_v0_1_17') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251119001947_Migration_v0_1_17', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    ALTER TABLE public."Users" ADD "FailedLoginAttempts" integer NOT NULL DEFAULT 0;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    ALTER TABLE public."Users" ADD "LastFailedLoginAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    ALTER TABLE public."Users" ADD "LastLoginIpAddress" text;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    ALTER TABLE public."Users" ADD "LastSuccessfulLoginAt" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    ALTER TABLE public."Users" ADD "LockedUntil" timestamp with time zone;
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    CREATE TABLE "AccountingRules" (
        "Id" uuid NOT NULL,
        "TransactionType" integer NOT NULL,
        "RuleCode" character varying(50) NOT NULL,
        "Description" character varying(500) NOT NULL,
        "DebitAccountCode" character varying(20) NOT NULL,
        "CreditAccountCode" character varying(20) NOT NULL,
        "Condition" character varying(200),
        "IsActive" boolean NOT NULL DEFAULT TRUE,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_AccountingRules" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    INSERT INTO "AccountingRules" ("Id", "Condition", "CreatedAt", "CreditAccountCode", "DebitAccountCode", "Description", "IsActive", "RuleCode", "TransactionType", "UpdatedAt")
    VALUES ('a0000000-0000-0000-0000-000000000001', NULL, TIMESTAMPTZ '2025-01-01T00:00:00Z', '2.1.01.001', '1.1.03.001', 'Purchase of inventory from suppliers', TRUE, 'PURCHASE', 1, TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "AccountingRules" ("Id", "Condition", "CreatedAt", "CreditAccountCode", "DebitAccountCode", "Description", "IsActive", "RuleCode", "TransactionType", "UpdatedAt")
    VALUES ('a0000000-0000-0000-0000-000000000002', NULL, TIMESTAMPTZ '2025-01-01T00:00:00Z', '1.1.03.001', '3.1.01.001', 'Sale of inventory to customers (COGS recognition)', TRUE, 'SALE', 2, TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "AccountingRules" ("Id", "Condition", "CreatedAt", "CreditAccountCode", "DebitAccountCode", "Description", "IsActive", "RuleCode", "TransactionType", "UpdatedAt")
    VALUES ('a0000000-0000-0000-0000-000000000003', NULL, TIMESTAMPTZ '2025-01-01T00:00:00Z', '3.1.01.001', '1.1.03.001', 'Customer returns inventory (COGS reversal)', TRUE, 'SALE_RETURN', 3, TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "AccountingRules" ("Id", "Condition", "CreatedAt", "CreditAccountCode", "DebitAccountCode", "Description", "IsActive", "RuleCode", "TransactionType", "UpdatedAt")
    VALUES ('a0000000-0000-0000-0000-000000000004', NULL, TIMESTAMPTZ '2025-01-01T00:00:00Z', '1.1.03.001', '2.1.01.001', 'Return of inventory to suppliers', TRUE, 'PURCHASE_RETURN', 4, TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "AccountingRules" ("Id", "Condition", "CreatedAt", "CreditAccountCode", "DebitAccountCode", "Description", "IsActive", "RuleCode", "TransactionType", "UpdatedAt")
    VALUES ('a0000000-0000-0000-0000-000000000005', 'Quantity > 0', TIMESTAMPTZ '2025-01-01T00:00:00Z', '4.2.01.001', '1.1.03.001', 'Positive inventory adjustment (overage)', TRUE, 'ADJUSTMENT_POSITIVE', 5, TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "AccountingRules" ("Id", "Condition", "CreatedAt", "CreditAccountCode", "DebitAccountCode", "Description", "IsActive", "RuleCode", "TransactionType", "UpdatedAt")
    VALUES ('a0000000-0000-0000-0000-000000000006', 'Quantity < 0', TIMESTAMPTZ '2025-01-01T00:00:00Z', '1.1.03.001', '3.2.01.002', 'Negative inventory adjustment (shortage)', TRUE, 'ADJUSTMENT_NEGATIVE', 5, TIMESTAMPTZ '2025-01-01T00:00:00Z');
    INSERT INTO "AccountingRules" ("Id", "Condition", "CreatedAt", "CreditAccountCode", "DebitAccountCode", "Description", "IsActive", "RuleCode", "TransactionType", "UpdatedAt")
    VALUES ('a0000000-0000-0000-0000-000000000007', NULL, TIMESTAMPTZ '2025-01-01T00:00:00Z', '1.1.03.001', '3.2.01.001', 'Inventory loss, shrinkage, or write-off', TRUE, 'LOSS', 7, TIMESTAMPTZ '2025-01-01T00:00:00Z');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN

                    INSERT INTO "ChartOfAccounts" ("Id", "AccountCode", "AccountName", "AccountType", "Balance", "CreatedAt", "Description", "IsActive", "IsAnalytic", "ParentAccountId", "UpdatedAt")
                    VALUES
                        ('10000000-0000-0000-0000-000000000001', '1.1.01.001', 'Cash and Cash Equivalents', 1, 0, '2025-01-01 00:00:00+00', 'Bank accounts and petty cash', true, true, NULL, '2025-01-01 00:00:00+00'),
                        ('10000000-0000-0000-0000-000000000002', '1.1.03.001', 'Inventory', 1, 0, '2025-01-01 00:00:00+00', 'Merchandise inventory for resale', true, true, NULL, '2025-01-01 00:00:00+00'),
                        ('20000000-0000-0000-0000-000000000001', '2.1.01.001', 'Accounts Payable - Suppliers', 2, 0, '2025-01-01 00:00:00+00', 'Amounts owed to suppliers for inventory purchases', true, true, NULL, '2025-01-01 00:00:00+00'),
                        ('30000000-0000-0000-0000-000000000001', '3.1.01.001', 'Cost of Goods Sold', 5, 0, '2025-01-01 00:00:00+00', 'Direct costs of goods sold to customers', true, true, NULL, '2025-01-01 00:00:00+00'),
                        ('30000000-0000-0000-0000-000000000002', '3.2.01.001', 'Inventory Loss', 5, 0, '2025-01-01 00:00:00+00', 'Inventory shrinkage, loss, and write-offs', true, true, NULL, '2025-01-01 00:00:00+00'),
                        ('30000000-0000-0000-0000-000000000003', '3.2.01.002', 'Other Operating Expenses', 5, 0, '2025-01-01 00:00:00+00', 'Miscellaneous operating expenses including negative inventory adjustments', true, true, NULL, '2025-01-01 00:00:00+00'),
                        ('40000000-0000-0000-0000-000000000001', '4.2.01.001', 'Other Operating Income', 4, 0, '2025-01-01 00:00:00+00', 'Miscellaneous income including positive inventory adjustments', true, true, NULL, '2025-01-01 00:00:00+00')
                    ON CONFLICT ("AccountCode") DO NOTHING;

    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    CREATE INDEX "IX_AccountingRules_IsActive" ON "AccountingRules" ("IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    CREATE UNIQUE INDEX "IX_AccountingRules_RuleCode" ON "AccountingRules" ("RuleCode");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    CREATE INDEX "IX_AccountingRules_TransactionType" ON "AccountingRules" ("TransactionType");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    CREATE INDEX "IX_AccountingRules_TransactionType_IsActive" ON "AccountingRules" ("TransactionType", "IsActive");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216135854_Migration_v0_1_18') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251216135854_Migration_v0_1_18', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216140010_Migration_v0_1_19') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251216140010_Migration_v0_1_19', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251216140158_Migration_v0_1_20') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251216140158_Migration_v0_1_20', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260222224509_Migration_v0_1_21') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260222224509_Migration_v0_1_21', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260301170852_Migration_v0_1_22') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260301170852_Migration_v0_1_22', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260410141623_Migration_v0_1_23') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260410141623_Migration_v0_1_23', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260614223559_Migration_v0_1_24') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260614223559_Migration_v0_1_24', '10.0.10');
    END IF;
END $EF$;
COMMIT;

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE TABLE public."Customers" (
        id uuid NOT NULL,
        user_id uuid NOT NULL,
        total_spending numeric(18,2) NOT NULL DEFAULT 0.0,
        total_orders integer NOT NULL DEFAULT 0,
        last_order_date timestamp with time zone,
        last_login_at timestamp with time zone,
        historical_average_order_days integer,
        is_active boolean NOT NULL DEFAULT TRUE,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_Customers" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE TABLE public."GiftCards" (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        card_number character varying(19) NOT NULL,
        balance numeric(18,2) NOT NULL DEFAULT 0.0,
        is_active boolean NOT NULL DEFAULT TRUE,
        is_revoked boolean NOT NULL DEFAULT FALSE,
        allows_reload boolean NOT NULL DEFAULT TRUE,
        issued_at timestamp with time zone NOT NULL,
        expires_at timestamp with time zone,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_GiftCards" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE TABLE public."InventoryPlans" (
        id uuid NOT NULL,
        product_id uuid NOT NULL,
        current_stock integer NOT NULL DEFAULT 0,
        reorder_point integer NOT NULL DEFAULT 0,
        average_daily_sales integer NOT NULL DEFAULT 0,
        lead_time_days integer,
        service_level_percent integer,
        received_at timestamp with time zone NOT NULL,
        cost_of_goods_sold numeric(18,2) NOT NULL DEFAULT 0.0,
        average_inventory_value numeric(18,2) NOT NULL DEFAULT 0.0,
        sales_in_last_90_days integer NOT NULL DEFAULT 0,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_InventoryPlans" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE TABLE public."Invoices" (
        id uuid NOT NULL,
        created_by uuid NOT NULL DEFAULT 'ce06e1a8-f688-44b6-b616-4badf09d9153',
        updated_by uuid,
        invoice_number character varying(50) NOT NULL,
        order_id uuid NOT NULL,
        customer_id uuid NOT NULL,
        subtotal numeric(18,2) NOT NULL DEFAULT 0.0,
        tax_amount numeric(18,2) NOT NULL DEFAULT 0.0,
        shipping_cost numeric(18,2) NOT NULL DEFAULT 0.0,
        total numeric(18,2) NOT NULL DEFAULT 0.0,
        paid_amount numeric(18,2) NOT NULL DEFAULT 0.0,
        due_date timestamp with time zone NOT NULL,
        issued_at timestamp with time zone NOT NULL,
        is_paid boolean NOT NULL DEFAULT FALSE,
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_Invoices" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE TABLE public."LoyaltyRewards" (
        id uuid NOT NULL,
        customer_id uuid NOT NULL,
        point_balance integer NOT NULL,
        total_points_earned integer NOT NULL,
        total_points_redeemed integer NOT NULL,
        last_earned_at timestamp with time zone,
        tier character varying(20) NOT NULL DEFAULT 'Bronze',
        is_deleted boolean NOT NULL DEFAULT FALSE,
        created_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        updated_at timestamp with time zone NOT NULL DEFAULT (CURRENT_TIMESTAMP),
        CONSTRAINT "PK_LoyaltyRewards" PRIMARY KEY (id)
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE INDEX ix_customers_is_active ON public."Customers" (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE UNIQUE INDEX ix_customers_user_id ON public."Customers" (user_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE UNIQUE INDEX ix_gift_cards_card_number ON public."GiftCards" (card_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE INDEX ix_gift_cards_is_active ON public."GiftCards" (is_active);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE INDEX ix_inventory_plans_is_deleted ON public."InventoryPlans" (is_deleted);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE INDEX ix_inventory_plans_product_id ON public."InventoryPlans" (product_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE INDEX ix_invoices_customer_id ON public."Invoices" (customer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE UNIQUE INDEX ix_invoices_invoice_number ON public."Invoices" (invoice_number);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE INDEX ix_invoices_is_paid ON public."Invoices" (is_paid);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE INDEX ix_invoices_order_id ON public."Invoices" (order_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    CREATE UNIQUE INDEX ix_loyalty_rewards_customer_id ON public."LoyaltyRewards" (customer_id);
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260807173003_Migration_v0_1_25') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260807173003_Migration_v0_1_25', '10.0.10');
    END IF;
END $EF$;
COMMIT;

