START TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251028011615_Migration_v0_0_13', '9.0.10');

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

CREATE INDEX ix_notifications_created_at ON public.notifications (created_at);

CREATE INDEX ix_notifications_is_read ON public.notifications (is_read);

CREATE INDEX ix_notifications_priority ON public.notifications (priority);

CREATE INDEX ix_notifications_type ON public.notifications (type);

CREATE INDEX ix_notifications_user_id ON public.notifications (user_id);

CREATE INDEX ix_notifications_user_id_is_read ON public.notifications (user_id, is_read);

CREATE UNIQUE INDEX ix_product_attributes_code ON public.product_attributes (code);

CREATE INDEX ix_product_attributes_display_order ON public.product_attributes (display_order);

CREATE INDEX ix_product_attributes_is_filterable ON public.product_attributes (is_filterable);

CREATE INDEX ix_product_attributes_is_variant_attribute ON public.product_attributes (is_variant_attribute);

CREATE INDEX ix_product_attributes_name ON public.product_attributes (name);

CREATE INDEX ix_product_variants_display_order ON public.product_variants (display_order);

CREATE INDEX ix_product_variants_is_available ON public.product_variants (is_available);

CREATE INDEX ix_product_variants_is_default ON public.product_variants (is_default);

CREATE INDEX ix_product_variants_product_id ON public.product_variants (product_id);

CREATE UNIQUE INDEX ix_product_variants_sku ON public.product_variants (sku);

CREATE INDEX ix_promotions_active_dates ON public.promotions (start_date, end_date, is_active);

CREATE INDEX ix_promotions_code ON public.promotions (code);

CREATE INDEX ix_promotions_end_date ON public.promotions (end_date);

CREATE INDEX ix_promotions_is_active ON public.promotions (is_active);

CREATE INDEX ix_promotions_is_featured ON public.promotions (is_featured);

CREATE INDEX ix_promotions_start_date ON public.promotions (start_date);

CREATE INDEX ix_promotions_type ON public.promotions (type);

CREATE INDEX ix_refunds_created_at ON public.refunds (created_at);

CREATE INDEX ix_refunds_customer_id ON public.refunds (customer_id);

CREATE INDEX ix_refunds_order_id ON public.refunds (order_id);

CREATE UNIQUE INDEX ix_refunds_refund_number ON public.refunds (refund_number);

CREATE INDEX ix_refunds_status ON public.refunds (status);

CREATE INDEX ix_shipments_created_at ON public.shipments (created_at);

CREATE INDEX ix_shipments_delivered_at ON public.shipments (delivered_at);

CREATE INDEX ix_shipments_order_id ON public.shipments (order_id);

CREATE INDEX ix_shipments_status ON public.shipments (status);

CREATE INDEX ix_shipments_tracking_number ON public.shipments (tracking_number);

CREATE INDEX ix_shipping_zones_is_active ON public.shipping_zones (is_active);

CREATE INDEX ix_shipping_zones_name ON public.shipping_zones (name);

CREATE INDEX ix_shipping_zones_priority ON public.shipping_zones (priority);

CREATE INDEX ix_stores_city ON public.stores (city);

CREATE INDEX ix_stores_country ON public.stores (country);

CREATE INDEX ix_stores_display_order ON public.stores (display_order);

CREATE INDEX ix_stores_is_active ON public.stores (is_active);

CREATE INDEX ix_stores_is_default ON public.stores (is_default);

CREATE INDEX ix_stores_name ON public.stores (name);

CREATE UNIQUE INDEX ix_stores_store_code ON public.stores (store_code);

CREATE INDEX ix_suppliers_company_name ON public.suppliers (company_name);

CREATE INDEX ix_suppliers_email ON public.suppliers (email);

CREATE INDEX ix_suppliers_is_active ON public.suppliers (is_active);

CREATE INDEX ix_suppliers_is_preferred ON public.suppliers (is_preferred);

CREATE INDEX ix_suppliers_rating ON public.suppliers (rating);

CREATE UNIQUE INDEX ix_suppliers_supplier_code ON public.suppliers (supplier_code);

CREATE INDEX ix_vendors_created_at ON public.vendors (created_at);

CREATE INDEX ix_vendors_email ON public.vendors (email);

CREATE INDEX ix_vendors_is_featured ON public.vendors (is_featured);

CREATE INDEX ix_vendors_rating ON public.vendors (rating);

CREATE INDEX ix_vendors_status ON public.vendors (status);

CREATE INDEX ix_vendors_store_name ON public.vendors (store_name);

CREATE UNIQUE INDEX ix_vendors_user_id ON public.vendors (user_id);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251103015250_Migration_v0_1_14', '9.0.10');

COMMIT;

