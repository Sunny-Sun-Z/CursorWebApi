using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.AspNetCore.Mvc.Filters;

namespace CursorWebApi.Api.Filters;

public class ValidationAndLoggingEndpointFilter : IEndpointFilter
{
    private readonly ILogger<ValidationAndLoggingEndpointFilter> _logger;

    public ValidationAndLoggingEndpointFilter(ILogger<ValidationAndLoggingEndpointFilter> logger)
    {
        _logger = logger;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Example: Log the request path and method
        var httpContext = context.HttpContext;
        _logger.LogInformation("EndpointFilter: {Method} {Path} called", httpContext.Request.Method, httpContext.Request.Path);

        // Example: Validate the first argument if it's a Product
        if (context.Arguments.Count > 0 && context.Arguments[0] is CursorWebApi.Domain.Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                _logger.LogWarning("Validation failed: Product name is required.");
                return Results.BadRequest(new { message = "Product name is required (from endpoint filter)." });
            }
        }

        // Call the next filter or endpoint
        var result = await next(context);

        // Example: Log the response type
        _logger.LogInformation("EndpointFilter: Response type is {Type}", result?.GetType().Name ?? "null");

        return result;
    }
}
