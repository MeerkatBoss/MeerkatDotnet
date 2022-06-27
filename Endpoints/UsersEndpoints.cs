using MeerkatDotnet.Models;
using MeerkatDotnet.Models.Requests;
using MeerkatDotnet.Models.Responses;
using MeerkatDotnet.Services;
using Microsoft.AspNetCore.Http.Extensions;

namespace MeerkatDotnet.Endpoints;

public static class UsersEndpoints
{
    private static string _apiPrefix = default!;
    private static string _endpointPrefix = default!;
    private static bool _isMapped = false;

    public static WebApplication MapUsersEndpoints(
            this WebApplication app,
            string apiPrefix,
            string endpointPrefix)
    {
        if (_isMapped)
            throw new InvalidOperationException("Endpoint already mapped");
        _isMapped = true;
        _apiPrefix = apiPrefix;
        _endpointPrefix = endpointPrefix;

        app.MapPost($"{apiPrefix}/{endpointPrefix}/sign_up", PostUser)
            .Accepts<UserInputModel>("application/json")
            .Produces<LogInResponse>(statusCode: 201)
            .ProducesValidationProblem()
            .AllowAnonymous();

        app.MapPost($"{apiPrefix}/{endpointPrefix}/log_in", LogInUser)
            .Accepts<LogInRequest>("application/json")
            .Produces<LogInResponse>()
            .Produces(statusCode: 401)
            .ProducesValidationProblem()
            .AllowAnonymous();

        app.MapPut($"{apiPrefix}/{endpointPrefix}/refresh", RefreshTokens)
            .Accepts<RefreshRequest>("application/json")
            .Produces<RefreshResponse>()
            .ProducesValidationProblem()
            .AllowAnonymous();

        app.MapGet($"{apiPrefix}/{endpointPrefix}/", GetUser)
            .Produces<UserOutputModel>()
            .Produces(statusCode: 401)
            .ProducesValidationProblem()
            .ProducesProblem(statusCode: 404)
            .RequireAuthorization();

        app.MapPut($"{apiPrefix}/{endpointPrefix}/", UpdateUser)
            .Produces<UserOutputModel>()
            .Produces(statusCode: 401)
            .ProducesValidationProblem()
            .RequireAuthorization();

        app.MapDelete($"{apiPrefix}/{endpointPrefix}/", DeleteUser)
            .Produces(statusCode: 204)
            .Produces(statusCode: 401)
            .ProducesValidationProblem()
            .RequireAuthorization();

        return app;
    }

    public static async Task<IResult> PostUser(
            UserInputModel request,
            HttpContext context,
            IUsersService usersService)
    {
        LogInResponse response = await usersService.SignUpUserAsync(request);
        string location = context.Request.GetEncodedUrl();
        location = location.Replace("sign_up", String.Empty);
        location += response.User.Id.ToString();
        return Results.Created(location, response);
    }

    public static async Task<IResult> LogInUser(
            LogInRequest request,
            IUsersService usersService)
    {
        LogInResponse response = await usersService.LogInUserAsync(request);
        return Results.Ok(response);
    }

    public static async Task<IResult> RefreshTokens(
            RefreshRequest request,
            IUsersService usersService)
    {
        RefreshResponse response = await usersService.RefreshTokens(request);
        return Results.Ok(response);
    }

    public static async Task<IResult> GetUser(
            HttpContext context,
            IUsersService usersService)
    {
        int id = Convert.ToInt32(context.User!.Identity!.Name);
        UserOutputModel response = await usersService.GetUserAsync(id);
        return Results.Ok(response);
    }

    public static async Task<IResult> UpdateUser(
            UserUpdateModel request,
            HttpContext context,
            IUsersService usersService)
    {
        int id = Convert.ToInt32(context.User!.Identity!.Name);
        UserOutputModel response = await usersService.UpdateUserAsync(id, request);
        return Results.Ok(response);
    }

    public static async Task<IResult> DeleteUser(
            UserDeleteModel user,
            HttpContext context,
            IUsersService usersService)
    {
        int id = Convert.ToInt32(context.User!.Identity!.Name);
        await usersService.DeleteUserAsync(id, user);
        return Results.NoContent();
    }

}
