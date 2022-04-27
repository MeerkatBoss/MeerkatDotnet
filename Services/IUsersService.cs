using MeerkatDotnet.Models;
using MeerkatDotnet.Models.Requests;
using MeerkatDotnet.Models.Responses;

namespace MeerkatDotnet.Services;

public interface IUsersService
{
    /// <summary>
    /// Signs up a user based on an input model
    /// </summary>
    /// <param name="inputModel">Input model for user</param>
    /// <returns>A pair of tokens and a user model</returns>
    public Task<LogInResponse> SignUpUserAsync(UserInputModel inputModel);

    /// <summary>
    /// Logs in user based on request
    /// </summary>
    /// <param name="request">Request containing login and password</param>
    /// <returns>A pair of tokens and a user model</returns>
    public Task<LogInResponse> LogInUserAsync(LogInRequest request);

    /// <summary>
    /// Retrives information about user with specified id
    /// </summary>
    /// <param name="id">Id of a user</param>
    /// <returns>User model</returns>
    public Task<UserOutputModel> GetUserAsync(int id);

    /// <summary>
    /// Updates user based on update model
    /// </summary>
    /// <param name="updateModel">Update model for user</param>
    /// <returns>Updated user model</returns>
    public Task<UserOutputModel> UpdateUserAsync(int id, UserUpdateModel updateModel);

    /// <summary>
    /// Deletes user with specified id
    /// </summary>
    /// <param name="id">Id of a user</param>
    public Task DeleteUserAsync(int id);

    /// <summary>
    /// Issues a new pair of tokens based on an old one
    /// </summary>
    /// <param name="request">Request containing old pair of tokens</param>
    /// <returns>New pair of tokens</returns>
    public Task<RefreshResponse> RefreshTokens(RefreshRequest request);
}