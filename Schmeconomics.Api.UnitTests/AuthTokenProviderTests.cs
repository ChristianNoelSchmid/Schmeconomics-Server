using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Schmeconomics.Api.Secrets;
using Schmeconomics.Api.Time;
using Schmeconomics.Api.Tokens.AuthTokens;

namespace Schmeconomics.Api.UnitTests;

[TestClass]
public class AuthTokenProviderTests
{
    private const string TEST_ISSUER = "TEST_ISSUER";
    private const string TEST_AUDIENCE = "TEST_AUDIENCE";

    private static readonly JwtAuthTokenProviderConfig s_config = new JwtAuthTokenProviderConfig
    {
        HashAlgorithm = JwtHashAlgorithm.HmacSha256,
        Issuer = TEST_ISSUER,
        Audience = TEST_AUDIENCE,
        TokenLifetimeLength = TimeSpan.FromSeconds(10)
    };

    private static async IAsyncEnumerable<byte[]> TEST_SECRET_BYTES_ITERATOR()
    {
        yield return Encoding.UTF8.GetBytes("secret_secret_secret_secret_secret");
        yield return Encoding.UTF8.GetBytes("secret_secret_secret_secret_secret_2");
    }
    private static async IAsyncEnumerable<byte[]> TEST_SECRET_BYTES_ITERATOR_2()
    {
        yield return Encoding.UTF8.GetBytes("secret_secret_secret_secret_secret_3");
    }
    private static async IAsyncEnumerable<byte[]> TEST_SECRET_BYTES_ITERATOR_OLDER()
    {
        yield return Encoding.UTF8.GetBytes("secret_secret_secret_secret_secret_2");
    }
    private static readonly DateTime TEST_DATE_TIME = DateTime.UtcNow;

    private JwtAuthTokenProvider _tokenProvider = default!;
    private IDateTimeProvider _dateTimeProvider = default!;
    private ISecretsProvider _secretsProvider = default!;

