using System.ComponentModel.DataAnnotations;

namespace Comex.API.DTOs;

/// <summary>
/// Data Transfer Object for redeeming an amount from a gift card
/// </summary>
public sealed class GiftCardRedeemRequestDto
{
    /// <summary>
    /// Gets or sets the amount to redeem from the gift card balance
    /// </summary>
    /// <value>The amount to redeem. Must be greater than zero and not exceed the available balance.</value>
    [Required(ErrorMessage = "Redeem amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Redeem amount must be greater than zero")]
    public decimal RedeemAmount { get; set; }
}

/// <summary>
/// Data Transfer Object for reloading a gift card balance
/// </summary>
public sealed class GiftCardReloadRequestDto
{
    /// <summary>
    /// Gets or sets the new total balance after the reload
    /// </summary>
    /// <value>The reloaded balance. Must be between 0.01 and 5000.</value>
    [Required(ErrorMessage = "Reload balance is required")]
    [Range(0.01, 5000, ErrorMessage = "Reload balance must be between 0.01 and 5000")]
    public decimal NewBalance { get; set; }
}
