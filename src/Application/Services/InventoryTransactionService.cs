using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Services;

/// <summary>
/// Service for managing inventory transactions with accounting integration
/// </summary>
public class InventoryTransactionService : IInventoryTransactionService
{
    private readonly IRepository<InventoryTransactionEntity> _transactionRepository;
    private readonly IAccountingService _accountingService;
    private readonly ILogger<InventoryTransactionService> _logger;
    private static int _transactionCounter = 0;
    private static readonly object _lock = new object();

    public InventoryTransactionService(
        IRepository<InventoryTransactionEntity> transactionRepository,
        IAccountingService accountingService,
        ILogger<InventoryTransactionService> logger
    )
    {
        _transactionRepository =
            transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _accountingService =
            accountingService ?? throw new ArgumentNullException(nameof(accountingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<InventoryTransactionEntity> RecordTransactionAsync(
        InventoryTransactionType transactionType,
        Guid productId,
        string productSku,
        string productName,
        int quantity,
        decimal unitCost,
        string toLocation,
        Guid createdBy,
        string? fromLocation = null,
        Guid? orderId = null,
        string? documentNumber = null,
        string? notes = null,
        CancellationToken cancellationToken = default
    )
    {
        // Create inventory transaction
        var transaction = new InventoryTransactionEntity
        {
            Id = Guid.NewGuid(),
            TransactionNumber = GenerateTransactionNumber(transactionType),
            TransactionDate = DateTime.UtcNow,
            TransactionType = transactionType,
            ProductId = productId,
            ProductSku = productSku,
            ProductName = productName,
            FromLocation = fromLocation,
            ToLocation = toLocation,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = Math.Abs(quantity) * unitCost,
            OrderId = orderId,
            DocumentNumber = documentNumber,
            Notes = notes,
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow,
        };

        // Save transaction
        await _transactionRepository.AddAsync(transaction, cancellationToken);

        // Generate corresponding accounting entry
        JournalEntryEntity? journalEntry = null;

        try
        {
            journalEntry = transactionType switch
            {
                InventoryTransactionType.Purchase => await _accountingService.RecordPurchaseAsync(
                    transaction,
                    createdBy,
                    cancellationToken
                ),

                InventoryTransactionType.Sale or InventoryTransactionType.Fulfillment =>
                    await _accountingService.RecordSaleAsync(
                        transaction,
                        createdBy,
                        cancellationToken
                    ),

                InventoryTransactionType.SaleReturn =>
                    await _accountingService.RecordSaleReturnAsync(
                        transaction,
                        createdBy,
                        cancellationToken
                    ),

                InventoryTransactionType.PurchaseReturn =>
                    await _accountingService.RecordPurchaseReturnAsync(
                        transaction,
                        createdBy,
                        cancellationToken
                    ),

                InventoryTransactionType.Adjustment =>
                    await _accountingService.RecordAdjustmentAsync(
                        transaction,
                        createdBy,
                        cancellationToken
                    ),

                InventoryTransactionType.Loss => await _accountingService.RecordLossAsync(
                    transaction,
                    createdBy,
                    cancellationToken
                ),

                // Reservation and release don't generate immediate accounting entries
                InventoryTransactionType.Reservation
                or InventoryTransactionType.ReservationRelease
                or InventoryTransactionType.Transfer => null,

                _ => throw new InvalidOperationException(
                    $"Unsupported transaction type: {transactionType}"
                ),
            };

            if (journalEntry != null)
            {
                transaction.JournalEntryId = journalEntry.Id;
                await _transactionRepository.UpdateAsync(transaction, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't fail inventory transaction
            _logger.LogError(
                ex,
                "Error creating accounting entry for transaction: TransactionType={TransactionType}, ProductId={ProductId}",
                transactionType,
                productId
            );
            transaction.Notes = $"{transaction.Notes} | Accounting error: {ex.Message}";
            await _transactionRepository.UpdateAsync(transaction, cancellationToken);
        }

        _logger.LogInformation(
            "Inventory transaction recorded successfully: TransactionNumber={TransactionNumber}, ProductId={ProductId}, Quantity={Quantity}",
            transaction.TransactionNumber,
            productId,
            quantity
        );

        return transaction;
    }

    public async Task<IEnumerable<InventoryTransactionEntity>> GetProductTransactionsAsync(
        Guid productId,
        CancellationToken cancellationToken = default
    )
    {
        var allTransactions = await _transactionRepository.GetAllAsync(cancellationToken);
        return allTransactions
            .Where(t => t.ProductId == productId)
            .OrderByDescending(t => t.TransactionDate)
            .ToList();
    }

    public async Task<IEnumerable<InventoryTransactionEntity>> GetTransactionsByPeriodAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default
    )
    {
        var allTransactions = await _transactionRepository.GetAllAsync(cancellationToken);
        return allTransactions
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .ToList();
    }

    private static string GenerateTransactionNumber(InventoryTransactionType type)
    {
        lock (_lock)
        {
            _transactionCounter++;
            var prefix = type switch
            {
                InventoryTransactionType.Purchase => "PURCH",
                InventoryTransactionType.Sale => "SALE",
                InventoryTransactionType.SaleReturn => "SALERET",
                InventoryTransactionType.PurchaseReturn => "PURRET",
                InventoryTransactionType.Adjustment => "ADJ",
                InventoryTransactionType.Transfer => "TRAN",
                InventoryTransactionType.Loss => "LOSS",
                InventoryTransactionType.Reservation => "RES",
                InventoryTransactionType.ReservationRelease => "REL",
                InventoryTransactionType.Fulfillment => "FULL",
                _ => "TRANS",
            };

            return $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{_transactionCounter:D6}";
        }
    }
}
