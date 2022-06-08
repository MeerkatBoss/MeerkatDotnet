using System.Security.Claims;
using NUnit.Framework;
using Moq;
using MeerkatDotnet.Repositories;
using MeerkatDotnet.Services;
using MeerkatDotnet.Models;
using MeerkatDotnet.Models.Responses;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Configurations;
using MeerkatDotnet.Models.Requests;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace MeerkatDotnet.Tests;

[TestFixture]
public class UsersServiceTests
{
    protected readonly Mock<IRepositoryContext> _contextMock;

    protected readonly Mock<IUsersRepository> _usersMock;

    protected readonly Mock<IRefreshTokensRepository> _tokensMock;

    protected readonly HashingOptions _hashingOptions = new()
    {
        Salt = "UMUxvp1vvZsLYPHN",
        IterationCount = 1
    };

    protected readonly JwtOptions _tokenOptions = new()
    {
        Issuer = "test.net",
        Audience = "test.net",
        Key = "SdbfkVibnwyqJJgpNFbWdmKKYPGZ1Nhl",
        AccessTokenExpirationMinutes = 2,
        RefreshTokenExpirationDays = 3
    };

    public UsersServiceTests()
    {
        _usersMock = new();
        _tokensMock = new();

        _contextMock = new();
        _contextMock.Setup(c => c.Users).Returns(_usersMock.Object);
        _contextMock.Setup(c => c.Tokens).Returns(_tokensMock.Object);

    }

    protected ClaimsPrincipal? ParseAccessToken(string? token)
    {
        if (token is null)
            return null;
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _tokenOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = _tokenOptions.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = _tokenOptions.SecurityKey,
            ClockSkew = TimeSpan.FromSeconds(2)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(
            token,
            parameters,
            out SecurityToken securityToken
        );
        var result = securityToken as JwtSecurityToken;
        if (result is null)
            return null;
        return principal;
    }

    protected void ValidateUserModel(UserModel? user, string username, string password)
    {
        Assert.NotNull(user);
        Assert.AreEqual(username, user!.Username);
        Assert.AreNotEqual(password, user!.PasswordHash);
    }

    protected void ValidateTokenModel(RefreshTokenModel? token)
    {
        var expectedExpiration =
            DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenExpirationDays);

