using Schmeconomics.Api.Tokens.AuthTokens;
using Schmeconomics.Api.Users;

namespace Schmeconomics.Api.Auth;
public class JwtMiddleware(
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

        var claims = await tokenProvider.ValidateAuthTokenAsync(token);
        var user = await userService.GetUserFromIdAsync(claims.First(c => c.Value == "UserId").Value);
        context.Items["User"] = user;
        _userSetter.User = user;

        await _next(context);        
    }
}
