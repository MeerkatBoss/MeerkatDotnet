using MeerkatDotnet.Configurations;
using MeerkatDotnet.Database;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Services.Database.Exceptions;
using MeerkatDotnet.Contracts;
using Microsoft.EntityFrameworkCore;

namespace MeerkatDotnet.Services.Database;

/// <summary>
/// Class containing basic queries for "users" table in database
/// </summary>
public sealed class UsersQuery : IUsersQuery
{
    private AppDbContext _database;

    public UsersQuery(AppDbContext database)
    {
        _database = database;
    }

    public async Task<UserModel> AddUserAsync(UserModel user)
    {
        CodeContract.Requires<UsernameTakenException>(
            await UsernameAvailable(user.Username),
            String.Format(
                "Username \"{0}\" is already taken",
                user.Username
            )
        );

        await _database.Users.AddAsync(user);
        await _database.SaveChangesAsync();
        return user.Clone();
    }

    public Task<UserModel?> GetUserAsync(int id)
    {
        CodeContract.Requires<ArgumentOutOfRangeException>(
            id > 0,
            "Id must be a positive integer");
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
        CodeContract.Requires<UserNotFoundException>(
            await UserExists(user.Id),
            String.Format("User with id={0} was not found", user.Id)
        );
        CodeContract.Requires<UsernameTakenException>(
            await UsernameAvailable(user.Id, user.Username),
            String.Format(
                "Username \"{0}\" is already taken",
                user.Username
            )
        );
        UserModel dbUser = (await _database.Users.FindAsync(user.Id))!;
        _database.Entry(dbUser).CurrentValues.SetValues(user);
        _database.Entry(dbUser).DetectChanges();
        await _database.SaveChangesAsync();
        return user.Clone();
    }

    public async Task DeleteUserAsync(int id)
    {
        CodeContract.Requires<UserNotFoundException>(
            await UserExists(id),
            String.Format("User with id={0} was not found", id)
        );
        UserModel user = (await GetUserAsync(id))!;
        _database.Users.Remove(user);
        await _database.SaveChangesAsync();
    }

    // private string GetHash(string password)
    // {
    //     byte[] bytes = KeyDerivation.Pbkdf2(
    //         password: password,
    //         salt: _hashingOptions.SaltBytes,
    //         prf: KeyDerivationPrf.HMACSHA256,
    //         iterationCount: _hashingOptions.IterationCount,
    //         numBytesRequested: 256
    //     );
    //     return Convert.ToBase64String(bytes);
    // }

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
        CodeContract.Requires<ArgumentOutOfRangeException>(
            id > 0,
            "Id must be a positive integer");
        return _database.Users
            .FindAsync(id)
            .AsTask()
            .ContinueWith(t => t.Result is not null);
    }
}