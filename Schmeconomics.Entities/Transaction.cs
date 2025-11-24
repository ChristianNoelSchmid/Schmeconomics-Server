
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Schmeconomics.Entities;

[PrimaryKey(nameof(Id))]
public class Transaction {
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required, ForeignKey(nameof(Category))]
    public string CategoryId { get; set; } = string.Empty;

    [Required]
    public DateTime TimestampUtc { get; set;} = DateTime.UtcNow;

    [Required]
    public int Amount { get; set; }

    [MaxLength(255)]
    public string? Notes { get; set; }

    [Required, ForeignKey(nameof(Account))]
    public string AccountId { get; set; } = string.Empty;

    public Category? Category { get; set; }
    
    public Account? Account { get; set; }
}
