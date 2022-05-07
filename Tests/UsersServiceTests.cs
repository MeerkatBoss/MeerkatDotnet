using NUnit.Framework;
using Moq;
using MeerkatDotnet.Repositories;
using MeerkatDotnet.Services;
using MeerkatDotnet.Models;
using MeerkatDotnet.Models.Responses;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Configurations;

namespace MeerkatDotnet.Tests;

[TestFixture]
public class UsersServiceTests
{
    private readonly Mock<IRepositoryContext> _contextMock;
    private readonly Mock<IUsersRepository> _usersMock;
    private readonly Mock<IRefreshTokensRepository> _tokensMock;
    private readonly HashingOptions _hashingOptions = new()
    {
        Salt = "UMUxvp1vvZsLYPHN",
        IterationCount = 1
    };
    private readonly JwtOptions _tokenOptions = new()
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

    [TearDown]
    public void ClearMocks()
    {
        _usersMock.Reset();
        _tokensMock.Reset();
    }

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
        {
            Id = 1
        };
        UserModel addedUser = null!;
        int activeTransactions = 0;
        _usersMock
            .Setup(obj => obj.AddUserAsync(It.IsAny<UserModel>()))
            .Callback<UserModel>(m => addedUser = m)
            .ReturnsAsync(returnedUser);
        _contextMock
            .Setup(x => x.BeginTransactionAsync())
            .Callback(() => activeTransactions++);
        _contextMock
            .Setup(x => x.CommitTransactionAsync())
            .Callback(() => activeTransactions--);
        var usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

        LogInResponse response = await usersService.SignUpUserAsync(userInput);


        Assert.NotNull(response.RefreshToken);
        Assert.NotNull(response.AccessToken);
        Assert.NotNull(response.User);

        Assert.AreEqual(returnedUser.Id, response.User.Id);
        Assert.AreEqual(returnedUser.Username, response.User.Username);

        Assert.NotNull(addedUser);
        Assert.AreEqual(userInput.Username, addedUser.Username);
        Assert.AreNotEqual(userInput.Password, addedUser.PasswordHash);

        Assert.AreEqual(0, activeTransactions);
    }

}