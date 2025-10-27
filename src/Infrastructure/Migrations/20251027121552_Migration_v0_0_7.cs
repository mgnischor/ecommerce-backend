using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.src.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Migration_v0_0_7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "products",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    updated_by = table.Column<Guid>(type: "uuid", nullable: false, defaultValue: new Guid("ce06e1a8-f688-44b6-b616-4badf09d9153")),
                    category = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "0"),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValueSql: "1"),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    sku = table.Column<string>(type: "character varying(50)", unicode: false, maxLength: 50, nullable: false),
                    brand = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    image_url = table.Column<string>(type: "character varying(500)", unicode: false, maxLength: 500, nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    discount_price = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    weight = table.Column<decimal>(type: "numeric(10,2)", nullable: false, defaultValue: 0m),
                    stock_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    min_stock_level = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    max_order_quantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 100),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_featured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    is_on_sale = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    tags = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    images = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'"),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_products_category",
                schema: "public",
                table: "products",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_products_created_at",
                schema: "public",
                table: "products",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_products_is_featured",
                schema: "public",
                table: "products",
                column: "is_featured");

            migrationBuilder.CreateIndex(
                name: "ix_products_is_on_sale",
                schema: "public",
                table: "products",
                column: "is_on_sale");

            migrationBuilder.CreateIndex(
                name: "ix_products_name",
                schema: "public",
                table: "products",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "ix_products_sku",
                schema: "public",
                table: "products",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_status",
                schema: "public",
                table: "products",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "products",
                schema: "public");
        }
    }
}
