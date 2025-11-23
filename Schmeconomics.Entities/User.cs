using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Schmeconomics.Entities;

[Index(nameof(Name))]
public class User
{
    [Required, Key]
    public string Id { get; set; } = string.Empty;
    
    [Required]
    public Role Role { get; set; } 

    [Required, StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    public ICollection<AccountUser> AccountUsers { get; set;} = [];
}