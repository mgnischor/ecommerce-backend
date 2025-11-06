using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ECommerce.API.Filters;

/// <summary>
/// Action filter that validates model state before controller action execution
/// </summary>
/// <remarks>
/// <para>
/// Automatically validates incoming request models using DataAnnotations attributes.
/// If validation fails, returns a standardized 400 Bad Request response with detailed error messages.
/// This eliminates the need to manually check ModelState.IsValid in every controller action.
/// </para>
/// <para>
/// <strong>Features:</strong>
/// </para>
/// <list type="bullet">
/// <item><description><strong>Automatic Validation:</strong> Runs before every action method</description></item>
/// <item><description><strong>Standardized Responses:</strong> Returns consistent error format</description></item>
/// <item><description><strong>Detailed Errors:</strong> Includes field-specific validation messages</description></item>
/// <item><description><strong>Early Exit:</strong> Prevents invalid data from reaching business logic</description></item>
/// </list>
/// <para>
/// <strong>Usage:</strong> Apply globally via ConfigureServices or per-controller/action with [ServiceFilter].
/// </para>
/// <code>
/// // Global registration in Program.cs:
/// builder.Services.AddControllers(options =>
/// {
///     options.Filters.Add&lt;ValidateModelStateFilter&gt;();
/// });
/// </code>
/// </remarks>
public sealed class ValidateModelStateFilter : IActionFilter
{
    /// <summary>
    /// Logger instance for tracking validation failures
    /// </summary>
    private readonly ILogger<ValidateModelStateFilter> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidateModelStateFilter"/> class
    /// </summary>
    /// <param name="logger">Logger for recording validation failures</param>
    /// <exception cref="ArgumentNullException">Thrown when logger is null</exception>
    public ValidateModelStateFilter(ILogger<ValidateModelStateFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates model state before action execution
    /// </summary>
    /// <param name="context">The action executing context containing the model state</param>
    /// <remarks>
    /// Checks ModelState.IsValid before the controller action executes.
    /// If validation fails, sets the result to a BadRequestObjectResult and prevents action execution.
    /// Logs validation failures with relevant context for debugging and monitoring.
    /// </remarks>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context
                .ModelState.Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp =>
                        kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                        ?? Array.Empty<string>()
                );

            _logger.LogWarning(
                "Model validation failed for {ActionName}. Errors: {Errors}",
                context.ActionDescriptor.DisplayName,
                string.Join("; ", errors.Select(e => $"{e.Key}: {string.Join(", ", e.Value)}"))
            );

            context.Result = new BadRequestObjectResult(
                new { Message = "Validation failed", Errors = errors }
            );
        }
    }

    /// <summary>
    /// Called after action execution (no-op for this filter)
    /// </summary>
    /// <param name="context">The action executed context</param>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
