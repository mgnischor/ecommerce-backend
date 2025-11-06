using ECommerce.Domain.Entities;
using ECommerce.Domain.Events;
using ECommerce.Domain.Policies;

namespace ECommerce.Domain.Aggregates;

/// <summary>
/// Product aggregate root - encapsulates product business logic with inventory
/// </summary>
public sealed class ProductAggregate
{
    private readonly List<ReviewEntity> _reviews = new();
    private readonly List<InventoryEntity> _inventoryLocations = new();
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Product entity (aggregate root)
    /// </summary>
    public ProductEntity Product { get; private set; }

    /// <summary>
    /// Product reviews (read-only collection)
    /// </summary>
    public IReadOnlyList<ReviewEntity> Reviews => _reviews.AsReadOnly();

    /// <summary>
    /// Inventory locations (read-only collection)
    /// </summary>
    public IReadOnlyList<InventoryEntity> InventoryLocations => _inventoryLocations.AsReadOnly();

    /// <summary>
    /// Average rating based on approved reviews
    /// </summary>
    public decimal AverageRating =>
        _reviews.Any(r => r.IsApproved)
            ? (decimal)_reviews.Where(r => r.IsApproved).Average(r => r.Rating)
            : 0;

    /// <summary>
    /// Total approved reviews count
    /// </summary>
    public int ReviewsCount => _reviews.Count(r => r.IsApproved);

    /// <summary>
    /// Total available stock across all locations
    /// </summary>
    public int TotalAvailableStock => _inventoryLocations.Sum(i => i.QuantityAvailable);

    /// <summary>
    /// Whether product is in stock
    /// </summary>
    public bool IsInStock => TotalAvailableStock > 0;

    /// <summary>
    /// Domain events collection (read-only)
    /// </summary>
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private ProductAggregate(ProductEntity product)
    {
        Product = product ?? throw new ArgumentNullException(nameof(product));
    }

    /// <summary>
    /// Loads an existing product with related data
    /// </summary>
    public static ProductAggregate Load(
        ProductEntity product,
        IEnumerable<ReviewEntity>? reviews = null,
        IEnumerable<InventoryEntity>? inventory = null
    )
    {
        var aggregate = new ProductAggregate(product);

        if (reviews != null)
            aggregate._reviews.AddRange(reviews);

        if (inventory != null)
            aggregate._inventoryLocations.AddRange(inventory);

        return aggregate;
    }

    /// <summary>
    /// Adds a review to the product
    /// </summary>
    public void AddReview(
        Guid customerId,
        int rating,
        string title,
        string comment,
        Guid? orderId = null
    )
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentException("Rating must be between 1 and 5", nameof(rating));

        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Review title is required", nameof(title));

        if (string.IsNullOrWhiteSpace(comment))
            throw new ArgumentException("Review comment is required", nameof(comment));

        var review = new ReviewEntity
        {
            Id = Guid.NewGuid(),
            ProductId = Product.Id,
            CustomerId = customerId,
            OrderId = orderId,
            Rating = rating,
            Title = title,
            Comment = comment,
            IsVerifiedPurchase = orderId.HasValue,
            IsApproved = false, // Reviews need approval
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _reviews.Add(review);
    }

