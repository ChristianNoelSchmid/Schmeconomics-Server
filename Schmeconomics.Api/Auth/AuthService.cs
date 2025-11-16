using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Schmeconomics.Api.Tokens.AuthTokens;
using Schmeconomics.Api.Tokens.RefreshTokens;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.Auth;

public class AuthService(
    SchmeconomicsDbContext _db,
    IAuthTokenProvider _authTokenProvider,
    IRefreshTokenProvider _refreshTokenProvider,
    IPasswordHasher<User> _passwordHasher
) : IAuthService {
    public async Task<AuthModel> SignInAsync(string name, string password, string ipAddress, CancellationToken stopToken = default)
    {
        // Get the user from the database that has the matching name
        var user = await _db.Users.Where(u => u.Name == name).FirstOrDefaultAsync(stopToken) 
            ?? throw new AuthServiceException.UserNotFoundException(name);

        // Verify the input password is correct
        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
            throw new AuthServiceException.InvalidCredentialsException();

        // Create auth token
        var claims = new Dictionary<string, object>
        {
            ["sub"] = user.Id,
            ["name"] = user.Name,
            ["role"] = user.Role.ToString()
        };

        var accessToken = await _authTokenProvider.CreateAuthTokenAsync(claims, stopToken);

        // Create refresh token
        var refreshToken = await _refreshTokenProvider.CreateNewTokenAsync(user.Id, ipAddress, stopToken);
        
        return new AuthModel(accessToken, refreshToken.Token, refreshToken.ExpiresOnUtc);
    }

    public async Task SignOutAsync(string refreshToken, string ipAddress, CancellationToken stopToken = default)
    {
        await _refreshTokenProvider.RevokeTokenAsync(refreshToken, ipAddress, stopToken);
    }
    
    public async Task<AuthModel> RefreshTokenAsync(string ipAddress, string refreshToken, CancellationToken stopToken = default)
    {
        // Use the refresh token provider to create a new auth token from the refresh token
        var refreshedTokenResult = await _refreshTokenProvider.CreateFromTokenAsync(refreshToken, ipAddress, stopToken);
        
        // Create new access token using the auth token provider
        var claims = new Dictionary<string, object>
        {
            ["sub"] = refreshedTokenResult.User.Id,
            ["name"] = refreshedTokenResult.User.Name,
            ["role"] = refreshedTokenResult.User.Role.ToString()
        };

        var accessToken = await _authTokenProvider.CreateAuthTokenAsync(claims, stopToken);
        
        return new AuthModel(accessToken, refreshToken, refreshedTokenResult.ExpiresOnUtc);
    }
}
