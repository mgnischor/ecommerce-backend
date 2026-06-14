using System.Security.Claims;
using ECommerce.API.Constants;
using ECommerce.API.DTOs;
using ECommerce.Application.Interfaces;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Domain.Interfaces;
using ECommerce.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Controllers;

/// <summary>
/// Payment processing endpoints backed by a fictitious payment gateway.
/// </summary>
/// <remarks>
/// Each operation contacts the fictitious gateway, which introduces a random latency between
/// 1 and 10 seconds and returns a randomized success or failure outcome.
/// </remarks>
[Tags("Payments")]
[ApiController]
[Route("api/v1/payments")]
[Produces("application/json")]
[Authorize]
public sealed class PaymentController : ControllerBase
{
    private readonly PostgresqlContext _context;
    private readonly IPaymentGatewayService _gateway;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILoggingService _logger;

    public PaymentController(
        PostgresqlContext context,
        IPaymentGatewayService gateway,
        IPaymentRepository paymentRepository,
        ILoggingService logger
    )
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _paymentRepository =
            paymentRepository ?? throw new ArgumentNullException(nameof(paymentRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // -------------------------------------------------------------------------
    // POST /api/v1/payments/process
    // -------------------------------------------------------------------------

    /// <summary>
    /// Processes a payment for an order through the fictitious gateway.
    /// </summary>
    /// <remarks>
    /// The gateway introduces a random latency between 1 and 10 seconds and returns a
    /// randomized outcome (≈80% success rate). A <see cref="PaymentEntity"/> record is
    /// persisted regardless of outcome.
    /// </remarks>
    /// <param name="request">Payment request with order ID, method, and currency.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Payment result with transaction ID and status.</returns>
    /// <response code="201">Payment processed (check <c>status</c> field for outcome).</response>
    /// <response code="400">Invalid request or order is not payable.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Order not found.</response>
    /// <response code="409">A completed payment already exists for this order.</response>
    [HttpPost("process")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PaymentResponseDto>> ProcessPayment(
        [FromBody] ProcessPaymentRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized(new { Message = ErrorMessages.UserIdNotFoundInToken });

        // Load the order
        var order = await _context.Orders.FirstOrDefaultAsync(
            o => o.Id == request.OrderId,
            cancellationToken
        );

        if (order is null)
            return NotFound(
                new { Message = ErrorMessages.OrderNotFoundById(request.OrderId.ToString()) }
            );

        // Check that the order is in a payable state
        var payableStatuses = new[]
        {
            OrderStatus.Pending,
            OrderStatus.Confirmed,
            OrderStatus.Processing,
        };
        if (!payableStatuses.Contains(order.Status))
            return BadRequest(
                new { Message = $"Orders with status '{order.Status}' cannot be paid." }
            );

        // Prevent duplicate payments
        var existingPayment = await _paymentRepository.GetByOrderIdAsync(
            request.OrderId,
            cancellationToken
        );
        if (existingPayment is not null && existingPayment.Status == PaymentStatus.Completed)
            return Conflict(new { Message = "A completed payment already exists for this order." });

        // Send to the fictitious gateway (may take 1–10 seconds)
        var gatewayResult = await _gateway.ProcessPaymentAsync(
            order.Id,
            order.TotalAmount,
            request.Currency,
            request.PaymentMethod,
            cancellationToken
        );

        // Persist the payment record
        var payment = new PaymentEntity
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            TransactionId = gatewayResult.TransactionId,
            PaymentMethod = request.PaymentMethod,
            Status = gatewayResult.Status,
            Amount = order.TotalAmount,
            Currency = request.Currency,
            PaymentProvider = "FictitiousGateway",
            ProviderResponse = gatewayResult.ProviderResponse,
            ErrorMessage = gatewayResult.ErrorMessage,
            CapturedAt = gatewayResult.IsSuccess ? gatewayResult.ProcessedAt : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _paymentRepository.AddAsync(payment, cancellationToken);

        // Update order status on successful payment
        if (gatewayResult.IsSuccess)
        {
            order.Status = OrderStatus.Confirmed;
            order.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Payment processed: PaymentId={PaymentId}, OrderId={OrderId}, Status={Status}, TransactionId={TxnId}",
            payment.Id,
            order.Id,
            payment.Status,
            payment.TransactionId
        );

        var responseDto = MapToDto(payment);
        return CreatedAtAction(nameof(GetPaymentById), new { id = payment.Id }, responseDto);
    }

    // -------------------------------------------------------------------------
    // POST /api/v1/payments/{id}/refund
    // -------------------------------------------------------------------------

    /// <summary>
    /// Requests a refund for a captured payment through the fictitious gateway.
    /// </summary>
    /// <param name="id">Payment ID to refund.</param>
    /// <param name="request">Optional partial refund amount. Omit to refund the full amount.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated payment record.</returns>
    /// <response code="200">Refund processed (check <c>status</c> field for outcome).</response>
    /// <response code="400">Payment is not in a refundable state.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Payment not found.</response>
    [HttpPost("{id:guid}/refund")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDto>> RefundPayment(
        Guid id,
        [FromBody] RefundPaymentRequestDto request,
        CancellationToken cancellationToken = default
    )
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var payment = await _context.Payments.FirstOrDefaultAsync(
            p => p.Id == id,
            cancellationToken
        );

        if (payment is null)
            return NotFound(new { Message = $"Payment with ID '{id}' not found." });

        if (payment.Status != PaymentStatus.Completed)
            return BadRequest(
                new
                {
                    Message = $"Only completed payments can be refunded. Current status: '{payment.Status}'.",
                }
            );

        if (string.IsNullOrEmpty(payment.TransactionId))
            return BadRequest(
                new
                {
                    Message = "Payment has no associated transaction ID – cannot process refund.",
                }
            );

        var refundAmount = request.Amount ?? payment.Amount;

        if (refundAmount > payment.Amount)
            return BadRequest(
                new
                {
                    Message = $"Refund amount ({refundAmount}) exceeds the original payment amount ({payment.Amount}).",
                }
            );

        // Send refund to the fictitious gateway (may take 1–10 seconds)
        var gatewayResult = await _gateway.RefundPaymentAsync(
            payment.TransactionId,
            refundAmount,
            payment.Currency,
            cancellationToken
        );

        // Update payment record
        payment.Status = gatewayResult.Status;
        payment.RefundAmount = gatewayResult.IsSuccess ? refundAmount : null;
        payment.RefundedAt = gatewayResult.IsSuccess ? gatewayResult.ProcessedAt : null;
        payment.ProviderResponse = gatewayResult.ProviderResponse;
        payment.ErrorMessage = gatewayResult.ErrorMessage;
        payment.UpdatedAt = DateTime.UtcNow;

        // Update order status on successful refund
        if (gatewayResult.IsSuccess)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(
                o => o.Id == payment.OrderId,
                cancellationToken
            );
            if (order is not null)
            {
                order.Status = OrderStatus.Refunded;
                order.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Refund processed: PaymentId={PaymentId}, Status={Status}, RefundAmount={Amount}",
            payment.Id,
            payment.Status,
            refundAmount
        );

        return Ok(MapToDto(payment));
    }

    // -------------------------------------------------------------------------
    // GET /api/v1/payments/{id}
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retrieves a payment record by its ID.
    /// </summary>
    /// <param name="id">Payment unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Payment details.</returns>
    /// <response code="200">Payment found.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">Payment not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDto>> GetPaymentById(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken);

        if (payment is null)
            return NotFound(new { Message = $"Payment with ID '{id}' not found." });

        return Ok(MapToDto(payment));
    }

    // -------------------------------------------------------------------------
    // GET /api/v1/payments/order/{orderId}
    // -------------------------------------------------------------------------

    /// <summary>
    /// Retrieves the payment record associated with a specific order.
    /// </summary>
    /// <param name="orderId">Order unique identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Payment details for the order.</returns>
    /// <response code="200">Payment found.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="404">No payment found for the specified order.</response>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentResponseDto>> GetPaymentByOrderId(
        Guid orderId,
        CancellationToken cancellationToken = default
    )
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);

        if (payment is null)
            return NotFound(new { Message = $"No payment found for order '{orderId}'." });

        return Ok(MapToDto(payment));
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return claim is not null && Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }

    private static PaymentResponseDto MapToDto(PaymentEntity p) =>
        new()
        {
            PaymentId = p.Id,
            OrderId = p.OrderId,
            TransactionId = p.TransactionId ?? string.Empty,
            Status = p.Status,
            Amount = p.Amount,
            Currency = p.Currency,
            PaymentMethod = p.PaymentMethod,
            ErrorMessage = p.ErrorMessage,
            ProviderResponse = p.ProviderResponse ?? string.Empty,
            ProcessedAt = p.CapturedAt ?? p.UpdatedAt,
            CreatedAt = p.CreatedAt,
        };
}
