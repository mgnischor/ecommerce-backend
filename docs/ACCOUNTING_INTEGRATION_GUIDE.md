# Integration Guide - Inventory Accounting System

## Initial Setup

### 1. Run Migrations

```bash
# In PowerShell
cd e:\GITHUB\ecommerce-backend
dotnet ef migrations add AddAccountingSystem
dotnet ef database update
```

### 2. Initialize Chart of Accounts

The chart of accounts is automatically initialized by the `DatabaseSeeder.SeedChartOfAccountsAsync()` method, which is called during application startup in `Program.cs`.

The seeding includes 40+ predefined accounts organized by:

-   **Assets**: Cash, Bank, Investments, Inventory, Receivables, Fixed Assets
-   **Liabilities**: Accounts Payable, Salaries Payable, Taxes Payable, Loans
-   **Equity**: Capital, Retained Earnings, Profit/Loss
-   **Revenue**: Sales, Services, Returns, Other Income
-   **Expenses**: COGS, Freight, Personnel, Marketing, Administrative

### 3. Register Services in DI Container

Services are already registered in `Program.cs`:

```csharp
using ECommerce.Application.Services;
using ECommerce.Domain.Interfaces;

// Accounting services are already registered
builder.Services.AddScoped<IAccountingService, AccountingService>();
builder.Services.AddScoped<IInventoryTransactionService, InventoryTransactionService>();
```

## Basic Usage

### Example 1: Record Merchandise Purchase

```csharp
public class PurchaseController : ControllerBase
{
    private readonly IInventoryTransactionService _transactionService;
    private readonly IProductRepository _productRepository;

    public async Task<IActionResult> RegisterPurchase(PurchaseDto dto)
    {
        var product = await _productRepository.GetByIdAsync(dto.ProductId);

        // Record purchase transaction
        var transaction = await _transactionService.RecordTransactionAsync(
            transactionType: InventoryTransactionType.Purchase,
            productId: product.Id,
            productSku: product.Sku,
            productName: product.Name,
            quantity: dto.Quantity,              // positive for entry
            unitCost: dto.UnitCost,
            toLocation: dto.WarehouseLocation,
            createdBy: GetCurrentUserId(),
            documentNumber: dto.InvoiceNumber,
            notes: $"Purchase Invoice {dto.InvoiceNumber} - Supplier {dto.SupplierId}"
        );

        // Update physical inventory
        var aggregate = ProductAggregate.Load(product);
        aggregate.UpdateStock(dto.WarehouseLocation, dto.Quantity);
        await _productRepository.UpdateAsync(product);

        return Ok(new {
            TransactionId = transaction.Id,
            JournalEntryId = transaction.JournalEntryId,
            Message = "Purchase recorded successfully"
        });
    }
}
```

**Generated Accounting Entry:**

```
Debit:  Merchandise Inventory (1.1.03.001)   $ 5,000.00
Credit: Accounts Payable (2.1.01.001)        $ 5,000.00
```

---

### Example 2: Record Sale (Inventory Withdrawal)

```csharp
public class OrderService
{
    private readonly IInventoryTransactionService _transactionService;

    public async Task ProcessOrderFulfillment(OrderAggregate order)
    {
        foreach (var item in order.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            var aggregate = ProductAggregate.Load(product);

            // 1. Withdraw physical inventory
            aggregate.FulfillReservedStock(item.Quantity, "Main Warehouse");

            // 2. Record accounting transaction
            var transaction = await _transactionService.RecordTransactionAsync(
                transactionType: InventoryTransactionType.Sale,
                productId: item.ProductId,
                productSku: item.ProductSku,
                productName: item.ProductName,
                quantity: -item.Quantity,           // negative for withdrawal
                unitCost: product.AverageCost,      // use average cost
                toLocation: "Main Warehouse",
                createdBy: order.Order.CustomerId,
                orderId: order.Order.Id,
                documentNumber: order.Order.OrderNumber
            );

            await _productRepository.UpdateAsync(product);
        }
    }
}
```

**Generated Accounting Entry:**

```
Debit:  COGS (3.1.01.001)                    $ 3,000.00
Credit: Merchandise Inventory (1.1.03.001)   $ 3,000.00
```

