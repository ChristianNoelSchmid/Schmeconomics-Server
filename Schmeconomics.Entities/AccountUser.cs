using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Schmeconomics.Entities;

[PrimaryKey(nameof(AccountId), nameof(UserId))]
public class AccountUser 
{
    [ForeignKey(nameof(Account))]
    public string AccountId { get; set; } = string.Empty;

    [ForeignKey(nameof(User))]
    public string UserId { get; set; } = string.Empty;

    public Account? Account { get; set; }
    public User? User { get; set; }
}