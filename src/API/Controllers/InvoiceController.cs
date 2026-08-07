using System.Security.Claims;
using ECommerce.API.Constants;
using ECommerce.API.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Application.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Policies;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Provides HTTP endpoints for managing invoices.
/// Supports creating, retrieving, updating, paying, issuing credit notes, and deleting invoices with role-based access control.
/// </summary>
/// <remarks>
/// This controller handles all invoice operations including:
/// <list type="bullet">
/// <item><description>Creating invoices with validated invoice numbers and calculated totals</description></item>
/// <item><description>Recording payments and checking payment terms</description></item>
/// <item><description>Issuing credit notes for paid invoices within the credit period</description></item>
/// <item><description>Soft-delete support for maintaining historical records</description></item>
/// </list>
/// All business rules are enforced via <see cref="InvoicePolicy"/>.
/// All responses are in JSON format and follow RESTful conventions.
/// </remarks>
/// <remarks>
/// Initializes a new instance of the <see cref="InvoiceController"/> class.
/// </remarks>
/// <param name="context">The database context for data access operations.</param>
/// <param name="logger">The logger for recording operational events and errors.</param>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="context"/> or <paramref name="logger"/> is null.
/// </exception>
[Tags("Invoices")]
[ApiController]
[Route("api/v1/invoices")]
[Produces("application/json")]
public sealed class InvoiceController(
    PostgresqlContext context,
    LoggingService<InvoiceController> logger
) : ControllerBase
{
    /// <summary>
    /// The database context for accessing invoice data.
    /// </summary>
    private readonly PostgresqlContext _context =
        context ?? throw new ArgumentNullException(nameof(context));

    /// <summary>
    /// Logger instance for recording controller operations and errors.
    /// </summary>
    private readonly ILoggingService _logger =
        logger ?? throw new ArgumentNullException(nameof(logger));

    /// <summary>
    /// Retrieves the unique identifier of the currently authenticated user.
    /// </summary>
    /// <returns>
    /// The user ID from the JWT token claims, or null if the user is not authenticated.
    /// </returns>
    private string? GetCurrentUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    /// <summary>
    /// Retrieves a specific invoice by its unique identifier.
    /// </summary>
    /// <param name="id">The unique GUID of the invoice to retrieve.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The invoice entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Retrieves complete invoice details including amounts, payment status, and dates.
    /// Excludes soft-deleted invoices.
    /// </remarks>
    /// <response code="200">Returns the invoice entity.</response>
    /// <response code="400">Invalid invoice ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Invoice not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(InvoiceEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceEntity>> GetInvoiceById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid invoice GUID provided");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var invoice = await _context
                .Invoices.AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted, cancellationToken);

            if (invoice == null)
            {
                _logger.LogInformation("Invoice not found: {InvoiceId}", id);
                return NotFound(new { Message = ErrorMessages.InvoiceNotFound });
            }

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice: {InvoiceId}", id);
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Retrieves an invoice by its invoice number.
    /// </summary>
    /// <param name="invoiceNumber">
    /// The invoice number to look up. Must be between 5 and 50 characters (letters, digits, dashes, or slashes).
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The invoice entity if found; otherwise, a 404 Not Found response.
    /// </returns>
    /// <remarks>
    /// Useful for customer billing lookups and external accounting system integration.
    /// Input is validated against <see cref="InvoicePolicy.IsValidInvoiceNumber"/> before querying.
    /// </remarks>
    /// <response code="200">Returns the invoice entity.</response>
    /// <response code="400">Invalid invoice number format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="404">Invoice not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpGet("number/{invoiceNumber}")]
    [Authorize]
    [ProducesResponseType(typeof(InvoiceEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceEntity>> GetInvoiceByNumber(
        string invoiceNumber,
        CancellationToken cancellationToken = default
    )
    {
        if (!InvoicePolicy.IsValidInvoiceNumber(invoiceNumber))
        {
            _logger.LogWarning("Invalid invoice number format provided");
            return BadRequest(new { Message = ErrorMessages.InvalidInvoiceNumber });
        }

        try
        {
            var invoice = await _context
                .Invoices.AsNoTracking()
                .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber && !i.IsDeleted, cancellationToken);

            if (invoice == null)
            {
                _logger.LogInformation("Invoice number not found: {InvoiceNumber}", invoiceNumber);
                return NotFound(new { Message = ErrorMessages.InvoiceNotFound });
            }

            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving invoice by number");
            return StatusCode(500, ErrorMessages.ProcessingRequestError);
        }
    }

    /// <summary>
    /// Creates a new invoice with the specified amounts and payment terms.
    /// </summary>
    /// <param name="invoice">
    /// The invoice entity containing the invoice number, amounts, and due date.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The newly created invoice entity with server-generated ID and timestamps.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Invoice number must be 5-50 characters (letters, digits, dashes, or slashes)</description></item>
    /// <item><description>Invoice number must be unique across all non-deleted invoices</description></item>
    /// <item><description>Amounts must not be negative</description></item>
    /// <item><description>Order and customer IDs must be provided</description></item>
    /// </list>
    /// <para><b>Automatic Field Assignment:</b></para>
    /// <list type="bullet">
    /// <item><description>Total: Calculated via <see cref="InvoicePolicy.CalculateInvoiceTotal"/></description></item>
    /// <item><description>Id: Auto-generated GUID</description></item>
    /// <item><description>CreatedAt/UpdatedAt: Set to current UTC time</description></item>
    /// <item><description>CreatedBy: Set from authenticated user's ID</description></item>
    /// </list>
    /// </remarks>
    /// <response code="201">Invoice created successfully. Returns the created invoice with Location header.</response>
    /// <response code="400">Invalid input data or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="409">Invoice number already exists.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(InvoiceEntity), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceEntity>> CreateInvoice(
        [FromBody] InvoiceEntity invoice,
        CancellationToken cancellationToken = default
    )
    {
        if (invoice == null)
        {
            _logger.LogWarning("Null invoice data received");
            return BadRequest(new { Message = ErrorMessages.InvoiceDataRequired });
        }

        if (!InvoicePolicy.IsValidInvoiceNumber(invoice.InvoiceNumber))
        {
            _logger.LogWarning("Invalid invoice number provided");
            return BadRequest(new { Message = ErrorMessages.InvalidInvoiceNumber });
        }

        if (
            invoice.Subtotal < 0
            || invoice.TaxAmount < 0
            || invoice.ShippingCost < 0
            || invoice.PaidAmount < 0
        )
        {
            _logger.LogWarning("Negative invoice amounts provided");
            return BadRequest(new { Message = ErrorMessages.InvalidInvoiceAmounts });
        }

        if (invoice.OrderId == Guid.Empty || invoice.CustomerId == Guid.Empty)
        {
            _logger.LogWarning("Missing order or customer ID for invoice");
            return BadRequest(new { Message = ErrorMessages.InvalidCustomerId });
        }

        try
        {
            var duplicateNumber = await _context.Invoices.AnyAsync(
                i => i.InvoiceNumber == invoice.InvoiceNumber && !i.IsDeleted,
                cancellationToken
            );

            if (duplicateNumber)
            {
                _logger.LogWarning("Duplicate invoice number attempt: {InvoiceNumber}", invoice.InvoiceNumber);
                return Conflict(new { Message = ErrorMessages.InvoiceNumberAlreadyExists });
            }

            var total = InvoicePolicy.CalculateInvoiceTotal(
                invoice.Subtotal,
                invoice.TaxAmount,
                invoice.ShippingCost
            );

            var newInvoice = new InvoiceEntity
            {
                Id = Guid.NewGuid(),
                InvoiceNumber = invoice.InvoiceNumber,
                OrderId = invoice.OrderId,
                CustomerId = invoice.CustomerId,
                Subtotal = invoice.Subtotal,
                TaxAmount = invoice.TaxAmount,
                ShippingCost = invoice.ShippingCost,
                Total = total,
                PaidAmount = invoice.PaidAmount,
                IsPaid = InvoicePolicy.IsInvoicePaid(invoice.PaidAmount, total),
                DueDate = invoice.DueDate == default ? DateTime.UtcNow.AddDays(30) : invoice.DueDate,
                IssuedAt = DateTime.UtcNow,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(GetCurrentUserId(), out var userId) ? userId : Guid.Empty,
            };

            _context.Invoices.Add(newInvoice);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Invoice created: {InvoiceId}, Number: {InvoiceNumber}, User: {UserId}",
                newInvoice.Id,
                newInvoice.InvoiceNumber,
                GetCurrentUserId() ?? "Unknown"
            );

            return CreatedAtAction(nameof(GetInvoiceById), new { id = newInvoice.Id }, newInvoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating invoice with number: {InvoiceNumber}", invoice.InvoiceNumber);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Updates an existing invoice's amounts, payment terms, and payment status.
    /// </summary>
    /// <param name="id">The unique GUID of the invoice to update.</param>
    /// <param name="invoice">
    /// The invoice entity with updated values. The Id must match the route parameter.
    /// </param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Updatable Fields:</b></para>
    /// <list type="bullet">
    /// <item><description>Subtotal, TaxAmount, ShippingCost (Total recalculated)</description></item>
    /// <item><description>PaidAmount (IsPaid recalculated via <see cref="InvoicePolicy.IsInvoicePaid"/>)</description></item>
    /// <item><description>DueDate</description></item>
    /// </list>
    /// <para><b>Protected Fields (Not Updated):</b></para>
    /// <list type="bullet">
    /// <item><description>Id: Cannot be changed</description></item>
    /// <item><description>InvoiceNumber, OrderId, CustomerId: Cannot be changed</description></item>
    /// <item><description>CreatedAt, CreatedBy: Audit fields remain unchanged</description></item>
    /// </list>
    /// </remarks>
    /// <response code="204">Invoice updated successfully. No content returned.</response>
    /// <response code="400">Invalid input data, ID mismatch, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Invoice not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateInvoice(
        Guid id,
        [FromBody] InvoiceEntity invoice,
        CancellationToken cancellationToken = default
    )
    {
        if (invoice == null)
        {
            _logger.LogWarning("Null invoice data received for update");
            return BadRequest(new { Message = ErrorMessages.InvoiceDataRequired });
        }

        if (id == Guid.Empty || id != invoice.Id)
        {
            _logger.LogWarning(
                "ID mismatch in invoice update. Route: {RouteId}, Body: {BodyId}",
                id,
                invoice.Id
            );
            return BadRequest(new { Message = ErrorMessages.IdMismatch });
        }

        if (
            invoice.Subtotal < 0
            || invoice.TaxAmount < 0
            || invoice.ShippingCost < 0
            || invoice.PaidAmount < 0
        )
        {
            _logger.LogWarning("Negative invoice amounts provided for update");
            return BadRequest(new { Message = ErrorMessages.InvalidInvoiceAmounts });
        }

        try
        {
            var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(
                i => i.Id == id && !i.IsDeleted,
                cancellationToken
            );

            if (existingInvoice == null)
            {
                _logger.LogWarning("Invoice not found for update: {InvoiceId}", id);
                return NotFound(new { Message = ErrorMessages.InvoiceNotFound });
            }

            existingInvoice.Subtotal = invoice.Subtotal;
            existingInvoice.TaxAmount = invoice.TaxAmount;
            existingInvoice.ShippingCost = invoice.ShippingCost;
            existingInvoice.Total = InvoicePolicy.CalculateInvoiceTotal(
                invoice.Subtotal,
                invoice.TaxAmount,
                invoice.ShippingCost
            );
            existingInvoice.PaidAmount = invoice.PaidAmount;
            existingInvoice.IsPaid = InvoicePolicy.IsInvoicePaid(invoice.PaidAmount, existingInvoice.Total);
            existingInvoice.DueDate = invoice.DueDate;
            existingInvoice.UpdatedAt = DateTime.UtcNow;
            existingInvoice.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Invoice updated: {InvoiceId}, User: {UserId}",
                id,
                GetCurrentUserId() ?? "Unknown"
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating invoice: {InvoiceId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Records a payment against an invoice, updating the paid amount and payment status.
    /// </summary>
    /// <param name="id">The unique GUID of the invoice to pay.</param>
    /// <param name="request">The payment request containing the amount paid.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The updated invoice entity.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Invoice must be within its payment terms via <see cref="InvoicePolicy.IsWithinPaymentTerms"/></description></item>
    /// <item><description>Payment amount must be positive and not cause the paid amount to exceed the total</description></item>
    /// </list>
    /// <para>
    /// The IsPaid flag is recalculated using <see cref="InvoicePolicy.IsInvoicePaid"/>.
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the updated invoice entity.</response>
    /// <response code="400">Invoice overdue, invalid payment amount, or validation failure.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Invoice not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost("{id:guid}/pay")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(InvoiceEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceEntity>> PayInvoice(
        Guid id,
        [FromBody] InvoicePaymentRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            _logger.LogWarning("Null payment request received");
            return BadRequest(new { Message = ErrorMessages.InvoiceDataRequired });
        }

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid invoice GUID provided for payment");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(
                i => i.Id == id && !i.IsDeleted,
                cancellationToken
            );

            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found for payment: {InvoiceId}", id);
                return NotFound(new { Message = ErrorMessages.InvoiceNotFound });
            }

            if (!InvoicePolicy.IsWithinPaymentTerms(invoice.DueDate))
            {
                _logger.LogWarning("Payment attempted on overdue invoice: {InvoiceId}", id);
                return BadRequest(new { Message = ErrorMessages.InvoiceOverdue });
            }

            var newPaidAmount = invoice.PaidAmount + request.PaidAmount;
            if (newPaidAmount > invoice.Total)
            {
                _logger.LogWarning(
                    "Payment exceeds invoice total: {InvoiceId}, Paid: {Paid}, Total: {Total}",
                    id,
                    newPaidAmount,
                    invoice.Total
                );
                return BadRequest(new { Message = ErrorMessages.InvalidInvoiceAmounts });
            }

            invoice.PaidAmount = newPaidAmount;
            invoice.IsPaid = InvoicePolicy.IsInvoicePaid(invoice.PaidAmount, invoice.Total);
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Invoice payment recorded: {InvoiceId}, Paid: {Paid}, User: {UserId}",
                id,
                request.PaidAmount,
                GetCurrentUserId() ?? "Unknown"
            );
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording invoice payment: {InvoiceId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Issues a credit note against a paid invoice within the credit period.
    /// </summary>
    /// <param name="id">The unique GUID of the invoice to issue a credit note against.</param>
    /// <param name="request">The credit note request containing the amount.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// The updated invoice entity.
    /// </returns>
    /// <remarks>
    /// <para><b>Validation Rules:</b></para>
    /// <list type="bullet">
    /// <item><description>Invoice must be fully paid</description></item>
    /// <item><description>Invoice must be within its payment terms (credit period)</description></item>
    /// <item><description>Credit note amount must be positive and not exceed the invoice total</description></item>
    /// </list>
    /// <para>
    /// Eligibility is enforced via <see cref="InvoicePolicy.CanIssueCreditNote"/>.
    /// </para>
    /// </remarks>
    /// <response code="200">Returns the updated invoice entity.</response>
    /// <response code="400">Credit note not permitted or invalid amount.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Invoice not found or has been deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpPost("{id:guid}/credit-note")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(typeof(InvoiceEntity), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<InvoiceEntity>> IssueCreditNote(
        Guid id,
        [FromBody] CreditNoteRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            _logger.LogWarning("Null credit note request received");
            return BadRequest(new { Message = ErrorMessages.InvoiceDataRequired });
        }

        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid invoice GUID provided for credit note");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(
                i => i.Id == id && !i.IsDeleted,
                cancellationToken
            );

            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found for credit note: {InvoiceId}", id);
                return NotFound(new { Message = ErrorMessages.InvoiceNotFound });
            }

            var isWithinPeriod = InvoicePolicy.IsWithinPaymentTerms(invoice.DueDate);

            if (
                !InvoicePolicy.CanIssueCreditNote(
                    invoice.IsPaid,
                    isWithinPeriod,
                    request.Amount,
                    invoice.Total
                )
            )
            {
                _logger.LogWarning("Credit note not permitted for invoice: {InvoiceId}", id);
                return BadRequest(new { Message = ErrorMessages.CreditNoteInvalid });
            }

            invoice.PaidAmount = Math.Max(invoice.PaidAmount - request.Amount, 0);
            invoice.IsPaid = InvoicePolicy.IsInvoicePaid(invoice.PaidAmount, invoice.Total);
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Credit note issued: {InvoiceId}, Amount: {Amount}, User: {UserId}",
                id,
                request.Amount,
                GetCurrentUserId() ?? "Unknown"
            );
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error issuing credit note for invoice: {InvoiceId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }

    /// <summary>
    /// Soft deletes an invoice, marking it as deleted while preserving the record for audit purposes.
    /// </summary>
    /// <param name="id">The unique GUID of the invoice to delete.</param>
    /// <param name="cancellationToken">
    /// Token to cancel the asynchronous operation if needed.
    /// </param>
    /// <returns>
    /// No content on success (204).
    /// </returns>
    /// <remarks>
    /// <para><b>Soft Delete Behavior:</b></para>
    /// <list type="bullet">
    /// <item><description>Sets IsDeleted = true</description></item>
    /// <item><description>Updates UpdatedAt to current UTC time</description></item>
    /// <item><description>Sets UpdatedBy to current user's ID</description></item>
    /// <item><description>Preserves all other invoice data for accounting and audit trails</description></item>
    /// </list>
    /// <para>
    /// A deleted invoice can no longer receive payments or credit notes and no longer appears in lookups.
    /// </para>
    /// </remarks>
    /// <response code="204">Invoice deleted successfully. No content returned.</response>
    /// <response code="400">Invalid invoice ID format provided.</response>
    /// <response code="401">Authentication required - user not logged in.</response>
    /// <response code="403">Insufficient permissions - Admin, Manager, or Developer role required.</response>
    /// <response code="404">Invoice not found or already deleted.</response>
    /// <response code="500">Internal server error occurred while processing the request.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager,Developer")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteInvoice(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        if (id == Guid.Empty)
        {
            _logger.LogWarning("Invalid invoice GUID provided for deletion");
            return BadRequest(new { Message = ErrorMessages.InvalidId });
        }

        try
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(
                i => i.Id == id && !i.IsDeleted,
                cancellationToken
            );

            if (invoice == null)
            {
                _logger.LogWarning("Invoice not found for deletion: {InvoiceId}", id);
                return NotFound(new { Message = ErrorMessages.InvoiceNotFound });
            }

            invoice.IsDeleted = true;
            invoice.UpdatedAt = DateTime.UtcNow;
            invoice.UpdatedBy = Guid.TryParse(GetCurrentUserId(), out var userId)
                ? userId
                : Guid.Empty;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Invoice deleted: {InvoiceId}, Number: {InvoiceNumber}, User: {UserId}",
                id,
                invoice.InvoiceNumber,
                GetCurrentUserId() ?? "Unknown"
            );
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invoice: {InvoiceId}", id);
            return StatusCode(500, new { Message = ErrorMessages.ProcessingRequestError });
        }
    }
}
