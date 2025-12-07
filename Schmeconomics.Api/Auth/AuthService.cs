using System.Data.Common;
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
    public async Task<Result<AuthModel>> SignInAsync(string name, string password, string ipAddress, CancellationToken stopToken = default)
    {
        try {
            // Get the user from the database that has the matching name
            var user = await _db.Users.Where(u => u.Name == name).FirstOrDefaultAsync(stopToken);
            if(user == null) return new AuthServiceError.UserNotFound(name);

            // Verify the input password is correct
            var verification = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);
            if (verification == PasswordVerificationResult.Failed)
                return new AuthServiceError.PasswordVerificationFailed();

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
        catch(DbException ex)
        {
            throw new AuthServiceException.DbException(ex);
        }
        catch (AuthTokenProviderException ex)
        {
            throw new AuthServiceException.AuthTokenProviderException(ex);
        }
        catch (RefreshTokenProviderException ex)
        {
            throw new AuthServiceException.RefreshTokenProviderException(ex);
        }
    }

    public async Task<Result> SignOutAsync(string refreshToken, string ipAddress, CancellationToken stopToken = default)
    {
        try {
            await _refreshTokenProvider.RevokeTokenAsync(refreshToken, ipAddress, stopToken);
            return Result.Ok();
        }
        catch (RefreshTokenProviderException ex)
        {
            throw new AuthServiceException.RefreshTokenProviderException(ex);
        }
    }
    
    public async Task<Result<AuthModel>> RefreshTokenAsync(string ipAddress, string refreshToken, CancellationToken stopToken = default)
    {
        // Use the refresh token provider to create a new auth token from the refresh token
        try {
            var refreshedTokenResult = await _refreshTokenProvider.CreateFromTokenAsync(refreshToken, ipAddress, stopToken);
            
            // Create new access token using the auth token provider
            var claims = new Dictionary<string, object>
            {
                ["sub"] = refreshedTokenResult.User.Id,
                ["name"] = refreshedTokenResult.User.Name,
                ["role"] = refreshedTokenResult.User.Role.ToString()
            };

            var accessToken = await _authTokenProvider.CreateAuthTokenAsync(claims, stopToken);
            
            return new AuthModel(accessToken, refreshedTokenResult.Token, refreshedTokenResult.ExpiresOnUtc);
        } 
        catch (AuthTokenProviderException ex)
        {
            throw new AuthServiceException.AuthTokenProviderException(ex);
        }
        catch (RefreshTokenProviderException ex)
        {
            throw new AuthServiceException.RefreshTokenProviderException(ex);
        }
    }
}
