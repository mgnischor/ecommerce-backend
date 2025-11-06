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
    /// <param name="logger">Logger instance for recording validation failures and model state errors</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is null</exception>
    /// <remarks>
    /// <para>
    /// The filter requires a logger to track validation failures for monitoring and debugging purposes.
    /// Validation errors are logged at Warning level with detailed error information.
    /// </para>
    /// <para>
    /// <strong>Dependency Injection:</strong> This filter is typically registered in the DI container
    /// and can be applied globally or per-controller using the [ServiceFilter] attribute.
    /// </para>
    /// </remarks>
    public ValidateModelStateFilter(ILogger<ValidateModelStateFilter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Validates model state before action execution
    /// </summary>
    /// <param name="context">The action executing context containing the model state, action arguments, and HTTP context</param>
    /// <remarks>
    /// <para>
    /// This method is called before the controller action method executes. It validates the model state
    /// populated by ASP.NET Core's model binding and validation pipeline using DataAnnotations attributes
    /// (e.g., [Required], [StringLength], [Range], [EmailAddress]).
    /// </para>
    /// <para>
    /// <strong>Validation Flow:</strong>
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Check ModelState:</strong> Evaluates ModelState.IsValid property</description></item>
    /// <item><description><strong>Extract Errors:</strong> Collects all field-level validation errors from ModelState</description></item>
    /// <item><description><strong>Format Errors:</strong> Creates a dictionary mapping field names to error message arrays</description></item>
    /// <item><description><strong>Log Failure:</strong> Records validation failure at Warning level with action name and detailed errors</description></item>
    /// <item><description><strong>Short-Circuit:</strong> Sets context.Result to BadRequestObjectResult, preventing action execution</description></item>
    /// </list>
    /// <para>
    /// <strong>Response Format (400 Bad Request):</strong>
    /// </para>
    /// <code>
    /// {
    ///   "message": "Validation failed",
    ///   "errors": {
    ///     "Email": ["The Email field is required."],
    ///     "Age": ["The field Age must be between 18 and 120."],
    ///     "Price": ["The Price field is required.", "The field Price must be greater than 0."]
    ///   }
    /// }
    /// </code>
    /// <para>
    /// <strong>Benefits:</strong>
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Consistency:</strong> Ensures all API endpoints return the same validation error format</description></item>
    /// <item><description><strong>DRY Principle:</strong> Eliminates repetitive ModelState.IsValid checks in controller actions</description></item>
    /// <item><description><strong>Early Exit:</strong> Prevents invalid data from reaching business logic layer</description></item>
    /// <item><description><strong>Client-Friendly:</strong> Provides detailed, field-specific error messages for UI display</description></item>
    /// <item><description><strong>Monitoring:</strong> Logs validation patterns for identifying common data quality issues</description></item>
    /// </list>
    /// <para>
    /// <strong>Note:</strong> If ModelState is valid, the method completes without setting context.Result,
    /// allowing normal action execution to proceed.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if context is null (handled by framework)</exception>
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
    /// <param name="context">The action executed context containing the action result and exception information</param>
    /// <remarks>
    /// <para>
    /// This method is intentionally left empty as all validation logic occurs in <see cref="OnActionExecuting"/>.
    /// Model validation must happen before action execution, not after, so no post-execution processing is needed.
    /// </para>
    /// <para>
    /// Part of the <see cref="IActionFilter"/> interface implementation. This method is called after the
    /// action method executes but before the result is executed (if the action wasn't short-circuited by validation failure).
    /// </para>
    /// </remarks>
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}
