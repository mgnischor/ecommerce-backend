# Inventory Accounting System - E-Commerce Backend

## Overview

This document describes the implementation of the accounting system for inventory movements, following Brazilian Generally Accepted Accounting Principles (NBC TG) and fundamental accounting principles.

## Applied Accounting Principles

### 1. Double-Entry Bookkeeping Method

Each inventory operation generates an accounting entry with equal debit and credit values, maintaining equity balance.

### 2. Accrual Basis Accounting

Transactions are recorded when they occur, regardless of payment or receipt.

### 3. NBC TG 16 - Inventories

-   Inventories are assets held for sale
-   Measured at the lower of cost and net realizable value
-   Cost includes acquisition costs and other costs to bring inventory to saleable condition

## Chart of Accounts Structure

### Implemented Accounts

```
1. ASSETS
   1.1 Current Assets
       1.1.01.001 - Cash
       1.1.03.001 - Merchandise Inventory

2. LIABILITIES
   2.1 Current Liabilities
       2.1.01.001 - Accounts Payable - Suppliers

3. COST/EXPENSE
   3.1.01.001 - Cost of Goods Sold (COGS)
   3.2.01.001 - Inventory Loss
   3.2.01.002 - Other Operating Expenses

4. REVENUE
   4.2.01.001 - Other Operating Income
```

## Movement Types and Accounting Entries

### 1. Merchandise Purchase

**Type:** `Purchase`

**Accounting Entry:**

```
Debit:  Merchandise Inventory (Asset)        $ XXX.XX
Credit: Accounts Payable - Suppliers (Liability)  $ XXX.XX
```

**Effect:** Increases asset (inventory) and increases liability (debt to supplier)

---

### 2. Merchandise Sale (Withdrawal - COGS)

**Type:** `Sale` or `Fulfillment`

**Accounting Entry:**

```
Debit:  Cost of Goods Sold (COGS)            $ XXX.XX
Credit: Merchandise Inventory (Asset)        $ XXX.XX
```

**Effect:** Increases cost/expense and decreases asset (inventory)

**Note:** This entry records only COGS. Revenue recognition and accounts receivable should be recorded separately in the sales module.

---

### 3. Sales Return

**Type:** `SaleReturn`

**Accounting Entry:**

```
Debit:  Merchandise Inventory (Asset)        $ XXX.XX
Credit: Cost of Goods Sold (COGS)            $ XXX.XX
```

**Effect:** Reverses COGS and returns merchandise to inventory

---

### 4. Purchase Return

**Type:** `PurchaseReturn`

**Accounting Entry:**

```
Debit:  Accounts Payable - Suppliers (Liability)  $ XXX.XX
Credit: Merchandise Inventory (Asset)        $ XXX.XX
```

**Effect:** Decreases supplier debt and decreases inventory

---

### 5. Inventory Adjustment

#### 5.1 Positive Adjustment (Overage)

**Type:** `Adjustment` (positive quantity)

**Accounting Entry:**

```
Debit:  Merchandise Inventory (Asset)        $ XXX.XX
Credit: Other Operating Income               $ XXX.XX
```

**Effect:** Increases inventory and recognizes revenue

#### 5.2 Negative Adjustment (Shortage)

**Type:** `Adjustment` (negative quantity)

**Accounting Entry:**

```
Debit:  Other Operating Expenses             $ XXX.XX
Credit: Merchandise Inventory (Asset)        $ XXX.XX
```

**Effect:** Decreases inventory and recognizes expense

---

### 6. Inventory Loss/Shrinkage

**Type:** `Loss`

**Accounting Entry:**

```
Debit:  Inventory Loss (Expense)             $ XXX.XX
Credit: Merchandise Inventory (Asset)        $ XXX.XX
```

**Effect:** Decreases inventory and recognizes operating expense

---

### 7. Reservation and Release

**Types:** `Reservation` and `ReservationRelease`

**Accounting Entry:** None

**Note:** Reservations only control physical availability without immediate accounting impact. Accounting entry occurs only on actual sale (Fulfillment).

---

### 8. Transfer Between Locations

**Type:** `Transfer`

**Accounting Entry:** None

**Note:** Internal transfers do not change total inventory value, only physical location. No impact on income statement.

## System Entities

### ChartOfAccountsEntity

Defines the chart of accounts:

-   Account code (hierarchical structure)
-   Name and description
-   Account type (Asset, Liability, Equity, Revenue, Expense)
-   Current balance
-   Analytic account and active status indicators

