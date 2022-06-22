using MeerkatDotnet.Database;
using MeerkatDotnet.Database.Models;
using Microsoft.EntityFrameworkCore;
using MeerkatDotnet.Repositories.Exceptions;

namespace MeerkatDotnet.Repositories;

public sealed class UsersRepository : IUsersRepository
{
    private AppDbContext _database;

    public UsersRepository(AppDbContext database)
    {
        _database = database;
    }

    public async Task<UserModel> AddUserAsync(UserModel user)
    {
        if(!await UsernameAvailable(user.Username))
        {
            throw new UsernameTakenException(
                    $"Cannot add user: username \"{user.Username}\" is already taken");
        }

        await _database.Users.AddAsync(user);
        await _database.SaveChangesAsync();
        return user.Clone();
    }

    public Task<UserModel?> GetUserAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                    $"Cannot get user: id={id} is not a positive integer");
        }

        return _database.Users.AsNoTracking()
            .Where(u => u.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<UserModel?> LoginUserAsync(string username, string passwordHash)
    {
        return await _database.Users.AsNoTracking()
            .Where(user => user.Username == username
                        && user.PasswordHash == passwordHash)
            .FirstOrDefaultAsync();
    }

    public async Task<UserModel> UpdateUserAsync(UserModel user)
    {
        if (user.Id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                    $"Cannot update user: id={user.Id} is not a positive integer");
        }
        if (!await UserExists(user.Id))
        {
            throw new UserNotFoundException(
                    $"Cannot update user: no user with id={user.Id} found");
        }
        if (!await UsernameAvailable(user.Id, user.Username))
        {
            throw new UsernameTakenException(
                    $"Cannot update user: username \"{user.Username}\" is already taken");
        }
        UserModel dbUser = (await _database.Users.FindAsync(user.Id))!;
        _database.Entry(dbUser).CurrentValues.SetValues(user);
        _database.Entry(dbUser).DetectChanges();
        await _database.SaveChangesAsync();
        return user.Clone();
    }

    public async Task DeleteUserAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                    $"Cannot delete user: id={id} is not a positive integer");
        }
        if (!await UserExists(id))
        {
            throw new UserNotFoundException(
                    $"Cannot delete user: no user with id={id} found");
        }

        UserModel user = (await _database.Users.FindAsync(id))!;
        _database.Users.Remove(user);
        await _database.SaveChangesAsync();
    }

    private Task<bool> UsernameAvailable(string username)
    {
        return _database.Users
                .Where(u => u.Username == username)
                .FirstOrDefaultAsync()
                .ContinueWith(t => t.Result is null);
    }

    private Task<bool> UsernameAvailable(int id, string username)
    {
        return _database.Users
            .Where(u => u.Id != id && u.Username == username)
            .FirstOrDefaultAsync()
            .ContinueWith(t => t.Result is null);
    }

    private Task<bool> UserExists(int id)
    {
        return _database.Users
            .FindAsync(id)
            .AsTask()
            .ContinueWith(t => t.Result is not null);
    }
}
