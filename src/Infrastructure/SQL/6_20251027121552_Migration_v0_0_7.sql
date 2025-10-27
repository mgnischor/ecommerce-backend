START TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251027034435_Migration_v0_0_6', '9.0.10');

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

CREATE INDEX ix_products_category ON public.products (category);

CREATE INDEX ix_products_created_at ON public.products (created_at);

CREATE INDEX ix_products_is_featured ON public.products (is_featured);

CREATE INDEX ix_products_is_on_sale ON public.products (is_on_sale);

CREATE INDEX ix_products_name ON public.products (name);

CREATE UNIQUE INDEX ix_products_sku ON public.products (sku);

CREATE INDEX ix_products_status ON public.products (status);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251027121552_Migration_v0_0_7', '9.0.10');

COMMIT;

