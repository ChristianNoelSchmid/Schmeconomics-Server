using System.Data.Common;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Schmeconomics.Api.Time;
using Schmeconomics.Api.Tokens;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.JwtSecrets;

public class DbSecretsProvider(
    IOptions<DbSecretsProviderConfig> _config,
    SchmeconomicsDbContext _dbContext,
    IDateTimeProvider _dateTimeProvider
) : ISecretsProvider {

    /// <summary>
    /// The point at which a new secret should be generated.
    /// Secrets are generated lazily (ie. whenever one is requested, if needed)
    /// so actual secret generation may not happen at the moment of this DateTime
    /// </summary>
    private DateTime _nextRefreshUtc = default;

    public async IAsyncEnumerable<byte[]> GetSecretsAsync(
        [EnumeratorCancellation] CancellationToken token = default
    ) {
        // If we are at or past the time where a new secret should be
        // generated, create a new one
        if (_dateTimeProvider.UtcNow > _nextRefreshUtc)
        {
            await RefreshAsync(token);
            _nextRefreshUtc = _dateTimeProvider.UtcNow + await GetTimeToNextRefreshAsync(token);
        }

        var config = _config.Value;
        var jwtConfigs = _dbContext.SecretConfigs.OrderByDescending(c => c.Id)
            .AsAsyncEnumerable().GetAsyncEnumerator(token);

        // The total number of configs traversed
        int count = 0;
        do
        {
            try
            {
                // When an entry exists next, increment count
                if (await jwtConfigs.MoveNextAsync()) count += 1;
                else
                {
                    // If end of Enumerator is reached, but count is 0, no
                    // records were found. This should never happen, since the background service
                    // should always ensure a current secret is generated.
                    if (count == 0) throw new SecretProviderNoSecretEntryFound();
                    // Otherwise, set count to -1 and continue
                    count = -1;
                }
            }
            catch (DbException dbException)
            {
                throw new TokenProviderException.DbException(dbException);
            }
            // If count is not -1, yield the current JwtConfig
            if (count != -1) yield return jwtConfigs.Current.SecretBytes;
        }
        while (count != -1);
    }

    protected async Task RefreshAsync(CancellationToken stoppingToken)
    {
        /*
         * 1) Delete any JwtConfigs that have expired
         * 2) Create JwtConfigs up to the max count
         */
        await DeleteExpiredTablesAsync(stoppingToken);
        await CreateNewJwtConfigsAsync(stoppingToken);
    }

    private async Task DeleteExpiredTablesAsync(CancellationToken stoppingToken)
    {
        try
        {
            var config = _config.Value;
            var expiredDateTime = _dateTimeProvider.UtcNow - config.SecretLifetimeLength;
            var expiredJwtConfigs = _dbContext.SecretConfigs.Where(c => c.CreatedOnUtc <= expiredDateTime);

            _dbContext.SecretConfigs.RemoveRange(expiredJwtConfigs);
            await _dbContext.SaveChangesAsync(stoppingToken);
        }
        catch (DbException ex)
        {
            throw new SecretProviderDbException(ex);
        }
    }

    private async Task CreateNewJwtConfigsAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Check if there's any entry that exists that is newer than
            // The expiration time-length / 2.
            // If there isn't one, create a single new entry.
            var config = _config.Value;
            var targetDateTime = _dateTimeProvider.UtcNow - config.SecretLifetimeLength / 2.0;

            var jwtConfig = await _dbContext.SecretConfigs
                .Where(c => c.CreatedOnUtc >= targetDateTime)
                .FirstOrDefaultAsync(stoppingToken);

            if (jwtConfig is null)
            {
                var newJwtConfig = new SecretConfig
                {
                    SecretBytes = TokenUtils.CreateRandomBytes(128),
                    CreatedOnUtc = _dateTimeProvider.UtcNow
                };
                _dbContext.SecretConfigs.Add(newJwtConfig);
                await _dbContext.SaveChangesAsync(stoppingToken);
            }
        }
        catch (DbException dbException)
        {
            throw new SecretProviderDbException(dbException);
        }
    }

    private async Task<TimeSpan> GetTimeToNextRefreshAsync(CancellationToken stoppingToken)
    {
        var config = _config.Value;
        var earliestDate = await _dbContext.SecretConfigs.MinAsync(c => c.CreatedOnUtc, stoppingToken);
        var expireDateTime = earliestDate + _config.Value.SecretLifetimeLength / 2.0;

        return expireDateTime - _dateTimeProvider.UtcNow;
    }
}