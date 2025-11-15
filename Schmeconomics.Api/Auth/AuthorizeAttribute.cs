namespace Schmeconomics.Api.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Schmeconomics.Api.Users;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute(Role Role) : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
        if (allowAnonymous) return;

        var user = context.HttpContext.Items["User"] as UserModel;
        if (user == null || user.Role < Role)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}