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

_Last Updated: October 27, 2025_
