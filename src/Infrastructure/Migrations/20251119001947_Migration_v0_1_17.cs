using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration_v0_1_17 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_financial_transactions_InventoryTransactions_inventory_tran~",
                table: "financial_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_financial_transactions_JournalEntries_journal_entry_id",
                table: "financial_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_financial_transactions_Payments_payment_id",
                table: "financial_transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_products_ProductId",
                table: "InventoryTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_vendors",
                schema: "public",
                table: "vendors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_users",
                schema: "public",
                table: "users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_suppliers",
                schema: "public",
                table: "suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stores",
                schema: "public",
                table: "stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shipments",
                schema: "public",
                table: "shipments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_refunds",
                schema: "public",
                table: "refunds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_promotions",
                schema: "public",
                table: "promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_products",
                schema: "public",
                table: "products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_notifications",
                schema: "public",
                table: "notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_shipping_zones",
                schema: "public",
                table: "shipping_zones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_variants",
                schema: "public",
                table: "product_variants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_product_attributes",
                schema: "public",
                table: "product_attributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_financial_transactions",
                table: "financial_transactions");

            migrationBuilder.RenameTable(
                name: "vendors",
                schema: "public",
                newName: "Vendors",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "users",
                schema: "public",
                newName: "Users",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "suppliers",
                schema: "public",
                newName: "Suppliers",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "stores",
                schema: "public",
                newName: "Stores",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "shipments",
                schema: "public",
                newName: "Shipments",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "refunds",
                schema: "public",
                newName: "Refunds",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "promotions",
                schema: "public",
                newName: "Promotions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "products",
                schema: "public",
                newName: "Products",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "notifications",
                schema: "public",
                newName: "Notifications",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "shipping_zones",
                schema: "public",
                newName: "ShippingZones",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "product_variants",
                schema: "public",
                newName: "ProductVariants",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "product_attributes",
                schema: "public",
                newName: "ProductAttributes",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "financial_transactions",
                newName: "FinancialTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_financial_transactions_journal_entry_id",
                table: "FinancialTransactions",
                newName: "IX_FinancialTransactions_journal_entry_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Vendors",
                schema: "public",
                table: "Vendors",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                schema: "public",
                table: "Users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Suppliers",
                schema: "public",
                table: "Suppliers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Stores",
                schema: "public",
                table: "Stores",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Shipments",
                schema: "public",
                table: "Shipments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Refunds",
                schema: "public",
                table: "Refunds",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promotions",
                schema: "public",
                table: "Promotions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Products",
                schema: "public",
                table: "Products",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Notifications",
                schema: "public",
                table: "Notifications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ShippingZones",
                schema: "public",
                table: "ShippingZones",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductVariants",
                schema: "public",
                table: "ProductVariants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductAttributes",
                schema: "public",
                table: "ProductAttributes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FinancialTransactions",
                table: "FinancialTransactions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_InventoryTransactions_inventory_trans~",
                table: "FinancialTransactions",
                column: "inventory_transaction_id",
                principalTable: "InventoryTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_JournalEntries_journal_entry_id",
                table: "FinancialTransactions",
                column: "journal_entry_id",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FinancialTransactions_Payments_payment_id",
                table: "FinancialTransactions",
                column: "payment_id",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_Products_ProductId",
                table: "InventoryTransactions",
                column: "ProductId",
                principalSchema: "public",
                principalTable: "Products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_InventoryTransactions_inventory_trans~",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_JournalEntries_journal_entry_id",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_FinancialTransactions_Payments_payment_id",
                table: "FinancialTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_InventoryTransactions_Products_ProductId",
                table: "InventoryTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Vendors",
                schema: "public",
                table: "Vendors");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                schema: "public",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Suppliers",
                schema: "public",
                table: "Suppliers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Stores",
                schema: "public",
                table: "Stores");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Shipments",
                schema: "public",
                table: "Shipments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Refunds",
                schema: "public",
                table: "Refunds");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promotions",
                schema: "public",
                table: "Promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Products",
                schema: "public",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Notifications",
                schema: "public",
                table: "Notifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ShippingZones",
                schema: "public",
                table: "ShippingZones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductVariants",
                schema: "public",
                table: "ProductVariants");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductAttributes",
                schema: "public",
                table: "ProductAttributes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FinancialTransactions",
                table: "FinancialTransactions");

            migrationBuilder.RenameTable(
                name: "Vendors",
                schema: "public",
                newName: "vendors",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "public",
                newName: "users",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Suppliers",
                schema: "public",
                newName: "suppliers",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Stores",
                schema: "public",
                newName: "stores",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Shipments",
                schema: "public",
                newName: "shipments",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Refunds",
                schema: "public",
                newName: "refunds",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Promotions",
                schema: "public",
                newName: "promotions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Products",
                schema: "public",
                newName: "products",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Notifications",
                schema: "public",
                newName: "notifications",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ShippingZones",
                schema: "public",
                newName: "shipping_zones",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ProductVariants",
                schema: "public",
                newName: "product_variants",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ProductAttributes",
                schema: "public",
                newName: "product_attributes",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "FinancialTransactions",
                newName: "financial_transactions");

            migrationBuilder.RenameIndex(
                name: "IX_FinancialTransactions_journal_entry_id",
                table: "financial_transactions",
                newName: "IX_financial_transactions_journal_entry_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_vendors",
                schema: "public",
                table: "vendors",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_users",
                schema: "public",
                table: "users",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_suppliers",
                schema: "public",
                table: "suppliers",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stores",
                schema: "public",
                table: "stores",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shipments",
                schema: "public",
                table: "shipments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_refunds",
                schema: "public",
                table: "refunds",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_promotions",
                schema: "public",
                table: "promotions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_products",
                schema: "public",
                table: "products",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_notifications",
                schema: "public",
                table: "notifications",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_shipping_zones",
                schema: "public",
                table: "shipping_zones",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_variants",
                schema: "public",
                table: "product_variants",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_product_attributes",
                schema: "public",
                table: "product_attributes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_financial_transactions",
                table: "financial_transactions",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_financial_transactions_InventoryTransactions_inventory_tran~",
                table: "financial_transactions",
                column: "inventory_transaction_id",
                principalTable: "InventoryTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_financial_transactions_JournalEntries_journal_entry_id",
                table: "financial_transactions",
                column: "journal_entry_id",
                principalTable: "JournalEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_financial_transactions_Payments_payment_id",
                table: "financial_transactions",
                column: "payment_id",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InventoryTransactions_products_ProductId",
                table: "InventoryTransactions",
                column: "ProductId",
                principalSchema: "public",
                principalTable: "products",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
