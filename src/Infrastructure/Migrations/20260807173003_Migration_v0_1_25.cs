using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Comex.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration_v0_1_25 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_spending = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    total_orders = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    last_order_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    last_login_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    historical_average_order_days = table.Column<int>(
                        type: "integer",
                        nullable: true
                    ),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    is_deleted = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "GiftCards",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")
                    ),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    card_number = table.Column<string>(
                        type: "character varying(19)",
                        unicode: false,
                        maxLength: 19,
                        nullable: false
                    ),
                    balance = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    is_active = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    is_revoked = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    allows_reload = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: true
                    ),
                    issued_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    expires_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    is_deleted = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GiftCards", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "InventoryPlans",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_stock = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    reorder_point = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    average_daily_sales = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    lead_time_days = table.Column<int>(type: "integer", nullable: true),
                    service_level_percent = table.Column<int>(type: "integer", nullable: true),
                    received_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    cost_of_goods_sold = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    average_inventory_value = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    sales_in_last_90_days = table.Column<int>(
                        type: "integer",
                        nullable: false,
                        defaultValue: 0
                    ),
                    is_deleted = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryPlans", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "Invoices",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(
                        type: "uuid",
                        nullable: false,
                        defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")
                    ),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: true),
                    invoice_number = table.Column<string>(
                        type: "character varying(50)",
                        unicode: false,
                        maxLength: 50,
                        nullable: false
                    ),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subtotal = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    tax_amount = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    shipping_cost = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    total = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    paid_amount = table.Column<decimal>(
                        type: "numeric(18,2)",
                        nullable: false,
                        defaultValue: 0m
                    ),
                    due_date = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    issued_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false
                    ),
                    is_paid = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    is_deleted = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.id);
                }
            );

            migrationBuilder.CreateTable(
                name: "LoyaltyRewards",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: false),
                    point_balance = table.Column<int>(type: "integer", nullable: false),
                    total_points_earned = table.Column<int>(type: "integer", nullable: false),
                    total_points_redeemed = table.Column<int>(type: "integer", nullable: false),
                    last_earned_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: true
                    ),
                    tier = table.Column<string>(
                        type: "character varying(20)",
                        maxLength: 20,
                        nullable: false,
                        defaultValue: "Bronze"
                    ),
                    is_deleted = table.Column<bool>(
                        type: "boolean",
                        nullable: false,
                        defaultValue: false
                    ),
                    created_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                    updated_at = table.Column<DateTime>(
                        type: "timestamp with time zone",
                        nullable: false,
                        defaultValueSql: "CURRENT_TIMESTAMP"
                    ),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyRewards", x => x.id);
                }
            );

            migrationBuilder.CreateIndex(
                name: "ix_customers_is_active",
                schema: "public",
                table: "Customers",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(
                name: "ix_customers_user_id",
                schema: "public",
                table: "Customers",
                column: "user_id",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_gift_cards_card_number",
                schema: "public",
                table: "GiftCards",
                column: "card_number",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_gift_cards_is_active",
                schema: "public",
                table: "GiftCards",
                column: "is_active"
            );

            migrationBuilder.CreateIndex(
                name: "ix_inventory_plans_is_deleted",
                schema: "public",
                table: "InventoryPlans",
                column: "is_deleted"
            );

            migrationBuilder.CreateIndex(
                name: "ix_inventory_plans_product_id",
                schema: "public",
                table: "InventoryPlans",
                column: "product_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_invoices_customer_id",
                schema: "public",
                table: "Invoices",
                column: "customer_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_invoices_invoice_number",
                schema: "public",
                table: "Invoices",
                column: "invoice_number",
                unique: true
            );

            migrationBuilder.CreateIndex(
                name: "ix_invoices_is_paid",
                schema: "public",
                table: "Invoices",
                column: "is_paid"
            );

            migrationBuilder.CreateIndex(
                name: "ix_invoices_order_id",
                schema: "public",
                table: "Invoices",
                column: "order_id"
            );

            migrationBuilder.CreateIndex(
                name: "ix_loyalty_rewards_customer_id",
                schema: "public",
                table: "LoyaltyRewards",
                column: "customer_id",
                unique: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Customers", schema: "public");

            migrationBuilder.DropTable(name: "GiftCards", schema: "public");

            migrationBuilder.DropTable(name: "InventoryPlans", schema: "public");

            migrationBuilder.DropTable(name: "Invoices", schema: "public");

            migrationBuilder.DropTable(name: "LoyaltyRewards", schema: "public");
        }
    }
}
