using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs;

/// <summary>
/// Request payload for creating a new order. Only the fields below are accepted
/// from the client - all other order fields (Id, Status, CreatedAt, totals, etc.)
/// are computed/assigned by the server.
/// </summary>
public sealed class CreateOrderRequestDto
{
    /// <summary>Customer identifier (the order is created on behalf of this customer).</summary>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>Order number / external reference.</summary>
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>Shipping cost in the order currency.</summary>
    [Range(0, double.MaxValue)]
    public decimal ShippingCost { get; set; }

    /// <summary>Tax amount in the order currency.</summary>
    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }

    /// <summary>Total discount amount applied to the order.</summary>
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    /// <summary>Items in the order. Must contain at least one item.</summary>
    [Required]
    [MinLength(1)]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

/// <summary>
/// Request payload for a single item inside a <see cref="CreateOrderRequestDto"/>.
/// </summary>
public sealed class CreateOrderItemDto
{
    /// <summary>Product identifier being ordered.</summary>
    [Required]
    public Guid ProductId { get; set; }

    /// <summary>Snapshot of the product name at order time.</summary>
    [Required]
    [StringLength(256, MinimumLength = 1)]
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Snapshot of the product SKU at order time.</summary>
    [Required]
    [StringLength(64, MinimumLength = 1)]
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>Quantity ordered. Must be greater than zero.</summary>
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>Unit price at the time of order.</summary>
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    /// <summary>Discount amount applied to this item (optional).</summary>
    [Range(0, double.MaxValue)]
    public decimal DiscountAmount { get; set; }

    /// <summary>Tax amount applied to this item (optional).</summary>
    [Range(0, double.MaxValue)]
    public decimal TaxAmount { get; set; }
}
