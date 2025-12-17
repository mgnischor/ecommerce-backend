using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ECommerce.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration_v0_1_18 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                schema: "public",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastFailedLoginAt",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastLoginIpAddress",
                schema: "public",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSuccessfulLoginAt",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedUntil",
                schema: "public",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccountingRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<int>(type: "integer", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DebitAccountCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreditAccountCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Condition = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountingRules", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "AccountingRules",
                columns: new[] { "Id", "Condition", "CreatedAt", "CreditAccountCode", "DebitAccountCode", "Description", "IsActive", "RuleCode", "TransactionType", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("a0000000-0000-0000-0000-000000000001"), null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2.1.01.001", "1.1.03.001", "Purchase of inventory from suppliers", true, "PURCHASE", 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0000-000000000002"), null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1.1.03.001", "3.1.01.001", "Sale of inventory to customers (COGS recognition)", true, "SALE", 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0000-000000000003"), null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "3.1.01.001", "1.1.03.001", "Customer returns inventory (COGS reversal)", true, "SALE_RETURN", 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0000-000000000004"), null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1.1.03.001", "2.1.01.001", "Return of inventory to suppliers", true, "PURCHASE_RETURN", 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0000-000000000005"), "Quantity > 0", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "4.2.01.001", "1.1.03.001", "Positive inventory adjustment (overage)", true, "ADJUSTMENT_POSITIVE", 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0000-000000000006"), "Quantity < 0", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1.1.03.001", "3.2.01.002", "Negative inventory adjustment (shortage)", true, "ADJUSTMENT_NEGATIVE", 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("a0000000-0000-0000-0000-000000000007"), null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1.1.03.001", "3.2.01.001", "Inventory loss, shrinkage, or write-off", true, "LOSS", 7, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            // Insert ChartOfAccounts with conflict handling
            migrationBuilder.Sql(@"
                INSERT INTO ""ChartOfAccounts"" (""Id"", ""AccountCode"", ""AccountName"", ""AccountType"", ""Balance"", ""CreatedAt"", ""Description"", ""IsActive"", ""IsAnalytic"", ""ParentAccountId"", ""UpdatedAt"")
                VALUES
                    ('10000000-0000-0000-0000-000000000001', '1.1.01.001', 'Cash and Cash Equivalents', 1, 0, '2025-01-01 00:00:00+00', 'Bank accounts and petty cash', true, true, NULL, '2025-01-01 00:00:00+00'),
                    ('10000000-0000-0000-0000-000000000002', '1.1.03.001', 'Inventory', 1, 0, '2025-01-01 00:00:00+00', 'Merchandise inventory for resale', true, true, NULL, '2025-01-01 00:00:00+00'),
                    ('20000000-0000-0000-0000-000000000001', '2.1.01.001', 'Accounts Payable - Suppliers', 2, 0, '2025-01-01 00:00:00+00', 'Amounts owed to suppliers for inventory purchases', true, true, NULL, '2025-01-01 00:00:00+00'),
                    ('30000000-0000-0000-0000-000000000001', '3.1.01.001', 'Cost of Goods Sold', 5, 0, '2025-01-01 00:00:00+00', 'Direct costs of goods sold to customers', true, true, NULL, '2025-01-01 00:00:00+00'),
                    ('30000000-0000-0000-0000-000000000002', '3.2.01.001', 'Inventory Loss', 5, 0, '2025-01-01 00:00:00+00', 'Inventory shrinkage, loss, and write-offs', true, true, NULL, '2025-01-01 00:00:00+00'),
                    ('30000000-0000-0000-0000-000000000003', '3.2.01.002', 'Other Operating Expenses', 5, 0, '2025-01-01 00:00:00+00', 'Miscellaneous operating expenses including negative inventory adjustments', true, true, NULL, '2025-01-01 00:00:00+00'),
                    ('40000000-0000-0000-0000-000000000001', '4.2.01.001', 'Other Operating Income', 4, 0, '2025-01-01 00:00:00+00', 'Miscellaneous income including positive inventory adjustments', true, true, NULL, '2025-01-01 00:00:00+00')
                ON CONFLICT (""AccountCode"") DO NOTHING;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRules_IsActive",
                table: "AccountingRules",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRules_RuleCode",
                table: "AccountingRules",
                column: "RuleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRules_TransactionType",
                table: "AccountingRules",
                column: "TransactionType");

            migrationBuilder.CreateIndex(
                name: "IX_AccountingRules_TransactionType_IsActive",
                table: "AccountingRules",
                columns: new[] { "TransactionType", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountingRules");

            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "ChartOfAccounts",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"));

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastFailedLoginAt",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastLoginIpAddress",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastSuccessfulLoginAt",
                schema: "public",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockedUntil",
                schema: "public",
                table: "Users");
        }
    }
}
