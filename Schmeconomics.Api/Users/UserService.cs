using System.Data.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Schmeconomics.Entities;

namespace Schmeconomics.Api.Users;

public class UserService(
    SchmeconomicsDbContext db,
    IPasswordHasher<User> passwordHasher
) : IUserService {
    private readonly SchmeconomicsDbContext _db = db;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    public async Task<Result<IEnumerable<UserModel>>> GetAllUsersAsync()
    {
        try
        {
            var users = await _db.Users
                .OrderBy(u => u.Name)
                .Select(u => (UserModel)u)
                .ToListAsync();

            return users;
        }
        catch (DbException ex)
        {
            throw new UserServiceException.DbException(ex);
        }
    }

    public async Task<Result> CreateAdminUser()
    {
        try 
        {
            var user = new User { 
                Id = Guid.NewGuid().ToString(), 
                Name = "Admin",
                Role = Role.Admin,
            };
            var hash = _passwordHasher.HashPassword(user, "admin");
            user.PasswordHash = hash;

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return Result.Ok();
        }
        catch(DbException ex)
        {
            throw new UserServiceException.DbException(ex);
        }
    }


    public async Task<UserModel> CreateUserAsync(string name, string password, CancellationToken token)
    {
        if (await GetUserFromName(name, token) != null)
            throw new UserServiceException.NameReuse(name);
        
        var user = new User { Id = Guid.NewGuid().ToString(), Name = name, };
        var passwordHash = _passwordHasher.HashPassword(user, password);
        user.PasswordHash = passwordHash;

        try
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(token);
            return (UserModel)user;
        }
        catch (DbException ex)
        {
            throw new UserServiceException.DbException(ex);
        }
    }

    public async Task<UserModel?> GetUserFromIdAsync(string id, CancellationToken token)
    {
        try 
        {
            var user = await _db.Users.FindAsync([id], token);
            if (user != null) return (UserModel)user;
            return null;
        } 
        catch(DbException ex)
        {
            throw new UserServiceException.DbException(ex);
        }
    }

    public async Task<UserModel?> GetUserFromName(string name, CancellationToken token)
    {
        try 
        {
            var user = await _db.Users.Where(u => u.Name == name).FirstOrDefaultAsync(token);
            if(user != null) return (UserModel)user;
            return null;
        } 
        catch(DbException ex)
        {
            throw new UserServiceException.DbException(ex);
        }
    }

    public async Task<UserModel?> UpdateUserAsync(
        string id,
        string? name,
        string? password,
        CancellationToken stopToken
    ) {
        var user = await _db.Users.FindAsync([id], stopToken);
        if (user == null) return null;

        if (name != null)
        {
            if (await GetUserFromName(name, stopToken) != null)
                throw new UserServiceException.NameReuse(name);

            user.Name = name;
        }
        if (password != null)
        {
            var passwordHash = _passwordHasher.HashPassword(user, password);
            user.PasswordHash = passwordHash;
        }

        try
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync(stopToken);
            return (UserModel)user;
        }
        catch (DbException ex)
        {
            throw new UserServiceException.DbException(ex);
        }
    }

    public async Task<UserModel?> DeleteUserAsync(string id, CancellationToken stopToken)
    {
        var user = await _db.Users.FindAsync([id], stopToken);
        if (user == null) return null;
        try
        {
            _db.Users.Remove(user);
            await _db.SaveChangesAsync(stopToken);
            return (UserModel)user;
        }
        catch (DbException ex)
        {
            throw new UserServiceException.DbException(ex);
        }
    }
}