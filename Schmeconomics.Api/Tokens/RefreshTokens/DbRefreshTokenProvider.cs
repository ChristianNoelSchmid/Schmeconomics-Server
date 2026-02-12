using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Schmeconomics.Api.Time;
using Schmeconomics.Api.Users;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.Tokens.RefreshTokens;
public class DbRefreshTokenProvider(
    IOptions<DbRefreshTokenProviderConfig> _config,
    SchmeconomicsDbContext _dbContext,
    IDateTimeProvider _dateTimeProvider
) : IRefreshTokenProvider {
    public async Task<RefreshTokenFamilyModel> CreateNewTokenAsync(string userId, string ipAddress, CancellationToken stopToken = default)
    {
        var user = await _dbContext.Users.FindAsync([userId], stopToken) 
            ?? throw new RefreshTokenProviderException.UserIdNotFound(userId, ipAddress);

        var config = _config.Value;
        var familyToken = Convert.ToBase64String(TokenUtils.CreateRandomBytes(config.FamilyTokenLength));
        var activeToken = Convert.ToBase64String(TokenUtils.CreateRandomBytes(config.RefreshTokenLength));

        var newToken = new RefreshTokenFamily
        {
            UserId = userId,
            FamilyToken = familyToken,
            ActiveToken = activeToken,
            ExpiresOnUtc = _dateTimeProvider.UtcNow + config.RefreshTokenLifetime,
            RecentIpAddresses = [ipAddress],
        };

        _dbContext.RefreshTokenFamilies.Add(newToken);

        try
        {
            await _dbContext.SaveChangesAsync(stopToken);
        }
        catch (DbException ex)
        {
            throw new RefreshTokenProviderException.DbException(ex);
        }

        return new RefreshTokenFamilyModel($"{familyToken}.{activeToken}", newToken.ExpiresOnUtc, (UserModel)user);
    }

    public async Task<RefreshTokenFamilyModel> CreateFromTokenAsync(string token, string ipAddress, CancellationToken stopToken = default)
    {
        var config = _config.Value;
        var splits = token.Split('.');

        try
        {
            if (splits.Length != 2)
                throw new RefreshTokenProviderException.MalformedRefreshToken(token, ipAddress);

            var familyToken = await GetRefreshTokenFamilyAsync(splits[0], ipAddress, stopToken);

            if (familyToken.RecentIpAddresses.Count == config.IpAddressMaxCount)
                familyToken.RecentIpAddresses.RemoveAt(0);

            familyToken.RecentIpAddresses.Add(ipAddress);

            // Check if the active token matches the provided token.
            // If it doesn't, revoke it and throw exception
            if (familyToken.ActiveToken != splits[1])
            {
                familyToken.ActiveToken = null;
                familyToken.RevokedOnUtc = _dateTimeProvider.UtcNow;

                throw new RefreshTokenProviderException.RefreshTokenDoesNotMatch(token, ipAddress);
            }
            else if(familyToken.RevokedOnUtc != null)
            {
                throw new RefreshTokenProviderException.RefreshTokenStale(token, ipAddress);
            }
            else
            {
                var newActiveToken = Convert.ToBase64String(TokenUtils.CreateRandomBytes(config.RefreshTokenLength));
                familyToken.ActiveToken = newActiveToken;
                familyToken.ExpiresOnUtc = _dateTimeProvider.UtcNow + config.RefreshTokenLifetime;

                _dbContext.RefreshTokenFamilies.Update(familyToken);
                await _dbContext.SaveChangesAsync(stopToken);

                // Get the user to include in the model
                var user = await _dbContext.Users.FindAsync([familyToken.UserId], stopToken) 
                    ?? throw new RefreshTokenProviderException.UserIdNotFound(familyToken.UserId, ipAddress);

                return new RefreshTokenFamilyModel($"{familyToken.FamilyToken}.{newActiveToken}", familyToken.ExpiresOnUtc, (UserModel)user);
            }
        }
        catch (DbException ex)
        {
            throw new RefreshTokenProviderException.DbException(ex);
        }
    }

    public async Task RevokeTokenAsync(string token, string ipAddress, CancellationToken stopToken = default)
    {
        var config = _config.Value;
        var splits = token.Split('.');

        try
        {
            if (splits.Length != 2)
                throw new RefreshTokenProviderException.MalformedRefreshToken(token, ipAddress);

            var familyToken = await GetRefreshTokenFamilyAsync(splits[0], ipAddress, stopToken);

            familyToken.ActiveToken = null;
            familyToken.RevokedOnUtc = _dateTimeProvider.UtcNow;

            if (familyToken.RecentIpAddresses.Count == config.IpAddressMaxCount)
                familyToken.RecentIpAddresses.RemoveAt(0);
            familyToken.RecentIpAddresses.Add(ipAddress);

            _dbContext.RefreshTokenFamilies.Update(familyToken);
            await _dbContext.SaveChangesAsync(stopToken);
        }
        catch (DbException ex)
        {
            throw new RefreshTokenProviderException.DbException(ex);
        }
    }

    private async Task<RefreshTokenFamily> GetRefreshTokenFamilyAsync(string familyToken, string ipAddress, CancellationToken stopToken)
    {
        // Get the token whose family token matches the first segment of the provided token
        var tokenEntity = await _dbContext.RefreshTokenFamilies
            .FirstOrDefaultAsync(f => f.FamilyToken == familyToken, stopToken)
                ?? throw new RefreshTokenProviderException.RefreshTokenNotFound(familyToken, ipAddress);

        // If the token is not active, or is past expiration, throw stale token exception
        if (
            tokenEntity.ActiveToken == null || 
            tokenEntity.ExpiresOnUtc < _dateTimeProvider.UtcNow || 
            tokenEntity.RevokedOnUtc != null
        )
            throw new RefreshTokenProviderException.RefreshTokenStale(familyToken, ipAddress);

        return tokenEntity;
    }
}
