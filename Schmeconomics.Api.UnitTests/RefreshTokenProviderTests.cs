using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Schmeconomics.Api.Secrets;
using Schmeconomics.Api.Time;
using Schmeconomics.Api.Tokens;
using Schmeconomics.Api.Tokens.AuthTokens;
using Schmeconomics.Api.Tokens.RefreshTokens;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.UnitTests;

[TestClass]
public sealed class RefreshTokenProviderTests
{
    private static readonly string TEST_USER_ID = Guid.NewGuid().ToString();
    private static readonly DateTime TEST_DATE_TIME = DateTime.UtcNow;
    private static readonly string TEST_IP_ADDRESS = "127.0.0.1";
    private static readonly DbRefreshTokenProviderConfig s_config = new()
    {
        FamilyTokenLength = 10,
        RefreshTokenLength = 10,
        RefreshTokenLifetime = TimeSpan.FromSeconds(10),
        IpAddressMaxCount = 5
    };

    private IDateTimeProvider _dateTimeProvider = default!;
    private SchmeconomicsDbContext _dbContext = default!;
    private DbRefreshTokenProvider _refreshTokenProvider = default!;

    [TestInitialize]
    public async Task TestInitialize()
    {
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME);
        _dbContext = await DbConfig.CreateSqliteInMemoryDbContextAsync();

        // Create a user to associate the refresh token families with
        var user = new User
        {
            Id = TEST_USER_ID,
            Name = "Bob",
            PasswordHash = string.Empty
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _refreshTokenProvider = new DbRefreshTokenProvider(Options.Create(s_config), _dbContext, _dateTimeProvider);
    }

    [TestMethod]
    public async Task CreateNewTokenAsyncCreatesNewTokenAndInputsIntoDatabase()
    {
        var tokenModel = await _refreshTokenProvider.CreateNewTokenAsync(TEST_USER_ID, TEST_IP_ADDRESS);

        var familyTokens = await _dbContext.RefreshTokenFamilies.ToListAsync();
        Assert.AreEqual(1, familyTokens.Count);

        var familyToken = familyTokens.First();
        CollectionAssert.AreEqual(familyToken.RecentIpAddresses, new List<string>() { TEST_IP_ADDRESS });
        Assert.AreEqual(TEST_DATE_TIME + s_config.RefreshTokenLifetime, familyToken.ExpiresOnUtc);
        Assert.AreEqual(1, familyToken.Id);
        Assert.AreEqual(TEST_USER_ID, familyToken.UserId);
        Assert.AreEqual(tokenModel.Token.Split('.')[0], familyToken.FamilyToken);
        Assert.AreEqual(tokenModel.Token.Split('.')[1], familyToken.ActiveToken);
    }

    [TestMethod]
    public async Task CreateFromTokenAsyncCreatesNewActiveTokenAndUpdatesFamilyTokenInDatabase()
    {
        // Create the initial token
        var tokenModel = await _refreshTokenProvider.CreateNewTokenAsync(TEST_USER_ID, TEST_IP_ADDRESS);

        // Update the date time provider to return 5 seconds into the future, in order
        // to check expiration set
        var futureTime = TEST_DATE_TIME + TimeSpan.FromSeconds(5);
        _dateTimeProvider.UtcNow.Returns(futureTime);

        var newTokenModel = await _refreshTokenProvider.CreateFromTokenAsync(tokenModel.Token, TEST_IP_ADDRESS);

        var familyTokens = await _dbContext.RefreshTokenFamilies.ToListAsync();
        Assert.AreEqual(1, familyTokens.Count);
        var familyToken = familyTokens.First();

        // There should be two recordings of the test IP address now
        CollectionAssert.AreEqual(familyToken.RecentIpAddresses, Enumerable.Repeat(TEST_IP_ADDRESS, 2).ToArray());

        // ID, user and TEST_DATE_TIME should be the same as the first token
        Assert.AreEqual(futureTime + s_config.RefreshTokenLifetime, familyToken.ExpiresOnUtc);
        Assert.AreEqual(1, familyToken.Id);
        Assert.AreEqual(TEST_USER_ID, familyToken.UserId);

        // The prior token and active token should both have the same family token
        Assert.AreEqual(tokenModel.Token.Split('.')[0], familyToken.FamilyToken);
        Assert.AreEqual(newTokenModel.Token.Split('.')[0], familyToken.FamilyToken);
        Assert.AreEqual(newTokenModel.Token.Split('.')[1], familyToken.ActiveToken);
    }