        Assert.NotNull(token);
        Assert.Less(
            expectedExpiration - token!.ExpirationDate,
            TimeSpan.FromSeconds(2)
        );
    }

    protected void ValidateAccessToken(string token, string identityName)
    {
        var claimsPrincipal = ParseAccessToken(token);
        Assert.NotNull(claimsPrincipal);
        Assert.AreEqual(claimsPrincipal!.Identity!.Name, identityName);
    }

    protected void AssertNoneNull(params object?[] args)
    {
        foreach (var arg in args)
            Assert.NotNull(arg);
    }

    [TearDown]
    public void ClearMocks()
    {
        _usersMock.Reset();
        _tokensMock.Reset();
    }

    [TestFixture]
    public class SignUpUserTests : UsersServiceTests
    {
        [Test]
        public async Task TestSignUpUser()
        {
            UserInputModel userInput = new(
                Username: "test",
                Password: "test",
                Email: null,
                Phone: null
            );
            UserModel returnedUser = new(
                username: userInput.Username,
                passwordHash: userInput.Password
            )
            { Id = 1 };
            RefreshTokenModel returnedToken = new("test", 1, DateTime.UtcNow.AddDays(7));
            UserModel? addedUser = null;
            int activeTransactions = 0;
            RefreshTokenModel? addedToken = null;
            _usersMock
                .Setup(obj => obj.AddUserAsync(It.IsAny<UserModel>()))
                .Callback<UserModel>(m => addedUser = m)
                .ReturnsAsync(returnedUser);
            _tokensMock
                .Setup(x => x.AddTokenAsync(It.IsAny<RefreshTokenModel>()))
                .Callback<RefreshTokenModel>(m => addedToken = m)
                .ReturnsAsync(returnedToken);
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => activeTransactions++);
            _contextMock
                .Setup(x => x.CommitTransactionAsync())
                .Callback(() => activeTransactions--);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            LogInResponse response = await usersService.SignUpUserAsync(userInput);

            AssertNoneNull(response.RefreshToken, response.AccessToken, response.User);
            Assert.AreEqual(returnedUser.Id, response.User.Id);
            Assert.AreEqual(userInput.Username, response.User.Username);
            _usersMock.Verify(
                x => x.AddUserAsync(It.Is<UserModel>(
                                    m => m.Username == userInput.Username
                                        && m.PasswordHash != userInput.Password)),
                Times.Once());
            ValidateUserModel(addedUser, userInput.Username, userInput.Password);
            ValidateTokenModel(addedToken);
            ValidateAccessToken(response.AccessToken, returnedUser.Id.ToString());
            Assert.AreEqual(0, activeTransactions);
            _contextMock.Verify(x => x.RollbackTransactionAsync(), Times.Never());
        }

    }

    [TestFixture]
    public class LogInUserTests : UsersServiceTests
    {
        [Test]
        public async Task TestLogInUser()
        {
            LogInRequest request = new("test", "test");
            UserModel returnedUser = new("test", "test") { Id = 1 };
            RefreshTokenModel returnedToken = new("test", 1, DateTime.UtcNow.AddDays(7));
            UserModel? loggedUser = null;
            RefreshTokenModel? addedToken = null;
            int activeTransactions = 0;
            _usersMock
                .Setup(x => x.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((s1, s2) => loggedUser = new UserModel(s1, s2))
                .ReturnsAsync(returnedUser);
            _tokensMock
                .Setup(x => x.AddTokenAsync(It.IsAny<RefreshTokenModel>()))
                .Callback<RefreshTokenModel>(m => addedToken = m)
                .ReturnsAsync(returnedToken);
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => activeTransactions++);
            _contextMock
                .Setup(x => x.CommitTransactionAsync())
                .Callback(() => activeTransactions--);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            LogInResponse response = await usersService.LogInUserAsync(request);

            AssertNoneNull(response.RefreshToken, response.AccessToken, response.User);
            Assert.AreEqual(returnedUser.Id, response.User.Id);
            Assert.AreEqual(request.Login, response.User.Username);
            _usersMock.Verify(
                x => x.LoginUserAsync(
                    It.Is<string>(s => s == request.Login),
                    It.Is<string>(s => s != request.Password)),
                Times.AtLeastOnce());
            _tokensMock.Verify(
                x => x.AddTokenAsync(It.IsAny<RefreshTokenModel>()),
                Times.Once());
            ValidateUserModel(loggedUser, request.Login, request.Password);
            ValidateTokenModel(addedToken);
            ValidateAccessToken(response.AccessToken, returnedUser.Id.ToString());
            Assert.AreEqual(0, activeTransactions);
            _contextMock.Verify(x => x.RollbackTransactionAsync(), Times.Never());
        }

    }

    [TestFixture]
    public class GetUserTests : UsersServiceTests
    {
        [Test]
        public async Task TestGetUser()
        {
            UserModel userModel = new("test", "test") { Id = 1 };
            _usersMock
                .Setup(x => x.GetUserAsync(It.IsAny<int>()))
                .ReturnsAsync(userModel);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            UserOutputModel response = await usersService.GetUserAsync(userModel.Id);

            Assert.AreEqual(userModel.Id, response.Id);
            Assert.AreEqual(userModel.Username, response.Username);
            _usersMock.Verify(
                x => x.GetUserAsync(userModel.Id),
                Times.AtLeastOnce());
            _contextMock.Verify(x => x.BeginTransactionAsync(), Times.Never());
            _contextMock.Verify(x => x.CommitTransactionAsync(), Times.Never());
            _contextMock.Verify(x => x.RollbackTransactionAsync(), Times.Never());
        }
    }

    [TestFixture]
    public class UpdateUserTests : UsersServiceTests
    {
        [Test]
        public async Task TestUpdateUser()
        {
            // Set up
            var updateModel = new UserUpdateModel(
                Username: "test_new",
                Password: null,
                Email: null,
                Phone: null
            );
            var returnedUser = new UserModel("test", "test") { Id = 1 };
            UserModel? updatedUser = null;
            int activeTransactions = 0;
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => activeTransactions++);
            _contextMock
                .Setup(x => x.CommitTransactionAsync())
                .Callback(() => activeTransactions--);

            _usersMock
                .Setup(x => x.GetUserAsync(returnedUser.Id))
                .ReturnsAsync(returnedUser);
            _usersMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserModel>()))
                .Callback<UserModel>(m => updatedUser = m)
                .ReturnsAsync(updatedUser!);

            // Act
            UserOutputModel response = await usersService.UpdateUserAsync(1, updateModel);

            // Assert
            Assert.AreEqual(0, activeTransactions);
            Assert.AreEqual(1, response.Id);
            Assert.AreEqual(updateModel.Username, response.Username);
            Assert.NotNull(updatedUser);
            Assert.NotNull(updatedUser!.PasswordHash);
            Assert.AreEqual(returnedUser.Username, updatedUser!.Username);
            _contextMock
                .Verify(x => x.RollbackTransactionAsync(), Times.Never());
            _contextMock
                .Verify(x => x.Tokens, Times.Never());
        }
    }

    [TestFixture]
    public class DeleteUserTests : UsersServiceTests
    {
        [Test]
        public async Task TestDeleteUser()
        {
            int userId = 1;
            int activeTransactions = 0;
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => activeTransactions++);
            _contextMock
                .Setup(x => x.CommitTransactionAsync())
                .Callback(() => activeTransactions--);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            await usersService.DeleteUserAsync(userId);

            Assert.AreEqual(0, activeTransactions);
            _contextMock
                .Verify(x => x.RollbackTransactionAsync(), Times.Never());
            _usersMock
                .Verify(x => x.DeleteUserAsync(userId), Times.Once());
        }
    }

    [TestFixture]
    public class RefreshTokensTests : UsersServiceTests
    {
        [Test]
        public async Task TestRefreshTokens()
        {
            JwtSecurityToken jwt = new(
                    issuer: _tokenOptions.Issuer,
                    audience: _tokenOptions.Audience,
                    claims: new List<Claim>{new Claim(ClaimTypes.Name, "1")},
                    expires: DateTime.UtcNow.AddMinutes(2),
                    signingCredentials: new(_tokenOptions.SecurityKey, SecurityAlgorithms.HmacSha256)
                    );
            string accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
            string refreshToken = "test";
            int activeTransactions = 0;
            RefreshTokenModel? addedToken = null;
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => activeTransactions++);
            _contextMock
                .Setup(x => x.CommitTransactionAsync())
                .Callback(() => activeTransactions--);
            _tokensMock
                .Setup(x => x.AddTokenAsync(It.IsAny<RefreshTokenModel>()))
                .Callback((RefreshTokenModel token) => addedToken = token)
                .Returns((RefreshTokenModel token) => token);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            RefreshResponse response = await usersService.RefreshTokens(new RefreshRequest(accessToken, refreshToken));

            AssertNoneNull(response, response.AccessToken, response.RefreshToken);
            ValidateAccessToken(response.AccessToken, "1");
            Assert.AreEqual(response.RefreshToken, addedToken!.Value);
            ValidateTokenModel(addedToken);
            Assert.AreEqual(0, activeTransactions);
            _tokensMock
                .Verify(x => x.DeleteTokenAsync(refreshToken), Times.Once());
        }
    }

}
