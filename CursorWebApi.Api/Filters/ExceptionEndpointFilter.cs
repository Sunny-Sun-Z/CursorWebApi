using CursorWebApi.Domain.Exceptions;

// this is for api
namespace CursorWebApi.Api.Filters;
public class ExceptionEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        try
        {
            return await next(context);
        }
        catch (ValidationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (ProductNotFoundException ex)
        {
            return Results.NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log or handle other exceptions
            return Results.Problem("An unexpected error occurred.");
        }
    }
}
