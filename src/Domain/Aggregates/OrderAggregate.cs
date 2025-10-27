using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Aggregates;

/// <summary>
/// Order aggregate root - encapsulates order business logic and maintains consistency
/// </summary>
public sealed class OrderAggregate
{
    private readonly List<OrderItemEntity> _items = new();

    /// <summary>
    /// Order entity (aggregate root)
    /// </summary>
    public OrderEntity Order { get; private set; }

    /// <summary>
    /// Order items (read-only collection)
    /// </summary>
    public IReadOnlyList<OrderItemEntity> Items => _items.AsReadOnly();

    /// <summary>
    /// Payment information
    /// </summary>
    public PaymentEntity? Payment { get; private set; }

    /// <summary>
    /// Shipping address
    /// </summary>
    public AddressEntity? ShippingAddress { get; private set; }

    /// <summary>
    /// Billing address
    /// </summary>
    public AddressEntity? BillingAddress { get; private set; }

    private OrderAggregate(OrderEntity order)
    {
        Order = order ?? throw new ArgumentNullException(nameof(order));
    }

    /// <summary>
    /// Creates a new order aggregate
    /// </summary>
    public static OrderAggregate Create(Guid customerId, string orderNumber)
    {
        var order = new OrderEntity
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            OrderNumber = orderNumber,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return new OrderAggregate(order);
    }

    /// <summary>
    /// Adds an item to the order
    /// </summary>
    public void AddItem(Guid productId, string productName, string productSku, int quantity, decimal unitPrice, string? imageUrl = null)
    {
        if (Order.Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot add items to order with status {Order.Status}");

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            // Update existing item quantity
            existingItem.Quantity += quantity;
            existingItem.TotalPrice = existingItem.Quantity * existingItem.UnitPrice - existingItem.DiscountAmount + existingItem.TaxAmount;
            existingItem.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Add new item
            var item = new OrderItemEntity
            {
                Id = Guid.NewGuid(),
                OrderId = Order.Id,
                ProductId = productId,
                ProductName = productName,
                ProductSku = productSku,
                Quantity = quantity,
                UnitPrice = unitPrice,
                ProductImageUrl = imageUrl,
                TotalPrice = quantity * unitPrice,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _items.Add(item);
        }

        RecalculateTotals();
    }

    /// <summary>
    /// Removes an item from the order
    /// </summary>
    public void RemoveItem(Guid productId)
    {
        if (Order.Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot remove items from order with status {Order.Status}");

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            RecalculateTotals();
        }
    }

    /// <summary>
    /// Updates item quantity
    /// </summary>
    public void UpdateItemQuantity(Guid productId, int newQuantity)
    {
        if (Order.Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot update items in order with status {Order.Status}");

        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(newQuantity));

        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not found in order");

        item.Quantity = newQuantity;
        item.TotalPrice = item.Quantity * item.UnitPrice - item.DiscountAmount + item.TaxAmount;
        item.UpdatedAt = DateTime.UtcNow;

        RecalculateTotals();
    }

    /// <summary>
    /// Applies a discount to a specific item
    /// </summary>
    public void ApplyItemDiscount(Guid productId, decimal discountAmount)
    {
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null)
            throw new InvalidOperationException($"Product {productId} not found in order");

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        item.DiscountAmount = discountAmount;
        item.TotalPrice = item.Quantity * item.UnitPrice - item.DiscountAmount + item.TaxAmount;
        item.UpdatedAt = DateTime.UtcNow;

        RecalculateTotals();
    }

    /// <summary>
    /// Applies a coupon discount to the order
    /// </summary>
    public void ApplyCoupon(string couponCode, decimal discountAmount)
    {
        if (string.IsNullOrWhiteSpace(couponCode))
            throw new ArgumentException("Coupon code cannot be empty", nameof(couponCode));

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        Order.CouponCode = couponCode;
        Order.DiscountAmount = discountAmount;
        Order.UpdatedAt = DateTime.UtcNow;

        RecalculateTotals();
    }

