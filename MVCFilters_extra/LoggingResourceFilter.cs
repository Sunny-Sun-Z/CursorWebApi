using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

public class LoggingResourceFilter : IResourceFilter
{
    private readonly ILogger<LoggingResourceFilter> _logger;

    public LoggingResourceFilter(ILogger<LoggingResourceFilter> logger)
    {
        _logger = logger;
    }

    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        // Runs before model binding and before the action
        _logger.LogInformation("Resource filter: OnResourceExecuting for {Path}", context.HttpContext.Request.Path);
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        // Runs after the action and after result execution
        _logger.LogInformation("Resource filter: OnResourceExecuted for {Path}", context.HttpContext.Request.Path);
    }
}
