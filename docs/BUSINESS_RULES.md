# Business Rules Implementation

This document describes all the domain business rules implemented in the e-commerce backend system.

## Table of Contents

1. [Policies](#policies)
2. [Specifications](#specifications)
3. [Value Objects](#value-objects)
4. [Domain Services](#domain-services)
5. [Domain Events](#domain-events)
6. [Domain Exceptions](#domain-exceptions)

---

## Policies

Business policies define the core business rules and constraints.

### PricingPolicy

**Location:** `src/Domain/Policies/PricingPolicy.cs`

**Purpose:** Defines business rules for product pricing

**Key Rules:**

-   Minimum price: $0.01
-   Maximum price: $999,999.99
-   Maximum discount: 99%
-   Validates discount prices relative to original prices
-   Calculates discount percentages and applies discounts
-   Determines effective prices considering discounts
-   Validates bulk pricing calculations

### StockManagementPolicy

**Location:** `src/Domain/Policies/StockManagementPolicy.cs`

**Purpose:** Defines business rules for inventory and stock management

**Key Rules:**

-   Checks out-of-stock status
-   Identifies low stock situations (default threshold: 10 units)
-   Validates stock reservation availability
-   Enforces order quantity limits
-   Calculates recommended reorder quantities (30-day supply)
-   Determines when automatic reorder should trigger

### OrderValidationPolicy

**Location:** `src/Domain/Policies/OrderValidationPolicy.cs`

**Purpose:** Defines business rules for order validation and processing

**Key Rules:**

-   Minimum order amount: $0.01
-   Maximum order amount: $999,999.99
-   Maximum items per order: 100
-   Order cancellation window: 24 hours
-   Refund eligibility: Within 30 days of delivery
-   Valid status transitions (e.g., Pending → Processing → Confirmed → Shipped → Delivered)
-   Validates order totals calculation
-   Determines shipping address requirements

### CouponValidationPolicy

**Location:** `src/Domain/Policies/CouponValidationPolicy.cs`

**Purpose:** Defines business rules for coupon validation and application

**Key Rules:**

-   Coupon code length: 3-50 characters (alphanumeric and hyphens only)
-   Maximum discount percentage: 100%
-   Maximum discount amount: $999,999.99
-   Validates coupon active status
-   Checks validity period (from/until dates)
-   Enforces usage limits (total and per customer)
-   Validates minimum order amount requirements
-   Calculates discount amounts (percentage or fixed)
-   Checks product applicability

### ReviewValidationPolicy

**Location:** `src/Domain/Policies/ReviewValidationPolicy.cs`

**Purpose:** Defines business rules for review moderation and validation

**Key Rules:**

-   Rating range: 1-5 stars
-   Title length: 3-200 characters
-   Comment length: 10-5,000 characters
-   Review edit window: 24 hours
-   Customers must have purchased to review
-   One review per customer per product
-   Moderation triggers for flagged words and extreme ratings
-   Helpful threshold: 60% with minimum 5 votes

### CartPolicy

**Location:** `src/Domain/Policies/CartPolicy.cs`

**Purpose:** Defines business rules for shopping cart management

**Key Rules:**

-   Maximum items in cart: 50
-   Maximum quantity per item: 99
-   Cart expiration: 30 days (7 days for guest carts)
-   Maximum cart value: $999,999.99
-   Validates item additions and quantity updates
-   Handles cart merging for duplicate items
-   Enforces stock availability checks

### SecurityPolicy

**Location:** `src/Domain/Policies/SecurityPolicy.cs`

**Purpose:** Defines security rules for authentication and authorization

**Key Rules:**

-   Session timeout: 30 minutes of inactivity
-   Access token validity: 15 minutes
-   Refresh token validity: 7 days
-   Maximum concurrent sessions: 5
-   Password reset token validity: 24 hours
-   API rate limit: 100 requests per minute
-   Permission validation based on user access levels
-   Admin feature access control

### ShipmentPolicy

**Location:** `src/Domain/Policies/ShipmentPolicy.cs`

**Purpose:** Defines business rules for shipment validation and tracking

**Key Rules:**

-   Tracking number length: 5-100 characters
-   Maximum shipping cost: $999,999.99
-   Maximum package weight: 500 kg
-   Standard delivery: 7 days
-   Express delivery: 3 days
-   Valid status transitions enforcement
-   Volumetric weight calculation
-   Package dimension validation

### GiftCardPolicy

**Location:** `src/Domain/Policies/GiftCardPolicy.cs`

**Purpose:** Defines business rules for gift card validation and redemption

**Key Rules:**

-   Card number format: 13-19 digits (alphanumeric with optional dashes)
-   Balance limits: $0.01 to $5,000
-   Cards are inactive when revoked
-   Default validity period: 24 months from issuance
-   Redemptions cannot exceed the available balance
-   Reloads are only allowed for reloadable cards below the maximum balance

### RewardsProgramPolicy

**Location:** `src/Domain/Policies/RewardsProgramPolicy.cs`

**Purpose:** Defines business rules for the customer rewards and loyalty program

**Key Rules:**

-   Earn 1 point per dollar spent
-   Earn bonus points for product reviews (capped at 100 points)
-   Rewards require a positive point cost
-   Points value: 100 points = $1.00
-   Points expire after 365 days of inactivity
-   Loyalty tiers: Bronze, Silver, Gold, Platinum with earning multipliers

### TaxPolicy

**Location:** `src/Domain/Policies/TaxPolicy.cs`

**Purpose:** Defines business rules for tax calculation and tax compliance

**Key Rules:**

-   Tax rates must be between 0% and 100%
-   Tax-exempt categories: Books, Groceries, Medical, Prescription, Education
-   Digital products are always taxable
-   Physical products are exempt in designated tax-free zones
-   Supports gross-to-base price extraction for tax-inclusive pricing

### CustomerSegmentationPolicy

**Location:** `src/Domain/Policies/CustomerSegmentationPolicy.cs`

**Purpose:** Defines business rules for customer segmentation and lifecycle analysis

**Key Rules:**

-   Segments: VIP, Regular, New, Occasional
-   High-value threshold: $5,000 in total spending
-   Churn threshold: 90 days without an order
-   At-risk detection: inactivity beyond 2x the customer's normal order cadence
-   Dormant threshold: 365 days without login
-   Churn risk levels: Critical, High, Medium, Low

### OrderFulfillmentPolicy

**Location:** `src/Domain/Policies/OrderFulfillmentPolicy.cs`

**Purpose:** Defines business rules for order fulfillment and shipment planning

**Key Rules:**

-   Fulfillment only starts for Confirmed or Processing orders
-   Shipments split when exceeding 20 items per package
-   Ready to ship requires payment and available inventory
-   Palletization required above 500 kg
-   Default fulfillment window: 24 hours
-   Shipments only combine for the same customer and address

### ReturnsPolicy

**Location:** `src/Domain/Policies/ReturnsPolicy.cs`

**Purpose:** Defines business rules for product returns

**Key Rules:**

-   Default return window: 30 days
-   Only delivered or completed physical orders are returnable
-   Digital products are non-returnable once downloaded or opened
-   Returns over $500 always require inspection
-   Refund percentage based on condition score (3-10 scale)
-   Auto-approval threshold: $50 with proof photos required

### SearchPolicy

**Location:** `src/Domain/Policies/SearchPolicy.cs`

**Purpose:** Defines business rules for product search and catalog queries

**Key Rules:**

-   Search term length: 2-100 characters (empty returns all)
-   Page numbers start at 1
-   Page size limits: 1-100 (default 20)
-   Falls back to relevance sorting for unknown sort options
-   Price range validation (min cannot exceed max)

### EmailPolicy

**Location:** `src/Domain/Policies/EmailPolicy.cs`

**Purpose:** Defines business rules for email and communication delivery

**Key Rules:**

-   Subject length: 1-150 characters
-   Body length: maximum 100,000 characters
-   Transactional types: Order, Payment, Shipment, Account
-   Transactional emails never require separate marketing consent
-   Marketing emails only sent between 8:00 and 21:00

### InventoryPlanningPolicy

**Location:** `src/Domain/Policies/InventoryPlanningPolicy.cs`

**Purpose:** Defines business rules for inventory planning and demand forecasting

**Key Rules:**

-   Simple moving average sales forecasting
-   Safety stock calculation with service level factors (90/95/99%)
-   Days of supply calculation from demand
-   Stock age threshold: 180 days
-   Slow-moving liquidation when turnover is below 10%

### AddressPolicy

**Location:** `src/Domain/Policies/AddressPolicy.cs`

**Purpose:** Defines business rules for customer address validation

**Key Rules:**

-   Street length: 3-200 characters
-   City length: 2-100 characters
-   Country codes: 3 characters (ISO)
-   Postal codes: 4-12 characters with letters, digits, spaces, and dashes
-   Addresses with a company name are treated as business addresses

### ProductCatalogPolicy

**Location:** `src/Domain/Policies/ProductCatalogPolicy.cs`

**Purpose:** Defines business rules for product catalog management

**Key Rules:**

-   Product name length: 3-200 characters
-   Description length: maximum 5,000 characters
-   Category changes blocked for products with active orders
-   Maximum product weight: 500 kg
-   Maximum of 10 tags per product (each up to 50 characters)

### SupplierPolicy

**Location:** `src/Domain/Policies/SupplierPolicy.cs`

**Purpose:** Defines business rules for supplier management

**Key Rules:**

-   Suppliers require a valid company name and contact email
-   Purchase orders only for verified, active vendors
-   Auto-replenishment requires 90%+ on-time delivery over 5+ orders
-   Payment terms follow the NET n format (1-365 days)
-   Suspension when complaint rate exceeds 20%

### StorePolicy

**Location:** `src/Domain/Policies/StorePolicy.cs`

**Purpose:** Defines business rules for store configuration and operation

**Key Rules:**

-   Store name length: 2-100 characters
-   Stores are inactive during suspension periods
-   Orders accepted only when active and within business hours
-   Business hours support stores that close after midnight

### ShippingRatePolicy

**Location:** `src/Domain/Policies/ShippingRatePolicy.cs`

**Purpose:** Defines business rules for shipping rate calculation and carrier selection

**Key Rules:**

-   Weight-based rates combine base rate and per-kg rate
-   Free shipping by country or order amount threshold
-   Per-item rate calculation
-   Surcharge application as a percentage
-   Carrier codes: alphanumeric with dashes, up to 10 characters
-   Handling fees capped at $100

### InvoicePolicy

**Location:** `src/Domain/Policies/InvoicePolicy.cs`

**Purpose:** Defines business rules for invoice generation and payment terms

**Key Rules:**

-   Invoice number format: 5-50 characters
-   Credit notes only for paid invoices within the valid period
-   Invoice totals must not exceed the sum of their components
-   Payment terms enforcement via due dates
-   Prepayment required for new customers and high-value orders

### Additional Policies

The following policies are also implemented in the system:

-   **CustomerPolicy**: Customer account management rules
-   **NotificationPolicy**: Notification delivery and preferences rules
-   **OrderProcessingPolicy**: Order processing workflow rules
-   **PaymentPolicy**: Payment validation and processing rules
-   **ProductAvailabilityPolicy**: Product availability and stock rules
-   **PromotionPolicy**: Promotion and campaign validation rules
-   **RefundPolicy**: Refund eligibility and processing rules
-   **ShippingZonePolicy**: Shipping zone and rate calculation rules
-   **StockTransactionPolicy**: Inventory transaction validation rules
-   **VendorPolicy**: Vendor management and validation rules
-   **WishlistPolicy**: Wishlist management rules

---

## Specifications

Specifications define complex query criteria for filtering entities.

### ProductSpecification

**Location:** `src/Domain/Specifications/ProductSpecification.cs`

**Purpose:** Filtering criteria for product searches

**Features:**

-   Search by term, category, price range
-   Filter by sale status, featured status, stock availability
-   Tag and brand filtering
-   Minimum rating filter
-   Sorting and pagination
-   Predefined specifications:
    -   Featured products
    -   On-sale products
    -   Low stock products

### OrderSpecification

**Location:** `src/Domain/Specifications/OrderSpecification.cs`

**Purpose:** Filtering criteria for order searches

**Features:**

-   Filter by customer, status, date range, amount range
-   Payment method and shipping method filters
-   Order number and coupon code search
-   Tracking number availability filter
-   Sorting and pagination
-   Predefined specifications:
    -   Pending orders
    -   Customer recent orders
    -   Orders requiring shipment

---

## Value Objects

Value objects represent immutable domain concepts with validation.

### Money

**Location:** `src/Domain/ValueObjects/Money.cs`

**Purpose:** Represents monetary values with currency

**Features:**

-   Amount validation (non-negative)
-   Currency code management
-   Arithmetic operations (add, subtract, multiply)
-   Discount application
-   Comparison operations
-   Enforces same-currency operations
-   Precision: 2 decimal places

### Discount

**Location:** `src/Domain/ValueObjects/Discount.cs`

**Purpose:** Represents discount values (percentage or fixed)

**Features:**

-   Percentage discount (0-100%)
-   Fixed amount discount
-   Calculate discount amount
-   Apply discount to prices
-   Validation logic
-   Human-readable string representation

### EmailAddress

**Location:** `src/Domain/ValueObjects/EmailAddress.cs`

**Purpose:** Represents validated email addresses

**Features:**

-   Email format validation
-   Normalization (lowercase, trimmed)
-   Extract domain and local parts
-   Privacy masking (e.g., j\*\*\*@example.com)
-   Immutable value object

### Sku

**Location:** `src/Domain/ValueObjects/Sku.cs`

**Purpose:** Represents product SKU (Stock Keeping Unit)

**Features:**

-   Length validation (3-50 characters)
-   Format validation (alphanumeric and hyphens)
-   SKU generation from category and product name
-   Category prefix extraction
-   Normalization (uppercase)

---

## Domain Services

Domain services encapsulate complex business logic that doesn't naturally fit in entities.

### DiscountCalculationService

**Location:** `src/Domain/Services/DiscountCalculationService.cs`

**Purpose:** Handles discount calculations and validations

**Capabilities:**

-   Calculate product final prices with discounts
-   Calculate savings amounts
-   Calculate cart totals with item discounts
-   Apply coupon discounts with full validation
-   Calculate bulk discounts based on quantity tiers:
    -   10-50 items: 5% off
    -   51-100 items: 10% off
    -   100+ items: 15% off

### OrderLifecycleService

**Location:** `src/Domain/Services/OrderLifecycleService.cs`

**Purpose:** Manages order lifecycle and state transitions

**Capabilities:**

-   Validate order placement readiness
-   Manage order status transitions
-   Generate unique order numbers
-   Calculate expected delivery dates based on shipping method
-   Calculate shipping costs based on weight and method:
    -   Same Day: $25 base
    -   Next Day: $15 base
    -   Express: $10 base
    -   Standard: $5 base
    -   International: $30 base
    -   Additional $0.50 per kg over 1kg

### ProductPricingService

**Location:** `src/Domain/Services/ProductPricingService.cs`

**Purpose:** Advanced product pricing strategies

**Capabilities:**

-   Validate product pricing
-   Calculate retail price from cost and target margin
-   Calculate profit margins
-   Suggest optimal discount prices based on stock levels
-   Dynamic pricing based on demand and stock
-   Price competitiveness analysis

### FraudDetectionService

**Location:** `src/Domain/Services/FraudDetectionService.cs`

**Purpose:** Detect and assess fraudulent order risks

**Risk Factors (Score 0-100):**

-   High value orders (>$5,000): +20 points
-   Excessive orders (>10 per day): +30 points
-   New customer high value (>$1,000): +25 points
-   Frequent address changes (>3 per day): +15 points
-   Different billing/shipping addresses: +10 points
-   Unverified email: +15 points
-   Expedited shipping on high value: +10 points

**Actions:**

-   Score ≥80: Auto-reject
-   Score ≥50: Flag for review
-   Address consistency validation

### CartManagementService

**Location:** `src/Domain/Services/CartManagementService.cs`

**Purpose:** Shopping cart management and validation

**Capabilities:**

-   Validate adding items to cart (stock, limits, status)
-   Validate cart readiness for checkout
-   Cart expiration management (30 days)
-   Merge anonymous and user carts after login
-   Calculate total cart weight
-   Free shipping qualification (threshold: $100)
-   Suggest products to reach free shipping

---

## Domain Events

Domain events represent significant business occurrences.

### Order Events

**Location:** `src/Domain/Events/OrderEvents.cs`

-   `OrderPlacedEvent` - New order created
-   `OrderStatusChangedEvent` - Order status transition
-   `OrderCancelledEvent` - Order cancelled with refund info
-   `OrderShippedEvent` - Order shipped with tracking
-   `OrderDeliveredEvent` - Order delivery confirmed
-   `HighRiskOrderDetectedEvent` - Fraud detection triggered

### Stock Events

**Location:** `src/Domain/Events/StockEvents.cs`

-   `LowStockAlertEvent` - Stock below minimum level
-   `ProductOutOfStockEvent` - Product completely out of stock
-   `StockReplenishedEvent` - Stock replenished
-   `StockReservedEvent` - Stock reserved for order
-   `StockReleasedEvent` - Reserved stock released

### Product Events

**Location:** `src/Domain/Events/ProductEvents.cs`

-   `ProductCreatedEvent` - New product added
-   `ProductPriceChangedEvent` - Price updated
-   `ProductOnSaleEvent` - Product put on sale
-   `ProductDiscontinuedEvent` - Product discontinued

### Customer Events

**Location:** `src/Domain/Events/CustomerEvents.cs`

-   `ReviewSubmittedEvent` - Customer submitted review
-   `ReviewApprovedEvent` - Review approved by admin
-   `ReviewFlaggedEvent` - Review flagged for moderation
-   `CouponAppliedEvent` - Coupon applied to order
-   `CouponExhaustedEvent` - Coupon usage limit reached

---

## Domain Exceptions

Custom exceptions for business rule violations.

### Stock Exceptions

**Location:** `src/Domain/Exceptions/StockExceptions.cs`

-   `InsufficientStockException` - Not enough stock for requested quantity
-   `OrderQuantityExceededException` - Order exceeds maximum allowed quantity
-   `ProductOutOfStockException` - Product has no stock

### Pricing Exceptions

**Location:** `src/Domain/Exceptions/PricingExceptions.cs`

-   `InvalidPriceException` - Invalid price value or discount price
-   `InvalidCouponException` - Coupon cannot be applied
-   `CouponExhaustedException` - Coupon usage limit reached
-   `MinimumOrderAmountException` - Order below minimum amount

### Order Exceptions

**Location:** `src/Domain/Exceptions/OrderExceptions.cs`

-   `InvalidOrderStatusTransitionException` - Invalid status change
-   `OrderCannotBeCancelledException` - Order not eligible for cancellation
-   `OrderCannotBeRefundedException` - Order not eligible for refund
-   `OrderValidationException` - General order validation failure
-   `FraudulentOrderException` - Order flagged as fraudulent

### Cart Exceptions

**Location:** `src/Domain/Exceptions/CartExceptions.cs`

-   `CartValidationException` - Cart validation failed
-   `CartExpiredException` - Cart has expired
-   `InvalidCartItemException` - Cannot add item to cart
-   `CartLimitExceededException` - Too many items in cart

### Product Exceptions

**Location:** `src/Domain/Exceptions/ProductExceptions.cs`

-   `ProductNotFoundException` - Product not found by ID or SKU
-   `ProductUnavailableException` - Product is inactive
-   `DuplicateSkuException` - SKU already exists

---

## Integration Points

These business rules integrate with:

1. **Controllers/API Layer**: Enforce rules on incoming requests
2. **Application Services**: Coordinate business operations
3. **Repository Layer**: Persist rule validations
4. **Event Handlers**: React to domain events
5. **Background Jobs**: Automated reordering, stock alerts
6. **Notification System**: Alert admins and customers

## Best Practices

1. **Policies** are stateless and contain pure business logic
2. **Specifications** define reusable query criteria
3. **Value Objects** are immutable and self-validating
4. **Domain Services** orchestrate complex operations
5. **Events** enable loose coupling between bounded contexts
6. **Exceptions** provide clear error semantics

---

_Last Updated: August 7, 2026_
