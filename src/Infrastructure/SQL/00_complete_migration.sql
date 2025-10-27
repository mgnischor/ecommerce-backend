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
    VALUES ('20251027025613_Migration_v0_0_2', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027032829_Migration_v0_0_3') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027032829_Migration_v0_0_3', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027033034_Migration_v0_0_4') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027033034_Migration_v0_0_4', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027033118_Migration_v0_0_5') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027033118_Migration_v0_0_5', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027034435_Migration_v0_0_6') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027034435_Migration_v0_0_6', '9.0.10');
    END IF;
END $EF$;

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
    VALUES ('20251027121552_Migration_v0_0_7', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027124850_Migration_v0_0_8') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027124850_Migration_v0_0_8', '9.0.10');
    END IF;
END $EF$;

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
    VALUES ('20251027154509_Migration_v0_0_9', '9.0.10');
    END IF;
END $EF$;

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
    VALUES ('20251027174620_Migration_v0_0_10', '9.0.10');
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20251027182346_Migration_v0_0_11') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20251027182346_Migration_v0_0_11', '9.0.10');
    END IF;
END $EF$;
COMMIT;

