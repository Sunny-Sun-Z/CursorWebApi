using System.Diagnostics;

namespace CursorWebApi.Api.Middleware;

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _logger;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
       var stopwatch = Stopwatch.StartNew();

        context.Response.OnStarting(() =>
        {
            var elapsedMs = stopwatch.ElapsedMilliseconds;
            context.Response.Headers["X-Elapsed-Milliseconds"] = elapsedMs.ToString();
            return Task.CompletedTask;
        });

        await _next(context);

        stopwatch.Stop();

        var path = context.Request.Path;
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;

        _logger.LogInformation("Request {Method} {Path} responded {StatusCode} in {Elapsed} ms",
            method, path, statusCode, stopwatch.ElapsedMilliseconds);
    }
}
