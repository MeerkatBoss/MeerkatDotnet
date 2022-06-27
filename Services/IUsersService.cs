using MeerkatDotnet.Models;
using MeerkatDotnet.Models.Requests;
using MeerkatDotnet.Models.Responses;

namespace MeerkatDotnet.Services;

/// <summary>
/// Service for managing users and their authentication
/// </summary>
public interface IUsersService
{
    /// <summary>
    /// Signs up a user based on an input model
    /// </summary>
    /// <param name="inputModel">Input model for user</param>
    /// <returns>A pair of tokens and a user model</returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Provided input model contains invalid user data
    /// </exception>
    Task<LogInResponse> SignUpUserAsync(UserInputModel inputModel);

    /// <summary>
    /// Logs in user based on request
    /// </summary>
    /// <param name="request">Request containing login and password</param>
    /// <returns>A pair of tokens and a user model</returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Login or username contain invalid characters
    /// </exception>
    /// <exception cref="LoginFailedException">
    /// No user with given login and password was found
    /// </exception>
    Task<LogInResponse> LogInUserAsync(LogInRequest request);

    /// <summary>
    /// Retrives information about user with specified id
    /// </summary>
    /// <param name="id">Id of a user</param>
    /// <returns>User model</returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Provided id is not a positive integer
    /// </exception>
    /// <exception cref="EntityNotFoundException">
    /// User with provided id doesn't exist
    /// </exception>
    Task<UserOutputModel> GetUserAsync(int id);

    /// <summary>
    /// Updates user based on update model
    /// </summary>
    /// <param name="updateModel">Update model for user</param>
    /// <returns>Updated user model</returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Id or update model aren't valid
    /// </exception>
    Task<UserOutputModel> UpdateUserAsync(int id, UserUpdateModel updateModel);

    /// <summary>
    /// Deletes user with specified id
    /// </summary>
    /// <param name="id">Id of a user</param>
    /// <exception cref="FluentValidation.ValidationException">
    /// Id is not a positive integer
    /// or user with provided id doesn't exist
    /// </exception>
    Task DeleteUserAsync(int id, UserDeleteModel user);

    /// <summary>
    /// Issues a new pair of tokens based on an old one
    /// </summary>
    /// <param name="request">Request containing old pair of tokens</param>
    /// <returns>New pair of tokens</returns>
    /// <exception cref="FluentValidation.ValidationException">
    /// Provided tokens weren't valid
    /// </exception>
    Task<RefreshResponse> RefreshTokens(RefreshRequest request);

}
