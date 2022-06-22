using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Repositories.Exceptions;

namespace MeerkatDotnet.Repositories;

/// <summary>
/// Repository for managing "tokens" table in database
/// </summary>
public interface IRefreshTokensRepository
{

    /// <summary>
    /// Creates refresh token in database based on provided model
    /// </summary>
    /// <param name="token">Token model to add to database</param>
    /// <returns>Added token</returns>
    /// <exception cref="TokenExistsException">
    /// Tokens with the same value already exists in the database
    /// </exception>
    /// <exception cref="TokenExpiredException">
    /// Trying to add expired token
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// User id is not a positive integer
    /// </exception>
    /// <exception cref="UserNotFoundException">
    /// User with provided id doesn't exist
    /// </exception>
    Task<RefreshTokenModel> AddTokenAsync(RefreshTokenModel token);

    /// <summary>
    /// Retrives token with the specified id.
    /// If no such token found, returns null
    /// </summary>
    /// <param name="token">value of a token</param>
    /// <returns>Retrived token or null</returns>
    Task<RefreshTokenModel?> GetTokenAsync(string token);

    /// <summary>
    /// Retrives all tokens of a user
    /// If user has no tokens, returns null
    /// </summary>
    /// <param name="userId">Id of a user</param>
    /// <returns>ICollection of tokens or null</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// User id is not a positive integer
    /// </exception>
    /// <exception cref="UserNotFoundException">
    /// User with provided id doesn't exist
    /// </exception>
    Task<ICollection<RefreshTokenModel>?> GetAllTokensAsync(int userId);

    /// <summary>
    /// Removes token with specified id from database
    /// </summary>
    /// <param name="token">value of a token</param>
    /// <exception cref="TokenNotFoundException">
    /// Token with provided value doesn't exist
    /// </exception>
    Task DeleteTokenAsync(string token);

}
