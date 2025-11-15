
using System.ComponentModel.DataAnnotations;

namespace Schmeconomics.Api.JwtSecrets;

public class DbSecretsProviderConfig
{
    /// <summary>
    /// The amount of time a single secret lasts
    /// </summary>
    [Required]
    public TimeSpan SecretLifetimeLength { get; set; }
}