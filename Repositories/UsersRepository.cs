using MeerkatDotnet.Configurations;
using MeerkatDotnet.Database;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Contracts;
using Microsoft.EntityFrameworkCore;
using MeerkatDotnet.Repositories.Exceptions;

namespace MeerkatDotnet.Repositories;

/// <summary>
/// Class containing basic queries for "users" table in database
/// </summary>
public sealed class UsersRepository : IUsersRepository
{
    private AppDbContext _database;

    public UsersRepository(AppDbContext database)
    {
        _database = database;
    }

    public async Task<UserModel> AddUserAsync(UserModel user)
    {
        CodeContract.Requires<UsernameTakenException>(
            await UsernameAvailable(user.Username),
            String.Format(
                "Cannot add user with username=\"{0}\": username is already taken",
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
            String.Format(
                "Cannot get user with id={0}: id must be a positive integer",
                id
            ));
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
        CodeContract.Requires<ArgumentOutOfRangeException>(
            user.Id > 0,
            String.Format(
                "Cannot update user with id={0}: id must be a positive integer",
                user.Id
            )
        );
        CodeContract.Requires<UserNotFoundException>(
            await UserExists(user.Id),
            String.Format(
                "Cannot update user with id={0}: no such user",
                user.Id)
        );
        CodeContract.Requires<UsernameTakenException>(
            await UsernameAvailable(user.Id, user.Username),
            String.Format(
                "Cannot update user with username=\"{0}\": username is already taken",
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
        CodeContract.Requires<ArgumentOutOfRangeException>(
            id > 0,
            String.Format(
                "Cannot delete user with id={0}: id must be a positive integer",
                id
            )
        );
        CodeContract.Requires<UserNotFoundException>(
            await UserExists(id),
            String.Format(
                "Cannot delete user with id={0}: no such user",
                id
            )
        );
        UserModel user = (await _database.Users.FindAsync(id))!;
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
        return _database.Users
            .FindAsync(id)
            .AsTask()
            .ContinueWith(t => t.Result is not null);
    }
}