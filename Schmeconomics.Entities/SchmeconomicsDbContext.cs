using Microsoft.EntityFrameworkCore;

namespace Schmeconomics.Entities;

public class SchmeconomicsDbContext(
    DbContextOptions<SchmeconomicsDbContext> options
) : DbContext(options) {
    public DbSet<SecretConfig> SecretConfigs { get; private set; } = null!;
    public DbSet<Account> Accounts { get; private set; } = null!;
    public DbSet<User> Users { get; private set; } = null!;
    public DbSet<AccountUser> AccountUsers { get; private set; } = null!;
    public DbSet<Category> Categories { get; private set; } = null!;
    public DbSet<Transaction> Transactions { get; private set; } = null!;
    public DbSet<RefreshTokenFamily> RefreshTokenFamilies { get; private set; } = null!;
}