### JournalEntryEntity

Represents a complete accounting entry:

-   Unique sequential number
-   Entry date
-   Document type and number
-   Descriptive history
-   Total amount
-   References (order, product, inventory transaction)
-   Posted status

### AccountingEntryEntity

Individual entry (debit or credit):

-   Reference to journal entry
-   Debited/credited account
-   Entry type (Debit/Credit)
-   Amount
-   Complementary description
-   Cost center (optional)

### InventoryTransactionEntity

Inventory movement record:

-   Sequential transaction number
-   Movement type
-   Product, location, quantity
-   Unit and total costs
-   References to order and fiscal document
-   Link to generated journal entry

## Implemented Services

### IAccountingService

Interface for accounting operations:

-   `RecordPurchaseAsync()` - Records purchases
-   `RecordSaleAsync()` - Records sales COGS
-   `RecordSaleReturnAsync()` - Records sales returns
-   `RecordPurchaseReturnAsync()` - Records purchase returns
-   `RecordAdjustmentAsync()` - Records inventory adjustments
-   `RecordLossAsync()` - Records losses/shrinkage
-   `GetOrCreateAccountAsync()` - Manages chart of accounts

### IInventoryTransactionService

Interface for inventory transactions:

-   `RecordTransactionAsync()` - Records transaction and generates accounting entry
-   `GetProductTransactionsAsync()` - Queries history by product
-   `GetTransactionsByPeriodAsync()` - Queries by period

## Operation Flow

```
1. Inventory Operation (e.g., sale)
   ↓
2. ProductAggregate.FulfillReservedStock()
   ↓
3. InventoryTransactionService.RecordTransactionAsync()
   ↓
4. Creates InventoryTransactionEntity
   ↓
5. AccountingService.RecordSaleAsync()
   ↓
6. Creates JournalEntryEntity
   ↓
7. Creates AccountingEntryEntity (Debit and Credit)
   ↓
8. Updates account balances
   ↓
9. Links entry to transaction
```

## Traceability and Audit

Each movement has:

-   Unique sequential number
-   Exact date and time
-   Responsible user
-   Source document
-   Notes
-   Link to corresponding accounting entry

This enables:

-   Complete operation audit
-   Accounting reconciliation
-   Cost analysis by product
-   Management and fiscal reports
-   Discrepancy tracking

## Possible Accounting Reports

### 1. Journal

Chronological list of all accounting entries

### 2. General Ledger

Movements by accounting account

### 3. Trial Balance

Balances of all accounts at a specific date

### 4. Income Statement

Revenue, costs, and expenses for the period

### 5. Balance Sheet

Assets, liabilities, and equity

### 6. Inventory Card

Movements and balance by product

### 7. COGS Report

Cost of goods sold during the period

## Legal Compliance

The system meets the requirements of:

-   **NBC TG 16 (CPC 16)** - Inventories
-   **NBC TG 26 (CPC 26)** - Presentation of Financial Statements
-   **Law 6.404/76** - Brazilian Corporate Law
-   **Civil Code** - Accounting bookkeeping
-   **Fundamental Accounting Principles**

## Next Steps

1. **Implement additional costs**: Freight, import taxes
2. **Costing methods**: FIFO, LIFO, Weighted Average
3. **Cost centers**: Detailed allocation by department
4. **Automatic reconciliation**: Double-entry validation
5. **Management reports**: Dashboard of accounting indicators
6. **Fiscal integration**: SPED generation, fiscal books
7. **Lot/serial control**: Specific traceability
8. **Conversion costs**: For manufactured products

## Usage Example

```csharp
// Record a sale
var transaction = await _inventoryTransactionService.RecordTransactionAsync(
    transactionType: InventoryTransactionType.Sale,
    productId: productId,
    productSku: "PROD-001",
    productName: "Dell Notebook",
    quantity: -2, // negative for withdrawal
    unitCost: 2500.00m,
    toLocation: "Main Warehouse",
    createdBy: userId,
    orderId: orderId,
    documentNumber: "INV-12345"
);

// Result:
// - InventoryTransaction created
// - JournalEntry created (COGS)
// - 2 AccountingEntries (Debit COGS, Credit Inventory)
// - Account balances updated
// - Complete traceability
```

## Conclusion

The implemented system provides a solid foundation for inventory accounting control, following best practices and Brazilian standards. All operations are traceable, auditable, and automatically generate corresponding accounting entries, ensuring the integrity of financial information.
