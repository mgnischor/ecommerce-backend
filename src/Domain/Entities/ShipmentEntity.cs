using ECommerce.Domain.Enums;

namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a shipment in the e-commerce system.
/// Tracks the physical delivery of orders to customers.
/// </summary>
public class ShipmentEntity
{
    /// <summary>
    /// Unique identifier for the shipment
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// User who created this shipment
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// User who last updated this shipment
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Order associated with this shipment
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Shipping address ID
    /// </summary>
    public Guid ShippingAddressId { get; set; }

    /// <summary>
    /// Shipment tracking number
    /// </summary>
    public string TrackingNumber { get; set; } = string.Empty;

    /// <summary>
    /// Carrier name (e.g., FedEx, UPS, DHL)
    /// </summary>
    public string Carrier { get; set; } = string.Empty;

    /// <summary>
    /// Carrier service type (e.g., Express, Standard)
    /// </summary>
    public string ServiceType { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the shipment
    /// </summary>
    public ShipmentStatus Status { get; set; } = ShipmentStatus.Preparing;

    /// <summary>
    /// Shipping cost
    /// </summary>
    public decimal ShippingCost { get; set; }

    /// <summary>
    /// Weight of the shipment in kg
    /// </summary>
    public decimal Weight { get; set; }

    /// <summary>
    /// Length in cm
    /// </summary>
    public decimal? Length { get; set; }

    /// <summary>
    /// Width in cm
    /// </summary>
    public decimal? Width { get; set; }

    /// <summary>
    /// Height in cm
    /// </summary>
    public decimal? Height { get; set; }

    /// <summary>
    /// Expected delivery date
    /// </summary>
    public DateTime? ExpectedDeliveryDate { get; set; }

    /// <summary>
    /// Actual delivery date
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// Date when shipment was shipped
    /// </summary>
    public DateTime? ShippedAt { get; set; }

    /// <summary>
    /// Tracking URL
    /// </summary>
    public string? TrackingUrl { get; set; }

    /// <summary>
    /// Additional notes about the shipment
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether the shipment is insured
    /// </summary>
    public bool IsInsured { get; set; } = false;

    /// <summary>
    /// Insurance amount
    /// </summary>
    public decimal? InsuranceAmount { get; set; }

    /// <summary>
    /// Whether signature is required on delivery
    /// </summary>
    public bool RequiresSignature { get; set; } = false;

    /// <summary>
    /// Name of person who received the package
    /// </summary>
    public string? ReceivedBy { get; set; }

    /// <summary>
    /// Whether the shipment is deleted (soft delete)
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the shipment was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the shipment was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
