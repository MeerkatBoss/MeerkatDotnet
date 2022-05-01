using MeerkatDotnet.Database.Models;

namespace MeerkatDotnet.Repositories;

public interface IUsersRepository
{
    /// <summary>
    /// Creates a user in database based on input model
    /// </summary>
    /// <param name="userInput">Input model for a user</param>
    /// <returns>Added user</returns>
    /// <exception cref="UsernameTakenException">
    /// Username is take by another user
    /// </exception>
    Task<UserModel> AddUserAsync(UserModel user);

    /// <summary>
    /// Retrieves user with given id from database.
    /// If user with given id does not exist, returns <c>null</c>
    /// </summary>
    /// <param name="id">Id of a user</param>
    /// <returns>Existing user or <c>null</c></returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Provided id was not a positive integer
    /// </exception>
    Task<UserModel?> GetUserAsync(int id);

    /// <summary>
    /// Retrieves user with given username and login.
    /// If such user does not exist, returns <c>null</c>
    /// </summary>
    /// <param name="username">Username used for login</param>
    /// <param name="password">Password user for login</param>
    /// <returns>Existing user or <c>null</c></returns>
    Task<UserModel?> LoginUserAsync(string username, string passwordHash);

    /// <summary>
    /// Updates user in database with given update model
    /// </summary>
    /// <param name="userUpdate">Update model for a user</param>
    /// <returns>Updated user</returns>
    /// <exception cref="UserNotFoundException">
    /// User with provided id does not exist
    /// </exception> 
    Task<UserModel> UpdateUserAsync(UserModel user);

    /// <summary>
    /// Deletes user with given id from database.
    /// </summary>
    /// <param name="id">Id of a user to delete</param>
    /// <exception cref="UserNotFoundException">
    /// User with provided id does not exist
    /// </exception>
    Task DeleteUserAsync(int id);
}