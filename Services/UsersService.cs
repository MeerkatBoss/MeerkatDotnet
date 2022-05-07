using System.Security.Cryptography;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using MeerkatDotnet.Configurations;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Models;
using MeerkatDotnet.Models.Requests;
using MeerkatDotnet.Models.Responses;
using MeerkatDotnet.Repositories;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;

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
        UserModel userModel = new(
            username: inputModel.Username,
            passwordHash: GetHash(inputModel.Password),
            email: inputModel.Email,
            phone: inputModel.Phone
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
        return new(
            RefreshToken: refreshToken,
            AccessToken: GetAccessToken(user.Id),
            User: user
        );
    }

    public Task DeleteUserAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<UserOutputModel> GetUserAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<LogInResponse> LogInUserAsync(LogInRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<RefreshResponse> RefreshTokens(RefreshRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<UserOutputModel> UpdateUserAsync(int id, UserUpdateModel updateModel)
    {
        throw new NotImplementedException();
    }

    private string GetHash(string password)
    {
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
}