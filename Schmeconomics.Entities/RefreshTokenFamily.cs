using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace Schmeconomics.Entities;

[Index(nameof(FamilyToken))]
public class RefreshTokenFamily
{
    [Key]
    public int Id { get; set; }

    [Required, ForeignKey(nameof(User))]
    public required string UserId { get; set; }

    /// <summary>
    /// The current, active refresh token
    /// </summary>
    public string? ActiveToken { get; set; }

    /// <summary>
    /// The family token which identifies the collection of tokens
    /// belonging to the signed in user
    /// </summary>
    [Required]
    public required string FamilyToken { get; set; }

    /// <summary>
    /// The time that this token family will expire if not refreshed
    /// </summary>
    [Required]
    public DateTime ExpiresOnUtc { get; set; }

    /// <summary>
    /// The time that this token family was revoked (ie. signed out)
    /// </summary>
    public DateTime? RevokedOnUtc { get; set; }

    /// <summary>
    /// The IP addresses of clients who have recently refreshed.
    /// If the token has been revoked, the last IP in this list is
    /// the one that revoked the token (whether through attempt to
    /// use a stale token or through purposeful sign-out)
    /// </summary>
    public List<string> RecentIpAddresses { get; set; } = [];

    /// <summary>
    /// The <see cref="User"/> the <see cref="RefreshTokenFamily"/> has
    /// been created for
    /// </summary>
    public User? User { get; set; }
}