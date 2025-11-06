namespace ECommerce.Domain.Entities;

/// <summary>
/// Represents a customer address for shipping or billing purposes in the e-commerce system.
/// </summary>
/// <remarks>
/// This entity supports multiple address types per customer and includes soft delete capability.
/// Addresses can be marked as default for streamlined checkout processes.
/// </remarks>
public sealed class AddressEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this address.
    /// </summary>
    /// <value>A <see cref="Guid"/> that uniquely identifies the address.</value>
    /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the customer who owns this address.
    /// </summary>
    /// <value>A <see cref="Guid"/> referencing the customer (user) entity.</value>
    /// <example>7c9e6679-7425-40de-944b-e07fc1f90ae7</example>
    public Guid CustomerId { get; set; }

    /// <summary>
    /// Gets or sets the type label for this address.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> categorizing the address usage.
    /// Defaults to "Shipping".
    /// </value>
    /// <example>Home, Work, Billing, Shipping</example>
    public string AddressType { get; set; } = "Shipping";

    /// <summary>
    /// Gets or sets the full name of the recipient at this address.
    /// </summary>
    /// <value>A <see cref="string"/> containing the recipient's complete name.</value>
    /// <example>John Smith</example>
    public string FullName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact phone number for this address.
    /// </summary>
    /// <value>A <see cref="string"/> containing the phone number, including country code if applicable.</value>
    /// <example>+1-555-123-4567</example>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first line of the street address.
    /// </summary>
    /// <value>A <see cref="string"/> containing the primary street address information.</value>
    /// <example>123 Main Street</example>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional second line of the street address.
    /// </summary>
    /// <value>
    /// A <see cref="string"/> containing additional address details such as apartment or suite number,
    /// or <c>null</c> if not applicable.
    /// </value>
    /// <example>Apt 4B, Suite 200, Building C</example>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Gets or sets the city name.
    /// </summary>
    /// <value>A <see cref="string"/> containing the city or locality name.</value>
    /// <example>New York, Los Angeles, London</example>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the state, province, or region.
    /// </summary>
    /// <value>A <see cref="string"/> containing the state, province, or administrative region.</value>
    /// <example>CA, NY, São Paulo, Ontario</example>
    public string State { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the postal code or ZIP code.
    /// </summary>
    /// <value>A <see cref="string"/> containing the postal or ZIP code.</value>
    /// <example>90210, 10001, SW1A 1AA</example>
    public string PostalCode { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the country name or code.
    /// </summary>
    /// <value>A <see cref="string"/> containing the country name or ISO country code.</value>
    /// <example>United States, USA, BR, GB</example>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this is the customer's default address.
    /// </summary>
    /// <value>
    /// <c>true</c> if this address should be used by default for shipping or billing;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether this address has been soft-deleted.
    /// </summary>
    /// <value>
    /// <c>true</c> if the address is deleted but retained for historical purposes;
    /// otherwise, <c>false</c>. Defaults to <c>false</c>.
    /// </value>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the date and time when this address was created.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the creation timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the date and time when this address was last updated.
    /// </summary>
    /// <value>
    /// A <see cref="DateTime"/> in UTC format representing the last modification timestamp.
    /// Defaults to <see cref="DateTime.UtcNow"/> when the entity is instantiated.
    /// </value>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
