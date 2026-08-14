START TRANSACTION;
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

CREATE INDEX ix_financial_transactions_counterparty ON financial_transactions (counterparty);

CREATE INDEX ix_financial_transactions_date_type ON financial_transactions (transaction_date, transaction_type);

CREATE INDEX ix_financial_transactions_inventory_transaction_id ON financial_transactions (inventory_transaction_id);

CREATE INDEX ix_financial_transactions_is_reconciled ON financial_transactions (is_reconciled);

CREATE INDEX "IX_financial_transactions_journal_entry_id" ON financial_transactions (journal_entry_id);

CREATE INDEX ix_financial_transactions_order_id ON financial_transactions (order_id);

CREATE INDEX ix_financial_transactions_payment_id ON financial_transactions (payment_id);

CREATE INDEX ix_financial_transactions_transaction_date ON financial_transactions (transaction_date);

CREATE UNIQUE INDEX ix_financial_transactions_transaction_number ON financial_transactions (transaction_number);

CREATE INDEX ix_financial_transactions_transaction_type ON financial_transactions (transaction_type);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251108215537_Migration_v0_1_15', '10.0.10');

COMMIT;

