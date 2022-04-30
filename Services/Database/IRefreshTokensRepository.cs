using MeerkatDotnet.Database.Models;

namespace MeerkatDotnet.Services.Database;

public interface IRefreshTokensRepository
{

    /// <summary>
    /// Creates refresh token in database based on provided model
    /// </summary>
    /// <param name="token">Token to add to database</param>
    /// <returns>Added token</returns>
    Task<RefreshTokenModel> AddTokenAsync(RefreshTokenModel token);

    /// <summary>
    /// Retrives token with the specified id.
    /// If no such token found, returns null
    /// </summary>
    /// <param name="tokenId">Id of a token</param>
    /// <returns>Retrived token or null</returns>
    Task<RefreshTokenModel?> GetTokenAsync(int tokenId);

    /// <summary>
    /// Retrives all tokens of a user
    /// If user has no tokens, returns null
    /// </summary>
    /// <param name="userId">Id of a user</param>
    /// <returns>ICollection of tokens or null</returns>
    Task<ICollection<RefreshTokenModel>?> GetAllTokensAsync(int userId);

    /// <summary>
    /// Removes token with specified id from database
    /// </summary>
    /// <param name="tokenId">Id of a token</param>
    /// <returns></returns>
    Task DeleteTokenAsync(int tokenId);

}