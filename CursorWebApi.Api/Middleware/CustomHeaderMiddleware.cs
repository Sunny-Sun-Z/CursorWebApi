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
        context.Request.Headers["X-Request-Received"] = DateTime.UtcNow.ToString("o");
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-Powered-By"] = "CursorWebApi";
            context.Response.Headers["X-Processed-At"] = DateTime.UtcNow.ToString("o");
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
