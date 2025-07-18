using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using CursorWebApi.Domain.Exceptions;

namespace CursorWebApi.Api.Filters;

// this is filter used for MVC controller
public class CustomExceptionFilter : IExceptionFilter
{
    private readonly ILogger<CustomExceptionFilter> _logger;

    public CustomExceptionFilter(ILogger<CustomExceptionFilter> logger)
    {

        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException vex)
        {
            _logger.LogWarning(context.Exception, "Validation error handled by filter.");
            context.Result = new BadRequestObjectResult(new
            {
                statusCode = 400,
                message = vex.Message,
                timestamp = DateTime.UtcNow,
                path = context.HttpContext.Request.Path,
                method = context.HttpContext.Request.Method
            });
            context.ExceptionHandled = true;
        }
        // You can add more custom exception handling here if needed
    }
}
