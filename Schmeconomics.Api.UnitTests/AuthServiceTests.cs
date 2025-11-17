using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Schmeconomics.Api.Auth;
using Schmeconomics.Api.Secrets;
using Schmeconomics.Api.Time;
using Schmeconomics.Api.Tokens.AuthTokens;
using Schmeconomics.Api.Tokens.RefreshTokens;
using Schmeconomics.Api.Users;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.UnitTests;

[TestClass]
public class AuthServiceTests
{
    private static readonly string TEST_USER_ID = Guid.NewGuid().ToString();
    private static readonly string TEST_USER_NAME = "Bob";
    private static readonly string TEST_PASSWORD = "password123";
    private static readonly string TEST_PASSWORD_HASH = "hashed_password";
    private static readonly string TEST_IP_ADDRESS = "127.0.0.1";
    private static readonly DateTime TEST_DATE_TIME = DateTime.UtcNow;

    private IDateTimeProvider _dateTimeProvider = default!;
    private SchmeconomicsDbContext _dbContext = default!;
    private IAuthTokenProvider _authTokenProvider = default!;
    private IRefreshTokenProvider _refreshTokenProvider = default!;
    private IPasswordHasher<User> _passwordHasher = default!;
    private AuthService _authService = default!;

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
            Name = TEST_USER_NAME,
            PasswordHash = TEST_PASSWORD_HASH
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _authTokenProvider = Substitute.For<IAuthTokenProvider>();
        _refreshTokenProvider = Substitute.For<IRefreshTokenProvider>();
        _passwordHasher = Substitute.For<IPasswordHasher<User>>();
        
        _authService = new AuthService(_dbContext, _authTokenProvider, _refreshTokenProvider, _passwordHasher);
    }

    [TestMethod]
    public async Task SignInAsync_WithValidCredentials_ReturnsAuthModel()
    {
        // Arrange
        var expectedAccessToken = "test_access_token";
        var expectedRefreshToken = "test_refresh_token";
        var expectedExpiresOnUtc = TEST_DATE_TIME.AddHours(1);
        
        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), TEST_PASSWORD, TEST_PASSWORD_HASH)
            .Returns(PasswordVerificationResult.Success);
        
        _authTokenProvider.CreateAuthTokenAsync(Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(expectedAccessToken);
        
        var refreshTokenFamilyModel = new RefreshTokenFamilyModel(expectedRefreshToken, expectedExpiresOnUtc, new Users.UserModel("", "", Role.User));
        _refreshTokenProvider.CreateNewTokenAsync(TEST_USER_ID, TEST_IP_ADDRESS, Arg.Any<CancellationToken>())
            .Returns(refreshTokenFamilyModel);

        // Act
        var result = await _authService.SignInAsync(TEST_USER_NAME, TEST_PASSWORD, TEST_IP_ADDRESS);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedAccessToken, result.AccessToken);
        Assert.AreEqual(expectedRefreshToken, result.RefreshToken);
        Assert.AreEqual(expectedExpiresOnUtc, result.ExpiresOnUtc);
    }

    [TestMethod]
    public async Task SignInAsync_WithInvalidCredentials_ThrowsInvalidCredentialsException()
    {
        // Arrange
        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<AuthServiceException.InvalidCredentialsException>(
            () => _authService.SignInAsync(TEST_USER_NAME, TEST_PASSWORD, TEST_IP_ADDRESS)
        );
    }

    [TestMethod]
    public async Task SignInAsync_WithNonExistentUser_ThrowsUserNotFoundException()
    {
        // Arrange
        var nonExistentUserName = "NonExistentUser";

        _passwordHasher.VerifyHashedPassword(Arg.Any<User>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(PasswordVerificationResult.Failed);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<AuthServiceException.UserNotFoundException>(
            () => _authService.SignInAsync(nonExistentUserName, TEST_PASSWORD, TEST_IP_ADDRESS)
        );
    }

    [TestMethod]
    public async Task SignOutAsync_CallsRefreshTokenProviderRevokeToken()
    {
        // Arrange
        var refreshToken = "test_refresh_token";

        // Act
        await _authService.SignOutAsync(refreshToken, TEST_IP_ADDRESS);

        // Assert
        await _refreshTokenProvider.Received(1)
            .RevokeTokenAsync(refreshToken, TEST_IP_ADDRESS, Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task RefreshTokenAsync_WithValidRefreshToken_ReturnsAuthModel()
    {
        // Arrange
        var expectedAccessToken = "test_access_token";
        var expectedRefreshToken = "test_refresh_token";
        var expectedExpiresOnUtc = TEST_DATE_TIME.AddHours(1);
        
        var user = new User
        {
            Id = TEST_USER_ID,
            Name = TEST_USER_NAME,
            PasswordHash = "hashed_password",
            Role = Role.User
        };

        var refreshTokenFamilyModel = new RefreshTokenFamilyModel(expectedRefreshToken, expectedExpiresOnUtc, (UserModel)user);
        _refreshTokenProvider.CreateFromTokenAsync(expectedRefreshToken, TEST_IP_ADDRESS, Arg.Any<CancellationToken>())
            .Returns(refreshTokenFamilyModel);

        _authTokenProvider.CreateAuthTokenAsync(Arg.Any<Dictionary<string, object>>(), Arg.Any<CancellationToken>())
            .Returns(expectedAccessToken);

        // Act
        var result = await _authService.RefreshTokenAsync(TEST_IP_ADDRESS, expectedRefreshToken);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedAccessToken, result.AccessToken);
        Assert.AreEqual(expectedRefreshToken, result.RefreshToken);
        Assert.AreEqual(expectedExpiresOnUtc, result.ExpiresOnUtc);
    }

    [TestMethod]
    public async Task RefreshTokenAsync_WithInvalidRefreshToken_ThrowsRefreshTokenProviderException()
    {
        // Arrange
        var refreshToken = "invalid_refresh_token";
        
        _refreshTokenProvider.CreateFromTokenAsync(refreshToken, TEST_IP_ADDRESS, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<RefreshTokenFamilyModel>(new RefreshTokenProviderException.RefreshTokenNotFound(refreshToken, TEST_IP_ADDRESS)));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<RefreshTokenProviderException.RefreshTokenNotFound>(
            () => _authService.RefreshTokenAsync(TEST_IP_ADDRESS, refreshToken)
        );
    }
}
