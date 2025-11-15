
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.UnitTests;

public static class DbConfig
{
    public static async Task<SchmeconomicsDbContext> CreateSqliteInMemoryDbContextAsync()
    {
        var sqliteConnection = new SqliteConnection("Filename=:memory:");
        await sqliteConnection.OpenAsync();

        var options = new DbContextOptionsBuilder<SchmeconomicsDbContext>()
            .UseSqlite(sqliteConnection)
            .Options;

        var dbContext = new SchmeconomicsDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();


        return dbContext;
    }
}