---

### Example 3: Sales Return

```csharp
public async Task ProcessSaleReturn(Guid orderId, Guid productId, int quantity)
{
    var product = await _productRepository.GetByIdAsync(productId);
    var order = await _orderRepository.GetByIdAsync(orderId);

    // Record return
    var transaction = await _transactionService.RecordTransactionAsync(
        transactionType: InventoryTransactionType.SaleReturn,
        productId: product.Id,
        productSku: product.Sku,
        productName: product.Name,
        quantity: quantity,                     // positive for return to inventory
        unitCost: product.AverageCost,
        toLocation: "Main Warehouse",
        createdBy: GetCurrentUserId(),
        orderId: orderId,
        documentNumber: order.OrderNumber,
        notes: "Sales return - defective product"
    );

    // Update inventory
    var aggregate = ProductAggregate.Load(product);
    aggregate.UpdateStock("Main Warehouse", quantity);
    await _productRepository.UpdateAsync(product);
}
```

**Generated Accounting Entry:**

```
Debit:  Merchandise Inventory (1.1.03.001)   $ 3,000.00
Credit: COGS (3.1.01.001)                    $ 3,000.00
```

---

### Example 4: Inventory Adjustment

```csharp
public async Task ProcessInventoryAdjustment(Guid productId, int quantityDifference, string reason)
{
    var product = await _productRepository.GetByIdAsync(productId);

    // Record adjustment
    var transaction = await _transactionService.RecordTransactionAsync(
        transactionType: InventoryTransactionType.Adjustment,
        productId: product.Id,
        productSku: product.Sku,
        productName: product.Name,
        quantity: quantityDifference,           // can be + or -
        unitCost: product.AverageCost,
        toLocation: "Main Warehouse",
        createdBy: GetCurrentUserId(),
        notes: $"Inventory adjustment: {reason}"
    );

    // Update inventory
    var aggregate = ProductAggregate.Load(product);
    aggregate.UpdateStock("Main Warehouse", quantityDifference);
    await _productRepository.UpdateAsync(product);
}
```

**Accounting Entry (Overage - positive):**

```
Debit:  Merchandise Inventory (1.1.03.001)   $ 500.00
Credit: Other Operating Income (4.2.01.001)  $ 500.00
```

**Accounting Entry (Shortage - negative):**

```
Debit:  Other Operating Expenses (3.2.01.002) $ 500.00
Credit: Merchandise Inventory (1.1.03.001)   $ 500.00
```

---

### Example 5: Inventory Loss

```csharp
public async Task RegisterInventoryLoss(Guid productId, int quantity, string reason)
{
    var product = await _productRepository.GetByIdAsync(productId);

    // Record loss
    var transaction = await _transactionService.RecordTransactionAsync(
        transactionType: InventoryTransactionType.Loss,
        productId: product.Id,
        productSku: product.Sku,
        productName: product.Name,
        quantity: -quantity,                    // negative for withdrawal
        unitCost: product.AverageCost,
        toLocation: "Main Warehouse",
        fromLocation: "Main Warehouse",
        createdBy: GetCurrentUserId(),
        notes: $"Loss recorded: {reason}"
    );

    // Update inventory
    var aggregate = ProductAggregate.Load(product);
    aggregate.UpdateStock("Main Warehouse", -quantity);
    await _productRepository.UpdateAsync(product);
}
```

**Generated Accounting Entry:**

```
Debit:  Inventory Loss (3.2.01.001)          $ 1,200.00
Credit: Merchandise Inventory (1.1.03.001)   $ 1,200.00
```

## Queries and Reports

### Product Transaction History

```csharp
public async Task<IEnumerable<InventoryTransactionEntity>> GetProductHistory(Guid productId)
{
    return await _transactionService.GetProductTransactionsAsync(productId);
}
```

### Movements by Period

```csharp
public async Task<IEnumerable<InventoryTransactionEntity>> GetMonthlyTransactions()
{
    var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
    var endDate = startDate.AddMonths(1).AddDays(-1);

    return await _transactionService.GetTransactionsByPeriodAsync(startDate, endDate);
}
```

