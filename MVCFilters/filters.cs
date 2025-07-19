// Resource Filter
public class MyResourceFilter : IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        // Runs before model binding
    }
    public void OnResourceExecuted(ResourceExecutedContext context)
    {
        // Runs after model binding
    }
}

// Action Filter
public class MyActionFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Runs before the action method
    }
    public void OnActionExecuted(ActionExecutedContext context)
    {
        // Runs after the action method
    }
}

// Exception Filter
public class MyExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        // Handles exceptions thrown by the action or other filters
        context.Result = new ObjectResult(new { error = context.Exception.Message })
        {
            StatusCode = 500
        };
        context.ExceptionHandled = true;
    }
}

// Result Filter
public class MyResultFilter : IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        // Runs before the action result is executed
    }
    public void OnResultExecuted(ResultExecutedContext context)
    {
        // Runs after the action result is executed
    }
}
