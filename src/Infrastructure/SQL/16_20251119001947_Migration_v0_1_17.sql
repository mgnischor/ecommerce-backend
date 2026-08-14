START TRANSACTION;
ALTER TABLE financial_transactions DROP CONSTRAINT "FK_financial_transactions_InventoryTransactions_inventory_tran~";

ALTER TABLE financial_transactions DROP CONSTRAINT "FK_financial_transactions_JournalEntries_journal_entry_id";

ALTER TABLE financial_transactions DROP CONSTRAINT "FK_financial_transactions_Payments_payment_id";

ALTER TABLE "InventoryTransactions" DROP CONSTRAINT "FK_InventoryTransactions_products_ProductId";

ALTER TABLE public.vendors DROP CONSTRAINT "PK_vendors";

ALTER TABLE public.users DROP CONSTRAINT "PK_users";

ALTER TABLE public.suppliers DROP CONSTRAINT "PK_suppliers";

ALTER TABLE public.stores DROP CONSTRAINT "PK_stores";

ALTER TABLE public.shipments DROP CONSTRAINT "PK_shipments";

ALTER TABLE public.refunds DROP CONSTRAINT "PK_refunds";

ALTER TABLE public.promotions DROP CONSTRAINT "PK_promotions";

ALTER TABLE public.products DROP CONSTRAINT "PK_products";

ALTER TABLE public.notifications DROP CONSTRAINT "PK_notifications";

ALTER TABLE public.shipping_zones DROP CONSTRAINT "PK_shipping_zones";

ALTER TABLE public.product_variants DROP CONSTRAINT "PK_product_variants";

ALTER TABLE public.product_attributes DROP CONSTRAINT "PK_product_attributes";

ALTER TABLE financial_transactions DROP CONSTRAINT "PK_financial_transactions";

ALTER TABLE public.vendors RENAME TO "Vendors";

ALTER TABLE public.users RENAME TO "Users";

ALTER TABLE public.suppliers RENAME TO "Suppliers";

ALTER TABLE public.stores RENAME TO "Stores";

ALTER TABLE public.shipments RENAME TO "Shipments";

ALTER TABLE public.refunds RENAME TO "Refunds";

ALTER TABLE public.promotions RENAME TO "Promotions";

ALTER TABLE public.products RENAME TO "Products";

ALTER TABLE public.notifications RENAME TO "Notifications";

ALTER TABLE public.shipping_zones RENAME TO "ShippingZones";

ALTER TABLE public.product_variants RENAME TO "ProductVariants";

ALTER TABLE public.product_attributes RENAME TO "ProductAttributes";

ALTER TABLE financial_transactions RENAME TO "FinancialTransactions";

ALTER INDEX "IX_financial_transactions_journal_entry_id" RENAME TO "IX_FinancialTransactions_journal_entry_id";

ALTER TABLE public."Vendors" ADD CONSTRAINT "PK_Vendors" PRIMARY KEY (id);

ALTER TABLE public."Users" ADD CONSTRAINT "PK_Users" PRIMARY KEY (id);

ALTER TABLE public."Suppliers" ADD CONSTRAINT "PK_Suppliers" PRIMARY KEY (id);

ALTER TABLE public."Stores" ADD CONSTRAINT "PK_Stores" PRIMARY KEY (id);

ALTER TABLE public."Shipments" ADD CONSTRAINT "PK_Shipments" PRIMARY KEY (id);

ALTER TABLE public."Refunds" ADD CONSTRAINT "PK_Refunds" PRIMARY KEY (id);

ALTER TABLE public."Promotions" ADD CONSTRAINT "PK_Promotions" PRIMARY KEY (id);

ALTER TABLE public."Products" ADD CONSTRAINT "PK_Products" PRIMARY KEY (id);

ALTER TABLE public."Notifications" ADD CONSTRAINT "PK_Notifications" PRIMARY KEY (id);

ALTER TABLE public."ShippingZones" ADD CONSTRAINT "PK_ShippingZones" PRIMARY KEY (id);

ALTER TABLE public."ProductVariants" ADD CONSTRAINT "PK_ProductVariants" PRIMARY KEY (id);

ALTER TABLE public."ProductAttributes" ADD CONSTRAINT "PK_ProductAttributes" PRIMARY KEY (id);

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "PK_FinancialTransactions" PRIMARY KEY (id);

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_InventoryTransactions_inventory_trans~" FOREIGN KEY (inventory_transaction_id) REFERENCES "InventoryTransactions" ("Id") ON DELETE SET NULL;

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_JournalEntries_journal_entry_id" FOREIGN KEY (journal_entry_id) REFERENCES "JournalEntries" ("Id") ON DELETE SET NULL;

ALTER TABLE "FinancialTransactions" ADD CONSTRAINT "FK_FinancialTransactions_Payments_payment_id" FOREIGN KEY (payment_id) REFERENCES "Payments" ("Id") ON DELETE SET NULL;

ALTER TABLE "InventoryTransactions" ADD CONSTRAINT "FK_InventoryTransactions_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."Products" (id) ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251119001947_Migration_v0_1_17', '10.0.10');

COMMIT;