    /// <summary>
    /// Sets shipping information
    /// </summary>
    public void SetShipping(ShippingMethod method, decimal cost, AddressEntity address)
    {
        if (address == null)
            throw new ArgumentNullException(nameof(address));

        if (cost < 0)
            throw new ArgumentException("Shipping cost cannot be negative", nameof(cost));

        Order.ShippingMethod = method;
        Order.ShippingCost = cost;
        Order.ShippingAddressId = address.Id;
        Order.UpdatedAt = DateTime.UtcNow;

        ShippingAddress = address;

        RecalculateTotals();
    }

    /// <summary>
    /// Sets billing address
    /// </summary>
    public void SetBillingAddress(AddressEntity address)
    {
        if (address == null)
            throw new ArgumentNullException(nameof(address));

        Order.BillingAddressId = address.Id;
        Order.UpdatedAt = DateTime.UtcNow;

        BillingAddress = address;
    }

    /// <summary>
    /// Sets payment method
    /// </summary>
    public void SetPaymentMethod(PaymentMethod method)
    {
        Order.PaymentMethod = method;
        Order.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Confirms the order
    /// </summary>
    public void Confirm()
    {
        if (Order.Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot confirm order with status {Order.Status}");

        if (_items.Count == 0)
            throw new InvalidOperationException("Cannot confirm order with no items");

        if (Order.ShippingAddressId == null)
            throw new InvalidOperationException("Shipping address must be set before confirming order");

        Order.Status = OrderStatus.Confirmed;
        Order.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks order as processing
    /// </summary>
    public void Process()
    {
        if (Order.Status != OrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot process order with status {Order.Status}");

        Order.Status = OrderStatus.Processing;
        Order.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Ships the order
    /// </summary>
    public void Ship(string trackingNumber, DateTime? expectedDeliveryDate = null)
    {
        if (Order.Status != OrderStatus.Processing)
            throw new InvalidOperationException($"Cannot ship order with status {Order.Status}");

        if (string.IsNullOrWhiteSpace(trackingNumber))
            throw new ArgumentException("Tracking number is required", nameof(trackingNumber));

        Order.Status = OrderStatus.Shipped;
        Order.TrackingNumber = trackingNumber;
        Order.ExpectedDeliveryDate = expectedDeliveryDate;
        Order.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks order as delivered
    /// </summary>
    public void Deliver()
    {
        if (Order.Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot deliver order with status {Order.Status}");

        Order.Status = OrderStatus.Delivered;
        Order.DeliveredAt = DateTime.UtcNow;
        Order.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancels the order
    /// </summary>
    public void Cancel(string reason)
    {
        if (Order.Status == OrderStatus.Delivered)
            throw new InvalidOperationException("Cannot cancel a delivered order");

        if (Order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled");

        Order.Status = OrderStatus.Cancelled;
        Order.CancelledAt = DateTime.UtcNow;
        Order.CancellationReason = reason;
        Order.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Adds payment information
    /// </summary>
    public void AddPayment(PaymentEntity payment)
    {
        if (payment == null)
            throw new ArgumentNullException(nameof(payment));

        payment.OrderId = Order.Id;
        Payment = payment;
    }

    /// <summary>
    /// Calculates tax for the order
    /// </summary>
    public void CalculateTax(decimal taxRate)
    {
        if (taxRate < 0)
            throw new ArgumentException("Tax rate cannot be negative", nameof(taxRate));

        Order.TaxAmount = Order.SubTotal * taxRate;
        Order.UpdatedAt = DateTime.UtcNow;

        RecalculateTotals();
    }

    /// <summary>
    /// Recalculates all order totals
    /// </summary>
    private void RecalculateTotals()
    {
        Order.SubTotal = _items.Sum(i => i.TotalPrice);
        Order.TotalAmount = Order.SubTotal + Order.TaxAmount + Order.ShippingCost - Order.DiscountAmount;
        Order.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Validates the order state
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (_items.Count == 0)
            errors.Add("Order must have at least one item");

        if (Order.CustomerId == Guid.Empty)
            errors.Add("Customer ID is required");

        if (string.IsNullOrWhiteSpace(Order.OrderNumber))
            errors.Add("Order number is required");

        if (Order.TotalAmount < 0)
            errors.Add("Total amount cannot be negative");

        return errors.Count == 0;
    }
}
