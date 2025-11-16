namespace Schmeconomics.Api.Tokens.AuthTokens;

using System.ComponentModel.DataAnnotations;


/// <summary>
/// Configuration for the <see cref="JwtAuthTokenProvider"/> class
/// </summary>
public class JwtAuthTokenProviderConfig
{
    /// <summary>
    /// Hashing algorithm to use for JWT generation
    /// </summary>
    [Required]
    public JwtHashAlgorithm HashAlgorithm { get; set; }

    /// <summary>
    /// The issuer providing the token
    /// </summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// The audience for the token
    /// </summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    public TimeSpan TokenLifetimeLength { get; set; } = TimeSpan.FromMinutes(5);
}