    [TestInitialize]
    public void TestInitialize()
    {
        _dateTimeProvider = Substitute.For<IDateTimeProvider>();
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME);
        _secretsProvider = Substitute.For<ISecretsProvider>();
        _secretsProvider.GetSecretsAsync().Returns(TEST_SECRET_BYTES_ITERATOR());
        _tokenProvider = new JwtAuthTokenProvider(Options.Create(s_config), _dateTimeProvider, _secretsProvider);
    }

    [TestMethod]
    public async Task CreateAuthTokenCreatesValidJwtWithCorrectClaims()
    {
        var subject = Guid.NewGuid().ToString();
        var token = await _tokenProvider.CreateAuthTokenAsync(
            new Dictionary<string, object> {
                { "sub", subject },
                { "name", "Bob" },
                { "role", "Esteemed" },
            }
        );

        var handler = new JwtSecurityTokenHandler();
        var outToken = handler.ReadJwtToken(token);

        Assert.AreEqual(TEST_ISSUER, outToken.Issuer);
        CollectionAssert.AreEqual(new string[] { TEST_AUDIENCE }, outToken.Audiences.ToArray());
        Assert.AreEqual(subject, outToken.Claims.First(c => c.Type == "sub").Value);
        Assert.AreEqual("Bob", outToken.Claims.First(c => c.Type == "name").Value);
        Assert.AreEqual("Esteemed", outToken.Claims.First(c => c.Type == "role").Value);

        Assert.AreEqual(TEST_DATE_TIME.Ticks - (TEST_DATE_TIME.Ticks % TimeSpan.TicksPerSecond), outToken.IssuedAt.Ticks);
        var expectedEpochTime = _dateTimeProvider.UtcNow + s_config.TokenLifetimeLength;
        var expectedTicks = expectedEpochTime.Ticks - (expectedEpochTime.Ticks % TimeSpan.TicksPerSecond);
        Assert.AreEqual(expectedTicks, outToken.ValidTo.Ticks);

        var validated = await handler.ValidateTokenAsync(
            token,
            new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    TEST_SECRET_BYTES_ITERATOR().ToBlockingEnumerable().First()
                ),
            }
        );

        Assert.IsTrue(validated.IsValid);
    }

    [TestMethod]
    public async Task ValidateAuthTokenReturnsClaimsOnSuccess()
    {
        var subject = Guid.NewGuid().ToString();
        var jwtToken = CreateTestJwtToken(
            claims: new()
            {
                ["sub"] = subject,
                ["name"] = "Jimothy",
                ["group"] = "1012"
            }
        );

        var claims = await _tokenProvider.ValidateAuthTokenAsync(jwtToken);
        Assert.AreEqual(subject, claims.First(c => c.Type == "sub").Value);
        Assert.AreEqual("Jimothy", claims.First(c => c.Type == "name").Value);
        Assert.AreEqual("1012", claims.First(c => c.Type == "group").Value);
    }

    [TestMethod]
    public async Task ValidateAuthTokenThrowsExceptionForExpiredTokens()
    {
        var subject = Guid.NewGuid().ToString();
        var jwtToken = CreateTestJwtToken(claims: []);
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME + TimeSpan.FromDays(30));

        await Assert.ThrowsExceptionAsync<AuthTokenProviderException.JwtException>(() => _tokenProvider.ValidateAuthTokenAsync(jwtToken));
    }

    [TestMethod]
    public async Task ValidateAuthTokenThrowsExceptionWithIncorrectAudience()
    {
        var subject = Guid.NewGuid().ToString();
        var jwtToken = CreateTestJwtToken(claims: [], audience: "INVALID_AUDIENCE");
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME + TimeSpan.FromDays(30));

        await Assert.ThrowsExceptionAsync<AuthTokenProviderException.JwtException>(() => _tokenProvider.ValidateAuthTokenAsync(jwtToken));
    }

    [TestMethod]
    public async Task ValidateAuthTokenThrowsExceptionWithIncorrectIssuer()
    {
        var subject = Guid.NewGuid().ToString();
        var jwtToken = CreateTestJwtToken(claims: [], issuer: "INVALID_ISSUER");
        _dateTimeProvider.UtcNow.Returns(TEST_DATE_TIME + TimeSpan.FromDays(30));

        await Assert.ThrowsExceptionAsync<AuthTokenProviderException.JwtException>(() => _tokenProvider.ValidateAuthTokenAsync(jwtToken));
    }

    [TestMethod]
    public async Task GenerateAuthTokenAndValidateAuthTokenWorkProperlyTogether()
    {
        var subjectId = Guid.NewGuid().ToString();
        var jwtToken = await _tokenProvider.CreateAuthTokenAsync(
            new Dictionary<string, object>
            {
                ["sub"] = subjectId,
                ["email"] = "tom.bombadil@middleearthmail.com"
            }
        );
        var claims = await _tokenProvider.ValidateAuthTokenAsync(jwtToken);

        Assert.AreEqual(subjectId, claims.First(c => c.Type == "sub").Value);
        Assert.AreEqual("tom.bombadil@middleearthmail.com", claims.First(c => c.Type == "email").Value);
    }

    [TestMethod]
    public async Task ValidateAuthTokenThrowsExceptionWithIncorrectSecret()
    {
        var subjectId = Guid.NewGuid().ToString();
        var jwtToken = await _tokenProvider.CreateAuthTokenAsync(
            new Dictionary<string, object>
            {
                ["sub"] = subjectId,
                ["email"] = "corgis.r.gems@mail.com"
            }
        );

        _secretsProvider.GetSecretsAsync().Returns(TEST_SECRET_BYTES_ITERATOR_2());
        await Assert.ThrowsExceptionAsync<AuthTokenProviderException.JwtException>(() => _tokenProvider.ValidateAuthTokenAsync(jwtToken));
    }

    [TestMethod]
    public async Task ValidateAuthTokenSucceedsWhenOlderJwtSecretIsProvided()
    {
        _secretsProvider.GetSecretsAsync().Returns(TEST_SECRET_BYTES_ITERATOR_OLDER());
        var jwtToken = await _tokenProvider.CreateAuthTokenAsync(new Dictionary<string, object>());
        // The secret provided should be the second in the list for this iterator.
        // The token should still be successfully validated
        _secretsProvider.GetSecretsAsync().Returns(TEST_SECRET_BYTES_ITERATOR());
        await _tokenProvider.ValidateAuthTokenAsync(jwtToken);
    }

    private string CreateTestJwtToken(
        Dictionary<string, object> claims,
        string issuer = TEST_ISSUER,
        string audience = TEST_AUDIENCE
    ) {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(TEST_SECRET_BYTES_ITERATOR().ToBlockingEnumerable().First()),
            s_config.HashAlgorithm.ToSecurityAlgorithmString()
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.OutboundClaimTypeMap.Clear();
        var jwtToken = tokenHandler.CreateEncodedJwt(
            issuer,
            audience,
            null,
            null,
            _dateTimeProvider.UtcNow + s_config.TokenLifetimeLength,
            _dateTimeProvider.UtcNow,
            signingCredentials,
            null,
            claims
        );

        return jwtToken;
    }
}