    public async Task CreateNewTokenAsyncThrowsExceptionWhenNonexistantTokenIsProvided()
    {
        await Assert.ThrowsExceptionAsync<RefreshTokenProviderException.UserIdNotFound>(
            () => _refreshTokenProvider.CreateNewTokenAsync(Guid.NewGuid().ToString(), TEST_IP_ADDRESS)
        );
    }

    [TestMethod]
    public async Task CreateFromTokenAsyncThrowsExceptionWhenIncorrectTokenIsProvided()
    {
        var tokenModel = await _refreshTokenProvider.CreateNewTokenAsync(TEST_USER_ID, TEST_IP_ADDRESS);
        var splits = tokenModel.Token.Split(".");

        // Updating the family token should throw exception
        var malformedToken1 = $"{Convert.ToBase64String(TokenUtils.CreateRandomBytes(16))}.{splits[1]}";
        await Assert.ThrowsExceptionAsync<RefreshTokenProviderException.RefreshTokenNotFound>(
            () => _refreshTokenProvider.CreateFromTokenAsync(malformedToken1, TEST_IP_ADDRESS)
        );

        // Updating the active token should throw exception
        var malformedToken2 = $"{splits[0]}.{Convert.ToBase64String(TokenUtils.CreateRandomBytes(16))}";
        await Assert.ThrowsExceptionAsync<RefreshTokenProviderException.RefreshTokenDoesNotMatch>(
            () => _refreshTokenProvider.CreateFromTokenAsync(malformedToken2, TEST_IP_ADDRESS)
        );
    }

    [TestMethod]
    public async Task CreateFromTokenAsyncThrowsExceptionWhenTokenContainsInvalidSegmentCount()
    {
        for (int i = 0; i < 3; i += 1)
        {
            if (i == 2) continue;
            var token = string.Join(
                ".",
                Enumerable.Range(0, i)
                    .Select(_ => Convert.ToBase64String(TokenUtils.CreateRandomBytes(16)))
            );
            await Assert.ThrowsExceptionAsync<RefreshTokenProviderException.MalformedRefreshToken>(
                () => _refreshTokenProvider.CreateFromTokenAsync(token, TEST_IP_ADDRESS)
            );
        }
    }

    [TestMethod]
    public async Task CreateFromTokenAsyncThrowsExceptionWhenTokenIsStale()
    {
        var tokenModel = await _refreshTokenProvider.CreateNewTokenAsync(TEST_USER_ID, TEST_IP_ADDRESS);

        // Token that is past expiration should throw exception
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME + TimeSpan.FromHours(2));

        await Assert.ThrowsExceptionAsync<RefreshTokenProviderException.RefreshTokenStale>(
            () => _refreshTokenProvider.CreateFromTokenAsync(tokenModel.Token, TEST_IP_ADDRESS)
        );

        // Token that is revoked should throw exception
        var tokenModel2 = await _refreshTokenProvider.CreateNewTokenAsync(TEST_USER_ID, TEST_IP_ADDRESS);
        await _refreshTokenProvider.RevokeTokenAsync(tokenModel2.Token, TEST_IP_ADDRESS);

        await Assert.ThrowsExceptionAsync<RefreshTokenProviderException.RefreshTokenStale>(
            () => _refreshTokenProvider.CreateFromTokenAsync(tokenModel2.Token, TEST_IP_ADDRESS)
        );
    }

    public async Task RevokeTokenAsyncRevokesFamilyToken()
    {
        var tokenModel = await _refreshTokenProvider.CreateNewTokenAsync(TEST_USER_ID, TEST_IP_ADDRESS);
        await _refreshTokenProvider.RevokeTokenAsync(tokenModel.Token, TEST_IP_ADDRESS);

        var familyTokens = await _dbContext.RefreshTokenFamilies.ToListAsync();
        Assert.AreEqual(1, familyTokens.Count);

        var familyToken = familyTokens[0];
        Assert.IsNull(familyToken.ActiveToken);
        Assert.AreEqual(TEST_DATE_TIME, familyToken.RevokedOnUtc);

        // There should be two recordings of the test IP address now
        CollectionAssert.AreEqual(familyToken.RecentIpAddresses, Enumerable.Repeat(TEST_IP_ADDRESS, 2).ToArray());
    }
}