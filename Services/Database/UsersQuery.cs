using MeerkatDotnet.Configurations;
using MeerkatDotnet.Database;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Models;
using MeerkatDotnet.Services.Database.Exceptions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace MeerkatDotnet.Services.Database;

/// <summary>
/// Class containing basic queries for "users" table in database
/// </summary>
public sealed class UsersQuery
{
    private AppDbContext _database;
    private HashingOptions _hashingOptions;

    public UsersQuery(AppDbContext database, HashingOptions hashingOptions)
    {
        _database = database;
        _hashingOptions = hashingOptions;
    }

    /// <summary>
    /// Creates a user in database based on input model
    /// </summary>
    /// <param name="userInput">Input model for a user</param>
    /// <returns>Added user</returns>
    public async Task<UserModel> AddUserAsync(UserInputModel userInput)
    {
        var user = new UserModel(
            username: userInput.Username,
            passwordHash: GetHash(userInput.Password),
            email: userInput.Email,
            phone: userInput.Phone);
        await _database.Users.AddAsync(user);
        _database.Users.Update(user);
        await _database.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Retrieves user with given id from database.
    /// If user with given id does not exist, returns <c>null</c>
    /// </summary>
    /// <param name="id">Id of a user</param>
    /// <returns>Existing user or <c>null</c></returns>
    public async Task<UserModel?> GetUserAsync(int id)
        => await _database.Users.FindAsync(id);

    /// <summary>
    /// Retrieves user with given username and login.
    /// If such user does not exist, returns <c>null</c>
    /// </summary>
    /// <param name="username">Username used for login</param>
    /// <param name="password">Password user for login</param>
    /// <returns>Existing user or <c>null</c></returns>
    public UserModel? LoginUser(string username, string password)
        => _database.Users
            .Where(user => user.Username == username
                        && user.PasswordHash == GetHash(password))
            .FirstOrDefault();

    /// <summary>
    /// Updates user in database with given update model
    /// </summary>
    /// <param name="userUpdate">Update model for a user</param>
    /// <returns>Updated user</returns>
    /// <exception cref="UserNotFoundException">
    /// User with provided id does not exist
    /// </exception> 
    public async Task<UserModel> UpdateUserAsync(int id, UserUpdateModel userUpdate)
    {
        var user = await GetUserAsync(id);
        if (user is null)
            throw new UserNotFoundException(
                String.Format("User with id={0} was not found", id)
            );
        if (userUpdate.Username is not null)
            user.Username = userUpdate.Username;
        if (userUpdate.Password is not null)
            user.PasswordHash = GetHash(userUpdate.Password);
        if (userUpdate.Email is not null)
            user.Email = userUpdate.Email;
        if (userUpdate.Phone is not null)
            user.Phone = userUpdate.Phone;
        await _database.SaveChangesAsync();
        return user;
    }

    /// <summary>
    /// Deletes user with given id from database.
    /// </summary>
    /// <param name="id">Id of a user to delete</param>
    /// <exception cref="UserNotFoundException">
    /// User with provided id does not exist
    /// </exception>
    public async Task DeleteUser(int id)
    {
        var user = await GetUserAsync(id);
        if (user is null)
            throw new UserNotFoundException(
                String.Format("User with id={0} was not found", id)
            );
        _database.Users.Remove(user);
        await _database.SaveChangesAsync();
    }

    private string GetHash(string password)
    {
        byte[] bytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: _hashingOptions.Salt,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: _hashingOptions.IterationCount,
            numBytesRequested: 256
        );
        return Convert.ToBase64String(bytes);
    }
}