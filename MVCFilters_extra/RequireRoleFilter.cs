using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class RequireRoleFilter : IAuthorizationFilter
{
    private readonly string _role;

    public RequireRoleFilter(string role)
    {
        _role = role;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (!user.Identity.IsAuthenticated || !user.IsInRole(_role))
        {
            // Short-circuit: user is not authorized
            context.Result = new ForbidResult(); // or UnauthorizedResult()
        }
    }
}