    /// <summary>
    /// Approves a review
    /// </summary>
    public void ApproveReview(Guid reviewId)
    {
        var review = _reviews.FirstOrDefault(r => r.Id == reviewId);
        if (review == null)
            throw new InvalidOperationException($"Review {reviewId} not found");

        review.IsApproved = true;
        review.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds an admin response to a review
    /// </summary>
    public void RespondToReview(Guid reviewId, string response)
    {
        if (string.IsNullOrWhiteSpace(response))
            throw new ArgumentException("Response cannot be empty", nameof(response));

        var review = _reviews.FirstOrDefault(r => r.Id == reviewId);
        if (review == null)
            throw new InvalidOperationException($"Review {reviewId} not found");

        review.AdminResponse = response;
        review.AdminRespondedAt = DateTime.UtcNow;
        review.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds inventory for a location
    /// </summary>
    public void AddInventoryLocation(
        string location,
        int quantity,
        int reorderLevel = 10,
        int reorderQuantity = 50
    )
    {
        if (string.IsNullOrWhiteSpace(location))
            throw new ArgumentException("Location is required", nameof(location));

        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

        var existingLocation = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (existingLocation != null)
            throw new InvalidOperationException(
                $"Inventory already exists for location {location}"
            );

        var inventory = new InventoryEntity
        {
            Id = Guid.NewGuid(),
            ProductId = Product.Id,
            Location = location,
            QuantityInStock = quantity,
            QuantityReserved = 0,
            QuantityAvailable = quantity,
            ReorderLevel = reorderLevel,
            ReorderQuantity = reorderQuantity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        _inventoryLocations.Add(inventory);
    }

    /// <summary>
    /// Updates stock quantity for a location
    /// </summary>
    /// <remarks>
    /// DEPRECATED: Use specific transaction methods (RecordPurchase, RecordAdjustment, etc.) for better traceability
    /// </remarks>
    public void UpdateStock(string location, int quantityChange)
    {
        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        var newQuantity = inventory.QuantityInStock + quantityChange;
        if (newQuantity < 0)
            throw new InvalidOperationException(
                $"Insufficient stock. Available: {inventory.QuantityInStock}, Requested: {Math.Abs(quantityChange)}"
            );

        inventory.QuantityInStock = newQuantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;

        if (quantityChange > 0)
        {
            inventory.LastStockReceived = DateTime.UtcNow;
            AddDomainEvent(
                new StockReplenishedEvent(
                    Product.Id,
                    Product.Name,
                    quantityChange,
                    inventory.QuantityInStock
                )
            );
        }

        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();
    }

    /// <summary>
    /// Reserves stock for an order
    /// </summary>
    public void ReserveStock(int quantity, Guid orderId, string location = "Main Warehouse")
    {
        var validation = StockTransactionPolicy.CanRecordReservation(
            quantity,
            _inventoryLocations.FirstOrDefault(i => i.Location == location)?.QuantityAvailable ?? 0,
            orderId
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        inventory.QuantityReserved += quantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;
        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new StockReservedEvent(orderId, Product.Id, quantity, inventory.QuantityAvailable)
        );
    }

    /// <summary>
    /// Releases reserved stock
    /// </summary>
    public void ReleaseReservedStock(int quantity, Guid orderId, string location = "Main Warehouse")
    {
        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        var validation = StockTransactionPolicy.CanReleaseReservation(
            quantity,
            inventory.QuantityReserved,
            orderId
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        inventory.QuantityReserved -= quantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;
        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new StockReleasedEvent(orderId, Product.Id, quantity, inventory.QuantityInStock)
        );
    }

    /// <summary>
    /// Fulfills reserved stock (decreases actual stock)
    /// </summary>
    public void FulfillReservedStock(int quantity, Guid orderId, string location = "Main Warehouse")
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        if (inventory.QuantityReserved < quantity)
            throw new InvalidOperationException(
                $"Cannot fulfill more than reserved. Reserved: {inventory.QuantityReserved}, Requested: {quantity}"
            );

        var validation = StockTransactionPolicy.CanRecordSale(
            quantity,
            inventory.QuantityReserved,
            location
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        inventory.QuantityInStock -= quantity;
        inventory.QuantityReserved -= quantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;
        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new StockFulfilledEvent(
                orderId,
                Product.Id,
                Product.Name,
                quantity,
                inventory.QuantityInStock,
                location
            )
        );

        // Check for low stock after fulfillment
        if (StockManagementPolicy.IsLowStock(inventory.QuantityAvailable, inventory.ReorderLevel))
        {
            AddDomainEvent(
                new LowStockAlertEvent(
                    Product.Id,
                    Product.Name,
                    inventory.QuantityAvailable,
                    inventory.ReorderLevel
                )
            );
        }

        // Check for out of stock
        if (StockManagementPolicy.IsOutOfStock(inventory.QuantityAvailable))
        {
            AddDomainEvent(new ProductOutOfStockEvent(Product.Id, Product.Name, Product.Sku));
        }
    }

    /// <summary>
    /// Applies a discount to the product
    /// </summary>
    public void ApplyDiscount(decimal discountPrice)
    {
        if (discountPrice < 0)
            throw new ArgumentException("Discount price cannot be negative", nameof(discountPrice));

        if (discountPrice >= Product.Price)
            throw new ArgumentException(
                "Discount price must be less than regular price",
                nameof(discountPrice)
            );

        Product.DiscountPrice = discountPrice;
        Product.IsOnSale = true;
        Product.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Removes discount from the product
    /// </summary>
    public void RemoveDiscount()
    {
        Product.DiscountPrice = null;
        Product.IsOnSale = false;
        Product.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks product as featured
    /// </summary>
    public void MarkAsFeatured()
    {
        Product.IsFeatured = true;
        Product.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unmarks product as featured
    /// </summary>
    public void UnmarkAsFeatured()
    {
        Product.IsFeatured = false;
        Product.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates the product
    /// </summary>
    public void Activate()
    {
        Product.IsActive = true;
        Product.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the product
    /// </summary>
    public void Deactivate()
    {
        Product.IsActive = false;
        Product.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates product stock status based on inventory
    /// </summary>
    private void UpdateProductStockStatus()
    {
        Product.StockQuantity = TotalAvailableStock;

        if (!IsInStock && Product.IsActive)
        {
            Product.IsActive = false; // Auto-deactivate if out of stock
        }
    }

    /// <summary>
    /// Checks if product needs reorder
    /// </summary>
    public bool NeedsReorder(out List<string> locations)
    {
        locations = _inventoryLocations
            .Where(i => i.QuantityAvailable <= i.ReorderLevel)
            .Select(i => i.Location)
            .ToList();

        return locations.Any();
    }

    /// <summary>
    /// Validates the product aggregate state
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (string.IsNullOrWhiteSpace(Product.Name))
            errors.Add("Product name is required");

        if (string.IsNullOrWhiteSpace(Product.Sku))
            errors.Add("Product SKU is required");

        if (Product.Price < 0)
            errors.Add("Product price cannot be negative");

        if (Product.DiscountPrice.HasValue && Product.DiscountPrice.Value >= Product.Price)
            errors.Add("Discount price must be less than regular price");

        if (Product.StockQuantity < 0)
            errors.Add("Stock quantity cannot be negative");

        return errors.Count == 0;
    }

    /// <summary>
    /// Adds a domain event to the aggregate
    /// </summary>
    private void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Records a stock purchase transaction
    /// </summary>
    public void RecordPurchase(
        int quantity,
        decimal unitCost,
        string location,
        string documentNumber,
        string? notes = null
    )
    {
        var validation = StockTransactionPolicy.CanRecordPurchase(
            quantity,
            unitCost,
            location,
            documentNumber
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        inventory.QuantityInStock += quantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;
        inventory.LastStockReceived = DateTime.UtcNow;
        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new StockPurchasedEvent(
                Product.Id,
                Product.Name,
                quantity,
                unitCost,
                quantity * unitCost,
                location,
                documentNumber
            )
        );
    }

    /// <summary>
    /// Records a stock transfer between locations
    /// </summary>
    public void RecordTransfer(
        int quantity,
        string fromLocation,
        string toLocation,
        string? notes = null
    )
    {
        var fromInventory = _inventoryLocations.FirstOrDefault(i => i.Location == fromLocation);
        if (fromInventory == null)
            throw new InvalidOperationException($"Source inventory not found: {fromLocation}");

        var validation = StockTransactionPolicy.CanRecordTransfer(
            quantity,
            fromInventory.QuantityAvailable,
            fromLocation,
            toLocation
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        var toInventory = _inventoryLocations.FirstOrDefault(i => i.Location == toLocation);
        if (toInventory == null)
        {
            // Create destination location if it doesn't exist
            AddInventoryLocation(toLocation, 0);
            toInventory = _inventoryLocations.First(i => i.Location == toLocation);
        }

        // Remove from source
        fromInventory.QuantityInStock -= quantity;
        fromInventory.QuantityAvailable =
            fromInventory.QuantityInStock - fromInventory.QuantityReserved;
        fromInventory.UpdatedAt = DateTime.UtcNow;

        // Add to destination
        toInventory.QuantityInStock += quantity;
        toInventory.QuantityAvailable = toInventory.QuantityInStock - toInventory.QuantityReserved;
        toInventory.LastStockReceived = DateTime.UtcNow;
        toInventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new StockTransferredEvent(Product.Id, Product.Name, quantity, fromLocation, toLocation)
        );
    }

    /// <summary>
    /// Records a stock adjustment (physical count correction)
    /// </summary>
    public void RecordAdjustment(string location, int adjustmentQuantity, string reason)
    {
        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        var validation = StockTransactionPolicy.CanRecordAdjustment(
            inventory.QuantityInStock,
            adjustmentQuantity,
            reason
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        var previousStock = inventory.QuantityInStock;

        inventory.QuantityInStock += adjustmentQuantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;
        inventory.LastInventoryCount = DateTime.UtcNow;
        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new StockAdjustedEvent(
                Product.Id,
                Product.Name,
                adjustmentQuantity,
                previousStock,
                inventory.QuantityInStock,
                location,
                reason
            )
        );
    }

    /// <summary>
    /// Records a stock loss (damage, theft, spoilage)
    /// </summary>
    public void RecordLoss(string location, int quantity, decimal estimatedUnitCost, string reason)
    {
        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        var validation = StockTransactionPolicy.CanRecordLoss(
            quantity,
            inventory.QuantityInStock,
            reason
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        inventory.QuantityInStock -= quantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;
        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new StockLossEvent(
                Product.Id,
                Product.Name,
                quantity,
                quantity * estimatedUnitCost,
                location,
                reason
            )
        );
    }

    /// <summary>
    /// Records a purchase return
    /// </summary>
    public void RecordPurchaseReturn(
        string location,
        int quantity,
        decimal unitCost,
        string? reason = null
    )
    {
        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        var validation = StockTransactionPolicy.CanRecordReturn(
            quantity,
            unitCost,
            location,
            null,
            isPurchaseReturn: true
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        inventory.QuantityInStock -= quantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;
        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new PurchaseReturnedEvent(
                Product.Id,
                Product.Name,
                quantity,
                unitCost,
                location,
                reason
            )
        );
    }

    /// <summary>
    /// Records a sale return
    /// </summary>
    public void RecordSaleReturn(string location, int quantity, decimal unitCost, Guid orderId)
    {
        var validation = StockTransactionPolicy.CanRecordReturn(
            quantity,
            unitCost,
            location,
            orderId,
            isPurchaseReturn: false
        );

        if (!validation.isValid)
            throw new InvalidOperationException(validation.errorMessage);

        var inventory = _inventoryLocations.FirstOrDefault(i => i.Location == location);
        if (inventory == null)
            throw new InvalidOperationException($"Inventory not found for location {location}");

        inventory.QuantityInStock += quantity;
        inventory.QuantityAvailable = inventory.QuantityInStock - inventory.QuantityReserved;
        inventory.LastStockReceived = DateTime.UtcNow;
        inventory.UpdatedAt = DateTime.UtcNow;

        UpdateProductStockStatus();

        AddDomainEvent(
            new SaleReturnedEvent(orderId, Product.Id, Product.Name, quantity, unitCost, location)
        );
    }
}
