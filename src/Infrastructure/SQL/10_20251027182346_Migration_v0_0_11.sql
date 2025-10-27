START TRANSACTION;
CREATE TABLE "Addresses" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "AddressType" text NOT NULL,
    "FullName" text NOT NULL,
    "PhoneNumber" text NOT NULL,
    "AddressLine1" text NOT NULL,
    "AddressLine2" text,
    "City" text NOT NULL,
    "State" text NOT NULL,
    "PostalCode" text NOT NULL,
    "Country" text NOT NULL,
    "IsDefault" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Addresses" PRIMARY KEY ("Id")
);

CREATE TABLE "CartItems" (
    "Id" uuid NOT NULL,
    "CartId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_CartItems" PRIMARY KEY ("Id")
);

CREATE TABLE "Carts" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "SessionId" text,
    "CouponCode" text,
    "DiscountAmount" numeric NOT NULL,
    "IsActive" boolean NOT NULL,
    "ExpiresAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Carts" PRIMARY KEY ("Id")
);

CREATE TABLE "Categories" (
    "Id" uuid NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "UpdatedBy" uuid,
    "ParentCategoryId" uuid,
    "Name" text NOT NULL,
    "Slug" text NOT NULL,
    "Description" text,
    "ImageUrl" text,
    "DisplayOrder" integer NOT NULL,
    "IsActive" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "MetaTitle" text,
    "MetaDescription" text,
    "MetaKeywords" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Categories" PRIMARY KEY ("Id")
);

CREATE TABLE "Coupons" (
    "Id" uuid NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "UpdatedBy" uuid,
    "Code" text NOT NULL,
    "Description" text NOT NULL,
    "DiscountType" text NOT NULL,
    "DiscountValue" numeric NOT NULL,
    "MinimumOrderAmount" numeric,
    "MaximumDiscountAmount" numeric,
    "MaxUsageCount" integer,
    "UsageCount" integer NOT NULL,
    "MaxUsagePerCustomer" integer,
    "ApplicableProductIds" uuid[],
    "ApplicableCategoryIds" integer[],
    "ValidFrom" timestamp with time zone,
    "ValidUntil" timestamp with time zone,
    "IsActive" boolean NOT NULL,
    "IsDeleted" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Coupons" PRIMARY KEY ("Id")
);

CREATE TABLE "Inventories" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Location" text NOT NULL,
    "QuantityInStock" integer NOT NULL,
    "QuantityReserved" integer NOT NULL,
    "QuantityAvailable" integer NOT NULL,
    "ReorderLevel" integer NOT NULL,
    "ReorderQuantity" integer NOT NULL,
    "LastStockReceived" timestamp with time zone,
    "LastInventoryCount" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Inventories" PRIMARY KEY ("Id")
);

CREATE TABLE "OrderItems" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "ProductName" text NOT NULL,
    "ProductSku" text NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric NOT NULL,
    "DiscountAmount" numeric NOT NULL,
    "TaxAmount" numeric NOT NULL,
    "TotalPrice" numeric NOT NULL,
    "ProductImageUrl" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id")
);

CREATE TABLE "Orders" (
    "Id" uuid NOT NULL,
    "CreatedBy" uuid NOT NULL,
    "UpdatedBy" uuid,
    "CustomerId" uuid NOT NULL,
    "OrderNumber" text NOT NULL,
    "Status" integer NOT NULL,
    "SubTotal" numeric NOT NULL,
    "TaxAmount" numeric NOT NULL,
    "ShippingCost" numeric NOT NULL,
    "DiscountAmount" numeric NOT NULL,
    "TotalAmount" numeric NOT NULL,
    "PaymentMethod" integer NOT NULL,
    "ShippingMethod" integer NOT NULL,
    "ShippingAddressId" uuid,
    "BillingAddressId" uuid,
    "CouponCode" text,
    "CustomerNotes" text,
    "AdminNotes" text,
    "TrackingNumber" text,
    "ExpectedDeliveryDate" timestamp with time zone,
    "DeliveredAt" timestamp with time zone,
    "CancelledAt" timestamp with time zone,
    "CancellationReason" text,
    "IsDeleted" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Orders" PRIMARY KEY ("Id")
);

CREATE TABLE "Payments" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "TransactionId" text,
    "PaymentMethod" integer NOT NULL,
    "Status" integer NOT NULL,
    "Amount" numeric NOT NULL,
    "Currency" text NOT NULL,
    "PaymentProvider" text,
    "ProviderResponse" text,
    "ErrorMessage" text,
    "AuthorizedAt" timestamp with time zone,
    "CapturedAt" timestamp with time zone,
    "RefundedAt" timestamp with time zone,
    "RefundAmount" numeric,
    "RefundReason" text,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Payments" PRIMARY KEY ("Id")
);

CREATE TABLE "Reviews" (
    "Id" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "OrderId" uuid,
    "Rating" integer NOT NULL,
    "Title" text NOT NULL,
    "Comment" text NOT NULL,
    "IsVerifiedPurchase" boolean NOT NULL,
    "IsApproved" boolean NOT NULL,
    "IsFlagged" boolean NOT NULL,
    "HelpfulCount" integer NOT NULL,
    "NotHelpfulCount" integer NOT NULL,
    "AdminResponse" text,
    "AdminRespondedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Reviews" PRIMARY KEY ("Id")
);

CREATE TABLE "WishlistItems" (
    "Id" uuid NOT NULL,
    "WishlistId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "Notes" text,
    "Priority" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_WishlistItems" PRIMARY KEY ("Id")
);

CREATE TABLE "Wishlists" (
    "Id" uuid NOT NULL,
    "CustomerId" uuid NOT NULL,
    "Name" text NOT NULL,
    "IsPublic" boolean NOT NULL,
    "IsDefault" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Wishlists" PRIMARY KEY ("Id")
);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251027174620_Migration_v0_0_10', '9.0.10');

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20251027182346_Migration_v0_0_11', '9.0.10');

COMMIT;

