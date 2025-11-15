using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Schmeconomics.Api.Tokens;
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
            ?? throw new ArgumentException("Invalid credentials");

        // Verify the input password is correct
        var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
            throw new ArgumentException("Invalid credentials");

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
}
