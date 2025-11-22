using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Schmeconomics.Api.Secrets;
using Schmeconomics.Api.Time;

namespace Schmeconomics.Api.Tokens.AuthTokens;

/// <summary>
/// Core implementation of <see cref="IAuthTokenProvider"/>
/// </summary>
public class JwtAuthTokenProvider : IAuthTokenProvider
{
    private JwtSecurityTokenHandler _tokenHandler = new();
    private readonly IOptions<JwtAuthTokenProviderConfig> _config;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISecretsProvider _secretsProvider;

    public JwtAuthTokenProvider(
        IOptions<JwtAuthTokenProviderConfig> config,
        IDateTimeProvider dateTimeProvider,
        ISecretsProvider secretsProvider
    ) {
        _config = config;
        _dateTimeProvider = dateTimeProvider;
        _secretsProvider = secretsProvider;

        // Clear the inbound and output claim type mapping
        _tokenHandler.InboundClaimTypeMap.Clear();
        _tokenHandler.OutboundClaimTypeMap.Clear();
    }

    public async Task<string> CreateAuthTokenAsync(
        IDictionary<string, object> claims,
        CancellationToken token = default
    ) {
        var config = _config.Value;

        try
        {
            // TODO - replace with FirstAsync() when it becomes available in .NET 9
            var secretByteIterator = _secretsProvider.GetSecretsAsync(token).GetAsyncEnumerator(token);
            await secretByteIterator.MoveNextAsync();
            byte[]? secretBytes = secretByteIterator.Current;

            var securityKey = new SymmetricSecurityKey(secretBytes!);
            var signingCredentials = new SigningCredentials(securityKey, config.HashAlgorithm.ToSecurityAlgorithmString());

            var jwtToken = _tokenHandler.CreateEncodedJwt(
                config.Issuer,
                config.Audience,
                null,
                null,
                _dateTimeProvider.UtcNow + config.TokenLifetimeLength,
                _dateTimeProvider.UtcNow,
                signingCredentials,
                null,
                claims
            );

            return jwtToken;
        }
        catch (SecretProviderException secretException)
        {
            throw new AuthTokenProviderException.SecretProviderException(secretException);
        }
    }

    public async Task<IReadOnlyList<Claim>> ValidateAuthTokenAsync(string token, CancellationToken cancelToken = default)
    {
        var config = _config.Value;
        Exception? exception = null;
        await foreach (var secretBytes in _secretsProvider.GetSecretsAsync(cancelToken))
        {
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(secretBytes),
                ValidAudience = config.Audience,
                ValidIssuer = config.Issuer,
                LifetimeValidator = (_, expires, _, _) => expires > _dateTimeProvider.UtcNow
            };

            var result = await _tokenHandler.ValidateTokenAsync(token, validationParams);
            if (result.IsValid) return [.. result.ClaimsIdentity.Claims];
            else if (result.Exception != null) exception = result.Exception;
        }
        if (exception != null) 
            throw new AuthTokenProviderException.JwtException(exception);
        else throw new AuthTokenProviderException.TokenInvalidException();
    }
}
