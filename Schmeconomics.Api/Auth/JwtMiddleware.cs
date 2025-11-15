using Schmeconomics.Api.Tokens;
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Auth;

public class JwtMiddleware(
    ILogger<JwtMiddleware> _logger,
    RequestDelegate _next
) {
    public async Task InvokeAsync(
        HttpContext context,
        IUserService userService,
        ICurrentUserSetter _userSetter,
        IAuthTokenProvider tokenProvider
    ) {
        var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(' ').Last();
        if (token == null) return;

        try
        {
            var claims = await tokenProvider.ValidateAuthTokenAsync(token);
            var user = await userService.GetUserFromIdAsync(claims.First(c => c.Value == "UserId").Value);
            context.Items["User"] = user;
            _userSetter.User = user;
        }
        catch (TokenProviderException ex) when (
            ex is TokenProviderException.DbException ||
            ex is TokenProviderException.JwtException
        )
        {
            _logger.LogError(ex, "Error while validating JWT");
        }
        catch (TokenProviderException)
        {
            // Any other exceptions can silently fail. It will simply return an unauthorized exception
        }

        await _next(context);        
    }
}