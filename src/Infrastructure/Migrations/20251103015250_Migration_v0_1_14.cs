using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration_v0_1_14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notifications",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    message = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    action_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    icon = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    related_entity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    related_entity_type = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    is_read = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    send_email = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    email_sent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    email_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    send_push = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    push_sent = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    push_sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metadata = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notifications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_attributes",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    code = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    data_type = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "Text"),
                    possible_values = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    default_value = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    is_required = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_variant_attribute = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_filterable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_searchable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_visible_on_product_page = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    unit = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: true),
                    validation_pattern = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    applicable_category_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'"),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_attributes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "product_variants",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    barcode = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    weight = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    images = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    attributes = table.Column<Dictionary<string, string>>(type: "jsonb", nullable: false, defaultValueSql: "'{}'"),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_available = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_variants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "promotions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    code = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    minimum_order_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    maximum_discount_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    max_usage_count = table.Column<int>(type: "integer", nullable: true),
                    usage_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_usage_per_user = table.Column<int>(type: "integer", nullable: true),
                    eligible_product_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'"),
                    eligible_category_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'"),
                    eligible_user_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'"),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_combinable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    banner_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    terms_and_conditions = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    refund_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    refund_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    customer_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    admin_notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    order_item_ids = table.Column<List<Guid>>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'"),
                    requires_return = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    return_tracking_number = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    approved_by = table.Column<Guid>(type: "uuid", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    rejection_reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    transaction_id = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: true),
                    restocking_fee = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refunds", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shipments",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    shipping_address_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tracking_number = table.Column<string>(type: "character varying(100)", unicode: false, maxLength: 100, nullable: false),
                    carrier = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    service_type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    shipping_cost = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    weight = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    length = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    width = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    height = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    expected_delivery_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    delivered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    shipped_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    tracking_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    is_insured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    insurance_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    requires_signature = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    received_by = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shipping_zones",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    countries = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    states = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    postal_codes = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    base_rate = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    rate_per_kg = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    rate_per_item = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    free_shipping_threshold = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    minimum_order_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    maximum_order_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    estimated_delivery_days_min = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    estimated_delivery_days_max = table.Column<int>(type: "integer", nullable: false, defaultValue: 7),
                    available_shipping_methods = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    tax_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipping_zones", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "stores",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    store_code = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    latitude = table.Column<decimal>(type: "numeric(10,7)", nullable: true),
                    longitude = table.Column<decimal>(type: "numeric(10,7)", nullable: true),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    opening_hours = table.Column<string>(type: "jsonb", nullable: true),
                    timezone = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false, defaultValue: "UTC"),
                    currency = table.Column<string>(type: "character varying(3)", unicode: false, maxLength: 3, nullable: false, defaultValue: "USD"),
                    logo_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    image_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    is_default = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    supports_pickup = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    supports_delivery = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    display_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stores", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "suppliers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    company_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    supplier_code = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    contact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    alternate_phone = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    fax_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    website = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    registration_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    bank_account_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bank_routing_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    payment_terms = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    credit_limit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    current_balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    rating = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0m),
                    lead_time_days = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    minimum_order_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_preferred = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "vendors",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    business_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    store_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    logo_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    banner_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: true),
                    address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    state = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    postal_code = table.Column<string>(type: "character varying(20)", unicode: false, maxLength: 20, nullable: false),
                    country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    tax_id = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    registration_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    commission_rate = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 10.0m),
                    rating = table.Column<decimal>(type: "numeric(3,2)", nullable: false, defaultValue: 0m),
                    total_ratings = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    total_sales = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    total_orders = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    bank_account_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    bank_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    bank_routing_number = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: true),
                    paypal_email = table.Column<string>(type: "character varying(256)", unicode: false, maxLength: 256, nullable: true),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    verified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vendors", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_created_at",
                schema: "public",
                table: "notifications",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_is_read",
                schema: "public",
                table: "notifications",
                column: "is_read");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_priority",
                schema: "public",
                table: "notifications",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_type",
                schema: "public",
                table: "notifications",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id",
                schema: "public",
                table: "notifications",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_notifications_user_id_is_read",
                schema: "public",
                table: "notifications",
                columns: new[] { "user_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "ix_product_attributes_code",
                schema: "public",
                table: "product_attributes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_attributes_display_order",
                schema: "public",
                table: "product_attributes",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_product_attributes_is_filterable",
                schema: "public",
                table: "product_attributes",
                column: "is_filterable");

            migrationBuilder.CreateIndex(
                name: "ix_product_attributes_is_variant_attribute",
                schema: "public",
                table: "product_attributes",
                column: "is_variant_attribute");

            migrationBuilder.CreateIndex(
                name: "ix_product_attributes_name",
                schema: "public",
                table: "product_attributes",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_product_variants_display_order",
                schema: "public",
                table: "product_variants",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_product_variants_is_available",
                schema: "public",
                table: "product_variants",
                column: "is_available");

            migrationBuilder.CreateIndex(
                name: "ix_product_variants_is_default",
                schema: "public",
                table: "product_variants",
                column: "is_default");

            migrationBuilder.CreateIndex(
                name: "ix_product_variants_product_id",
                schema: "public",
                table: "product_variants",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_variants_sku",
                schema: "public",
                table: "product_variants",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_promotions_active_dates",
                schema: "public",
                table: "promotions",
                columns: new[] { "start_date", "end_date", "is_active" });

            migrationBuilder.CreateIndex(
                name: "ix_promotions_code",
                schema: "public",
                table: "promotions",
                column: "code");

            migrationBuilder.CreateIndex(
                name: "ix_promotions_end_date",
                schema: "public",
                table: "promotions",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "ix_promotions_is_active",
                schema: "public",
                table: "promotions",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_promotions_is_featured",
                schema: "public",
                table: "promotions",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "ix_promotions_start_date",
                schema: "public",
                table: "promotions",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "ix_promotions_type",
                schema: "public",
                table: "promotions",
                column: "type");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_created_at",
                schema: "public",
                table: "refunds",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_customer_id",
                schema: "public",
                table: "refunds",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_order_id",
                schema: "public",
                table: "refunds",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_refund_number",
                schema: "public",
                table: "refunds",
                column: "refund_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refunds_status",
                schema: "public",
                table: "refunds",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_created_at",
                schema: "public",
                table: "shipments",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_delivered_at",
                schema: "public",
                table: "shipments",
                column: "delivered_at");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_order_id",
                schema: "public",
                table: "shipments",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_status",
                schema: "public",
                table: "shipments",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_shipments_tracking_number",
                schema: "public",
                table: "shipments",
                column: "tracking_number");

            migrationBuilder.CreateIndex(
                name: "ix_shipping_zones_is_active",
                schema: "public",
                table: "shipping_zones",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_shipping_zones_name",
                schema: "public",
                table: "shipping_zones",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_shipping_zones_priority",
                schema: "public",
                table: "shipping_zones",
                column: "priority");

            migrationBuilder.CreateIndex(
                name: "ix_stores_city",
                schema: "public",
                table: "stores",
                column: "city");

            migrationBuilder.CreateIndex(
                name: "ix_stores_country",
                schema: "public",
                table: "stores",
                column: "country");

            migrationBuilder.CreateIndex(
                name: "ix_stores_display_order",
                schema: "public",
                table: "stores",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "ix_stores_is_active",
                schema: "public",
                table: "stores",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_stores_is_default",
                schema: "public",
                table: "stores",
                column: "is_default");

            migrationBuilder.CreateIndex(
                name: "ix_stores_name",
                schema: "public",
                table: "stores",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_stores_store_code",
                schema: "public",
                table: "stores",
                column: "store_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_company_name",
                schema: "public",
                table: "suppliers",
                column: "company_name");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_email",
                schema: "public",
                table: "suppliers",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_is_active",
                schema: "public",
                table: "suppliers",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_is_preferred",
                schema: "public",
                table: "suppliers",
                column: "is_preferred");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_rating",
                schema: "public",
                table: "suppliers",
                column: "rating");

            migrationBuilder.CreateIndex(
                name: "ix_suppliers_supplier_code",
                schema: "public",
                table: "suppliers",
                column: "supplier_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_vendors_created_at",
                schema: "public",
                table: "vendors",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_vendors_email",
                schema: "public",
                table: "vendors",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "ix_vendors_is_featured",
                schema: "public",
                table: "vendors",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "ix_vendors_rating",
                schema: "public",
                table: "vendors",
                column: "rating");

            migrationBuilder.CreateIndex(
                name: "ix_vendors_status",
                schema: "public",
                table: "vendors",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_vendors_store_name",
                schema: "public",
                table: "vendors",
                column: "store_name");

            migrationBuilder.CreateIndex(
                name: "ix_vendors_user_id",
                schema: "public",
                table: "vendors",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notifications",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_attributes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "product_variants",
                schema: "public");

            migrationBuilder.DropTable(
                name: "promotions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "refunds",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shipments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "shipping_zones",
                schema: "public");

            migrationBuilder.DropTable(
                name: "stores",
                schema: "public");

            migrationBuilder.DropTable(
                name: "suppliers",
                schema: "public");

            migrationBuilder.DropTable(
                name: "vendors",
                schema: "public");
        }
    }
}
