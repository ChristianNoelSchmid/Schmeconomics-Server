using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Schmeconomics.Api.JwtSecrets;
using Schmeconomics.Api.Time;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.UnitTests;

[TestClass]
public sealed class DbSecretsProviderTests
{
    private static readonly DateTime TEST_DATE_TIME = DateTime.UtcNow;

    private readonly static DbSecretsProviderConfig s_config = new()
    {
        SecretLifetimeLength = TimeSpan.FromMilliseconds(200)
    };

    private IDateTimeProvider _dateTimeProvider = default!;
    private DbSecretsProvider _secretsProvider = default!;
    private SchmeconomicsDbContext _dbContext = default!;
    private CancellationTokenSource _serviceToken = default!;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        // Default implementation for mock IDateTimeProvider
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME);
        _dbContext = await DbConfig.CreateSqliteInMemoryDbContextAsync();

        _secretsProvider = new DbSecretsProvider(
            Options.Create(s_config),
            _dbContext,
            _dateTimeProvider
        );

        _serviceToken = new CancellationTokenSource();
    }

    [TestCleanup]
    public async Task TestCleanup() => await _serviceToken.CancelAsync();

    [TestMethod]
    public async Task GetSecretsAsyncProducesSingleSecret()
    {
        var secretBytes = _secretsProvider.GetSecretsAsync().ToBlockingEnumerable().ToList();
        var configs = await _dbContext.SecretConfigs.ToListAsync();
        Assert.AreEqual(1, configs.Count);

        var config = configs[0];
        Assert.AreEqual(1, config.Id);
        Assert.AreEqual(TEST_DATE_TIME, config.CreatedOnUtc);
        CollectionAssert.AreEqual(secretBytes.First(), config.SecretBytes);
    }

    [TestMethod]
    public async Task GetSecretsAsyncPastSecretOverlapProducesSecondSecret()
    {
        var secretBytes1 = _secretsProvider.GetSecretsAsync().ToBlockingEnumerable().ToList();
        // Update the IDateTimeProvider to return a date past overlap time
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME + TimeSpan.FromMilliseconds(150));
        // Run the method again
        var secretBytes2 = _secretsProvider.GetSecretsAsync().ToBlockingEnumerable().ToList();
        CollectionAssert.AreNotEqual(secretBytes1.First(), secretBytes2.First());

        var configs = await _dbContext.SecretConfigs.ToListAsync();

        var config = configs[1];
        Assert.AreEqual(2, config.Id);
        Assert.AreEqual(TEST_DATE_TIME + TimeSpan.FromMilliseconds(150), config.CreatedOnUtc);
        CollectionAssert.AreEqual(secretBytes2.First(), config.SecretBytes);
    }

    [TestMethod]
    public void GetSecretsAsyncPastExpirationRemovesFirstSecret()
    {
        var secretBytes1 = _secretsProvider.GetSecretsAsync().ToBlockingEnumerable().ToList();
        // Update the IDateTimeProvider to return a date past overlap time
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME + TimeSpan.FromMilliseconds(150));
        _ = _secretsProvider.GetSecretsAsync().ToBlockingEnumerable().ToList();
        // Update the IDateTimeProvider to return a date past first secret expiration
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME + TimeSpan.FromMilliseconds(225));
        var secretBytes2 = _secretsProvider.GetSecretsAsync().ToBlockingEnumerable();

        Assert.AreEqual(1, secretBytes2.Count());
        CollectionAssert.AreNotEqual(secretBytes1.First(), secretBytes2.First());
    }

    [TestMethod]
    public void GetSecretsAsyncCancelsTaskWhenCancellationTokenIsCancelled()
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();

        Assert.ThrowsException<OperationCanceledException>(
            () => _secretsProvider.GetSecretsAsync(tokenSource.Token).ToBlockingEnumerable().ToList()
        );
    }
}
