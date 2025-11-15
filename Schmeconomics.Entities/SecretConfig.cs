using System.ComponentModel.DataAnnotations;

namespace Schmeconomics.Entities;

public class SecretConfig
{
    [Key, Required]
    public int Id { get; set; }

    [Required, Length(512, 512)]
    public byte[] SecretBytes { get; set; } = [];

    public DateTime CreatedOnUtc { get; set; }
}