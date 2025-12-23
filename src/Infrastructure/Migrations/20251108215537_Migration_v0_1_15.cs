using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration_v0_1_15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "financial_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    transaction_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    transaction_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "USD"),
                    transaction_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    inventory_transaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    journal_entry_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    counterparty = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    reference_number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    is_reconciled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    reconciled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    reconciled_by = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    payment_provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    notes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    tax_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    fee_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    net_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_financial_transactions_InventoryTransactions_inventory_tran~",
                        column: x => x.inventory_transaction_id,
                        principalTable: "InventoryTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_financial_transactions_JournalEntries_journal_entry_id",
                        column: x => x.journal_entry_id,
                        principalTable: "JournalEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_financial_transactions_Payments_payment_id",
                        column: x => x.payment_id,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_counterparty",
                table: "financial_transactions",
                column: "counterparty");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_date_type",
                table: "financial_transactions",
                columns: new[] { "transaction_date", "transaction_type" });

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_inventory_transaction_id",
                table: "financial_transactions",
                column: "inventory_transaction_id");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_is_reconciled",
                table: "financial_transactions",
                column: "is_reconciled");

            migrationBuilder.CreateIndex(
                name: "IX_financial_transactions_journal_entry_id",
                table: "financial_transactions",
                column: "journal_entry_id");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_order_id",
                table: "financial_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_payment_id",
                table: "financial_transactions",
                column: "payment_id");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_transaction_date",
                table: "financial_transactions",
                column: "transaction_date");

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_transaction_number",
                table: "financial_transactions",
                column: "transaction_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_financial_transactions_transaction_type",
                table: "financial_transactions",
                column: "transaction_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "financial_transactions");
        }
    }
}
