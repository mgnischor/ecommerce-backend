START TRANSACTION;
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

CREATE INDEX ix_customers_is_active ON public."Customers" (is_active);

CREATE UNIQUE INDEX ix_customers_user_id ON public."Customers" (user_id);

CREATE UNIQUE INDEX ix_gift_cards_card_number ON public."GiftCards" (card_number);

CREATE INDEX ix_gift_cards_is_active ON public."GiftCards" (is_active);

CREATE INDEX ix_inventory_plans_is_deleted ON public."InventoryPlans" (is_deleted);

CREATE INDEX ix_inventory_plans_product_id ON public."InventoryPlans" (product_id);

CREATE INDEX ix_invoices_customer_id ON public."Invoices" (customer_id);

CREATE UNIQUE INDEX ix_invoices_invoice_number ON public."Invoices" (invoice_number);

CREATE INDEX ix_invoices_is_paid ON public."Invoices" (is_paid);

CREATE INDEX ix_invoices_order_id ON public."Invoices" (order_id);

CREATE UNIQUE INDEX ix_loyalty_rewards_customer_id ON public."LoyaltyRewards" (customer_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260807173003_Migration_v0_1_25', '10.0.10');

COMMIT;

