using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Schmeconomics.Entities;

[PrimaryKey(nameof(Id))]
public class Account 
{
    [Required]
    public string Id { get; set; } = string.Empty;
    
    [Required, StringLength(255)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Category> Categories { get; } = [];
    
    public ICollection<AccountUser> AccountUsers { get; } = [];
}
