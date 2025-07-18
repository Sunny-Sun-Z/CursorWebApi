namespace CursorWebApi.Api.Middleware;

public class CustomHeaderMiddleware
{
    private readonly RequestDelegate _next;

    public CustomHeaderMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add a custom request header (for demonstration)
        // context.Request.Headers["X-Request-Received"] = DateTime.UtcNow.ToString("o");

        // Call the next middleware/component in the pipeline
        await _next(context);

        // Add custom response headers
        context.Response.Headers["X-Powered-By"] = "CursorWebApi";
        context.Response.Headers["X-Processed-At"] = DateTime.UtcNow.ToString("o");

        // Optionally, you can modify the response body here (advanced)
    }
}