### Account Balance

```csharp
public async Task<decimal> GetAccountBalance(string accountCode)
{
    var account = await _chartOfAccountsRepository
        .GetAllAsync()
        .FirstOrDefaultAsync(a => a.AccountCode == accountCode);

    return account?.Balance ?? 0;
}
```

### Journal Entries for a Period

```csharp
public async Task<IEnumerable<JournalEntryEntity>> GetJournalEntries(DateTime startDate, DateTime endDate)
{
    var entries = await _journalEntryRepository.GetAllAsync();

    return entries
        .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
        .OrderBy(e => e.EntryDate)
        .ThenBy(e => e.EntryNumber);
}
```

## Important Validations

### 1. Verify Double-Entry

```csharp
public async Task<bool> ValidateJournalEntry(Guid journalEntryId)
{
    var entries = await _accountingEntryRepository
        .GetAllAsync()
        .Where(e => e.JournalEntryId == journalEntryId);

    var totalDebit = entries
        .Where(e => e.EntryType == EntryType.Debit)
        .Sum(e => e.Amount);

    var totalCredit = entries
        .Where(e => e.EntryType == EntryType.Credit)
        .Sum(e => e.Amount);

    return totalDebit == totalCredit;
}
```

### 2. Inventory Reconciliation

```csharp
public async Task<bool> ReconcileInventory(Guid productId)
{
    // Physical balance
    var product = await _productRepository.GetByIdAsync(productId);
    var physicalBalance = product.StockQuantity;

    // Accounting balance (based on transactions)
    var transactions = await _transactionService.GetProductTransactionsAsync(productId);
    var accountingBalance = transactions.Sum(t => t.Quantity);

    return physicalBalance == accountingBalance;
}
```

## Error Handling

```csharp
try
{
    var transaction = await _transactionService.RecordTransactionAsync(...);
}
catch (InvalidOperationException ex)
{
    // Business error (e.g., insufficient inventory)
    _logger.LogWarning(ex, "Error recording transaction");
    return BadRequest(new { error = ex.Message });
}
catch (Exception ex)
{
    // Unexpected error
    _logger.LogError(ex, "Critical error processing transaction");
    return StatusCode(500, new { error = "Internal server error" });
}
```

## Best Practices

### 1. Use Database Transactions

```csharp
using var transaction = await _dbContext.Database.BeginTransactionAsync();

try
{
    // 1. Update inventory
    await _productRepository.UpdateAsync(product);

    // 2. Record accounting transaction
    var invTransaction = await _transactionService.RecordTransactionAsync(...);

    // 3. Commit if everything succeeds
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

### 2. Maintain Detailed Logs

```csharp
_logger.LogInformation(
    "Inventory transaction created: {Type} | Product: {ProductSku} | Qty: {Quantity} | Cost: {Cost}",
    transaction.TransactionType,
    transaction.ProductSku,
    transaction.Quantity,
    transaction.TotalCost
);
```

### 3. Implement Audit Trail

All entities already have:

-   `CreatedAt` - Creation timestamp
-   `CreatedBy` - Responsible user
-   `UpdatedAt` - Last update timestamp

### 4. Accounting Period Close

```csharp
public async Task CloseAccountingPeriod(DateTime periodEndDate)
{
    var entries = await _journalEntryRepository
        .GetAllAsync()
        .Where(e => e.EntryDate <= periodEndDate && !e.IsPosted);

    foreach (var entry in entries)
    {
        entry.IsPosted = true;
        entry.PostedAt = DateTime.UtcNow;
        await _journalEntryRepository.UpdateAsync(entry);
    }
}
```

## Next Steps

1. **Implement REST endpoints** for querying journal entries
2. **Create management reports** (Income Statement, Trial Balance, etc.)
3. **Add fiscal validations** (SPED, Electronic Invoice)
4. **Implement additional costs** (freight, import taxes)
5. **Costing methods** (FIFO, Weighted Average, LIFO)

## Support

For accounting questions:

-   See: `docs/ACCOUNTING_SYSTEM.md`
-   Standards: NBC TG 16 (Inventories), NBC TG 26 (Financial Statements)
