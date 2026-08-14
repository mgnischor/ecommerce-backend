START TRANSACTION;
ALTER TABLE public."Users" ADD "FailedLoginAttempts" integer NOT NULL DEFAULT 0;

ALTER TABLE public."Users" ADD "LastFailedLoginAt" timestamp with time zone;

ALTER TABLE public."Users" ADD "LastLoginIpAddress" text;

ALTER TABLE public."Users" ADD "LastSuccessfulLoginAt" timestamp with time zone;

ALTER TABLE public."Users" ADD "LockedUntil" timestamp with time zone;

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


CREATE INDEX "IX_AccountingRules_IsActive" ON "AccountingRules" ("IsActive");

CREATE UNIQUE INDEX "IX_AccountingRules_RuleCode" ON "AccountingRules" ("RuleCode");

CREATE INDEX "IX_AccountingRules_TransactionType" ON "AccountingRules" ("TransactionType");

CREATE INDEX "IX_AccountingRules_TransactionType_IsActive" ON "AccountingRules" ("TransactionType", "IsActive");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251216135854_Migration_v0_1_18', '10.0.10');

COMMIT;

