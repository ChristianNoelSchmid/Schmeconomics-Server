using System.ComponentModel.DataAnnotations;

namespace Schmeconomics.Api.Tokens;

public class DbRefreshTokenProviderConfig
{
    public required TimeSpan RefreshTokenLifetime { get; set; }

    [Required, Range(64, 256)]
    public required uint RefreshTokenLength { get; set; }

    [Required, Range(64, 256)]
    public required uint FamilyTokenLength { get; set; }

    [Required, Range(1, 255)]
    public int IpAddressMaxCount { get; set; }
}