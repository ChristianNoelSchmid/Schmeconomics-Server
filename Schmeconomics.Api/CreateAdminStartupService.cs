

using Microsoft.EntityFrameworkCore;
using Schmeconomics.Api.Users;
using Schmeconomics.Entities;

public class CreateAdminStartupService(
    IServiceScopeFactory _scopeFactory
) : IHostedService {
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var (db, userService) = (scope.ServiceProvider.GetRequiredService<SchmeconomicsDbContext>(), scope.ServiceProvider.GetRequiredService<IUserService>());

        var adminUser = await db.Users.Where(u => u.Role == Role.Admin).FirstOrDefaultAsync(cancellationToken);
        if(adminUser == null)
        {
            await userService.CreateAdminUser();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) { throw new NotImplementedException();
    }
}