START TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251027124850_Migration_v0_0_8', '9.0.10');

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

CREATE INDEX "IX_AccountingEntries_AccountId" ON "AccountingEntries" ("AccountId");

CREATE INDEX "IX_AccountingEntries_EntryType" ON "AccountingEntries" ("EntryType");

CREATE INDEX "IX_AccountingEntries_JournalEntryId" ON "AccountingEntries" ("JournalEntryId");

CREATE UNIQUE INDEX "IX_ChartOfAccounts_AccountCode" ON "ChartOfAccounts" ("AccountCode");

CREATE INDEX "IX_ChartOfAccounts_AccountType" ON "ChartOfAccounts" ("AccountType");

CREATE INDEX "IX_ChartOfAccounts_IsActive" ON "ChartOfAccounts" ("IsActive");

CREATE INDEX "IX_InventoryTransactions_CreatedBy" ON "InventoryTransactions" ("CreatedBy");

CREATE INDEX "IX_InventoryTransactions_JournalEntryId" ON "InventoryTransactions" ("JournalEntryId");

CREATE INDEX "IX_InventoryTransactions_OrderId" ON "InventoryTransactions" ("OrderId");

CREATE INDEX "IX_InventoryTransactions_ProductId" ON "InventoryTransactions" ("ProductId");

CREATE INDEX "IX_InventoryTransactions_TransactionDate" ON "InventoryTransactions" ("TransactionDate");

CREATE UNIQUE INDEX "IX_InventoryTransactions_TransactionNumber" ON "InventoryTransactions" ("TransactionNumber");

CREATE INDEX "IX_InventoryTransactions_TransactionType" ON "InventoryTransactions" ("TransactionType");

CREATE INDEX "IX_JournalEntries_DocumentType" ON "JournalEntries" ("DocumentType");

CREATE INDEX "IX_JournalEntries_EntryDate" ON "JournalEntries" ("EntryDate");

CREATE UNIQUE INDEX "IX_JournalEntries_EntryNumber" ON "JournalEntries" ("EntryNumber");

CREATE INDEX "IX_JournalEntries_InventoryTransactionId" ON "JournalEntries" ("InventoryTransactionId");

CREATE INDEX "IX_JournalEntries_IsPosted" ON "JournalEntries" ("IsPosted");

CREATE INDEX "IX_JournalEntries_OrderId" ON "JournalEntries" ("OrderId");

CREATE INDEX "IX_JournalEntries_ProductId" ON "JournalEntries" ("ProductId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251027154509_Migration_v0_0_9', '9.0.10');

COMMIT;

