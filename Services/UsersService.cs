using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Diagnostics.CodeAnalysis;
using MeerkatDotnet.Configurations;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Models;
using MeerkatDotnet.Models.Requests;
using MeerkatDotnet.Models.Responses;
using MeerkatDotnet.Repositories;
using MeerkatDotnet.Validators;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using FluentValidation;
using FluentValidation.Results;
using System.Text;

namespace MeerkatDotnet.Services;

public class UsersService : IUsersService
{
    private readonly IRepositoryContext _context;
    private readonly HashingOptions _hashingOptions;
    private readonly JwtOptions _tokenOptions;

    public UsersService(
        IRepositoryContext context,
        HashingOptions hashingOptions,
        JwtOptions tokenOptions)
    {
        _context = context;
        _hashingOptions = hashingOptions;
        _tokenOptions = tokenOptions;
    }

    public async Task<LogInResponse> SignUpUserAsync(UserInputModel inputModel)
    {
        var validator = new UserInputModelValidator();
        ValidationResult res = validator.Validate(inputModel);
        if (!res.IsValid)
            throw new ValidationException(res.Errors);

        UserModel userModel = new(
            username: inputModel.Username,
            passwordHash: GetHash(inputModel.Password),
            email: inputModel.Email,
            phone: CleanPhoneNumber(inputModel.Phone)
        );

        string refreshToken = GenerateRefreshToken();
        DateTime refreshTokenExpires = DateTime.UtcNow
            .AddDays(_tokenOptions.RefreshTokenExpirationDays);
        UserModel user = await _context.Users.AddUserAsync(userModel);
        RefreshTokenModel tokenModel = new(
            value: refreshToken,
            userId: user.Id,
            expirationDate: refreshTokenExpires
        );
        await _context.Tokens.AddTokenAsync(tokenModel);
        return new(
            RefreshToken: refreshToken,
            AccessToken: GetAccessToken(user.Id),
            User: user
        );
    }

    public async Task<LogInResponse> LogInUserAsync(LogInRequest request)
    {
        var validator = new LogInRequestValidator();
        ValidationResult res = validator.Validate(request);
        if(!res.IsValid)
            throw new ValidationException(res.Errors);

        (string username, string password) = request;
        string passwordHash = GetHash(password);
        UserModel? user = await _context.Users.LoginUserAsync(username, passwordHash);
        string refreshToken = GenerateRefreshToken();
        var tokenModel = new RefreshTokenModel(
                value: refreshToken,
                userId: user!.Id,
                expirationDate: DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenExpirationDays));
        await _context.Tokens.AddTokenAsync(tokenModel);
        string accessToken = GetAccessToken(user!.Id);
        return new LogInResponse(refreshToken, accessToken, user);
    }

    public Task<UserOutputModel> GetUserAsync(int id)
    {
        if (id <= 0)
        {
            FluentValidation.Results.ValidationFailure failure = new("Id", "Invalid id provided");
            throw new ValidationException(new[] { failure });
        }

        return _context.Users.GetUserAsync(id)
            .ContinueWith(task => (UserOutputModel)task.Result!);
    }

    public async Task<UserOutputModel> UpdateUserAsync(int id, UserUpdateModel updateModel)
    {
        var validator = new UserUpdateModelValidator();
        ValidationResult res = validator.Validate(updateModel);
        if (id <= 0)
            res.Errors.Add(new("Id", "Invalid id provided"));
        if (!res.IsValid)
            throw new ValidationException(res.Errors);

        UserModel? existingUser = await _context.Users.GetUserAsync(id);
        var updatedUser = new UserModel(
                username: updateModel.Username ?? existingUser!.Username,
                passwordHash: GetHash(updateModel.Password) ?? existingUser!.PasswordHash,
                email: updateModel.Email ?? existingUser!.Email,
                phone: CleanPhoneNumber(updateModel.Phone) ?? existingUser!.Phone
                )
        { Id = id };
        await _context.Users.UpdateUserAsync(updatedUser);
        return updatedUser;
    }

    public async Task DeleteUserAsync(int id)
    {
        if (id <= 0)
        {
            FluentValidation.Results.ValidationFailure failure = new("Id", "Invalid id provided");
            throw new ValidationException(new[] { failure });
        }

        await _context.Users.DeleteUserAsync(id);
    }

    public async Task<RefreshResponse> RefreshTokens(RefreshRequest request)
    {
        (string accessToken, string refreshToken) = request;
        await _context.Tokens.DeleteTokenAsync(refreshToken);
        int userId = GetAccessTokenUser(accessToken);
        refreshToken = GenerateRefreshToken();
        var tokenModel = new RefreshTokenModel(
                value: refreshToken,
                userId: userId,
                expirationDate: DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenExpirationDays));
        await _context.Tokens.AddTokenAsync(tokenModel);
        accessToken = GetAccessToken(userId);
        return new RefreshResponse(refreshToken, accessToken);
    }

    [return: NotNullIfNotNull("password")]
    private string? GetHash(string? password)
    {
        if (password is null) return null;
        byte[] bytes = KeyDerivation.Pbkdf2(
            password: password,
            salt: _hashingOptions.SaltBytes,
            prf: KeyDerivationPrf.HMACSHA256,
            iterationCount: _hashingOptions.IterationCount,
            numBytesRequested: 256
        );
        return Convert.ToBase64String(bytes);
    }

    private string GetAccessToken(int userId)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.Name, userId.ToString()) };
        var signingCredentials = new SigningCredentials(
            _tokenOptions.SecurityKey,
            SecurityAlgorithms.HmacSha256
        );
        var token = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpirationMinutes),
            signingCredentials: signingCredentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        using (var random = RandomNumberGenerator.Create())
        {
            var bytes = new byte[64];
            random.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
    }

    private int GetAccessTokenUser(string accessToken)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _tokenOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _tokenOptions.Audience,
            IssuerSigningKey = _tokenOptions.SecurityKey,
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(
                accessToken,
                parameters,
                out SecurityToken securityToken
            );

            return Convert.ToInt32(principal!.Identity!.Name);
        }
        catch (Exception e)
        {
            FluentValidation.Results.ValidationFailure failure = new("JWT", e.Message);
            throw new ValidationException(new[] { failure });
        }
    }

    private string? CleanPhoneNumber(string? phone)
    {
        if (phone is null)
            return null;

        string charsToErase = " +-()";
        StringBuilder builder = new(phone);
        foreach (var c in charsToErase)
            builder.Replace(c.ToString(), null);

        return builder.ToString();
    }
}
