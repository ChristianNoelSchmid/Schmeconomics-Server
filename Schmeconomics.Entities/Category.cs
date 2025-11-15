using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Schmeconomics.Entities;

[PrimaryKey(nameof(Id))]
public class Category 
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public int Balance { get; set; }

    [Required]
    public int RefillValue { get; set; }

    [ForeignKey(nameof(Account))]
    public string AccountId { get; set; } = string.Empty;

    public Account? Account { get; set; } = new();
}