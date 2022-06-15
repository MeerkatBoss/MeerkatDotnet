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
using FluentValidation;
using MeerkatDotnet.Repositories.Exceptions;

namespace MeerkatDotnet.Tests;

[TestFixture]
[Category("UsersService")]
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

    protected void ValidateUserModel(
            UserModel? user,
            string username,
            string password,
            string? email,
            string? phone)
    {
        Assert.NotNull(user);
        Assert.AreEqual(username, user!.Username);
        Assert.AreNotEqual(password, user!.PasswordHash);
        Assert.AreEqual(email, user!.Email);
        Assert.AreEqual(phone, user!.Phone);
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
        _contextMock.Reset();
        _contextMock.Setup(c => c.Users).Returns(_usersMock.Object);
        _contextMock.Setup(c => c.Tokens).Returns(_tokensMock.Object);
        _usersMock.Reset();
        _tokensMock.Reset();
    }

    protected class TestValues
    {
        public static readonly string[] ValidUsernames = new[]
        {
            "test", "test_test", "test123"
        };

        public static readonly string?[] ValidUpdateUsernames
            = Enumerable.Concat(ValidUsernames, new string?[] { null }).ToArray();

        public static readonly string[] ValidPasswords = new[]
        {
            "testtest", "test1234", "test_test",
            "!testtest", "@testtest", "$testtest",
            "%testtest", "^testtest", "&testtest", "*testtest"
        };

        public static readonly string?[] ValidUpdatePasswords
            = Enumerable.Concat(ValidPasswords, new string?[] { null }).ToArray();

        public static readonly string?[] ValidEmails = new[]
        {
            "test@test.com", "test_test@test.com", "test123@test.com", null
        };

        public static readonly string?[] ValidPhones = new[]
        {
            "12345", "+12345", "1(234)5",
            "12-34-5", "12 34 5", null
        };

        public static readonly string[] InvalidUsernames = new[]
        {
            "test test", "!test", "@test",
            "#test", "$test", "%test",
            "^test", "&test", "*test",
            "(test", ")test", "`test",
            "-test", "=test", "+test",
            "~test", "[test", "]test",
            ":test", "{test", "}test",
            ";test", ",test", ".test",
            "/test", "?test", "|test",
            "\\test", "<test", ">test",
            "\'test", "\"test"
        };

        public static readonly string[] InvalidPasswords = new[]
        {
            "test test", "test",
            "(testtest", ")testtest", "`testtest",
            "~testtest", "[testtest", "]testtest",
            ":testtest", "{testtest", "}testtest",
            ";testtest", ",testtest", ".testtest",
            "/testtest", "|testtest", "\\testtest",
            "<testtest", ">testtest", "\'testtest",
            "\"testtest"
        };

        public static readonly string[] InvalidEmails = new[]
        {
            "test@test", "test.com", "test",
            "test test@test.com", "!test@test.com",
            "#test@test.com", "$test@test.com", "%test@test.com",
            "^test@test.com", "&test@test.com", "*test@test.com",
            "(test@test.com", ")test@test.com", "`test@test.com",
            "-test@test.com", "=test@test.com", "+test@test.com",
            "~test@test.com", "[test@test.com", "]test@test.com",
            ":test@test.com", "{test@test.com", "}test@test.com",
            ";test@test.com", ",test@test.com",
            "/test@test.com", "?test@test.com", "|test@test.com",
            "\\test@test.com", "<test@test.com", ">test@test.com",
            "\'test@test.com", "\"test@test.com"
        };

        public static readonly string[] InvalidPhones = new[]
        {
            " 12345", "!12345", "@12345",
            "#12345", "$12345", "%12345",
            "^12345", "&12345", "*12345",
            "`12345", "=12345", "123+45",
            ")12345", "()12345", "123)(45",
            "123()45", "12345(", "12(3(45))",
            "-1234", "12345-",
            "~12345", "[12345", "]12345",
            ":12345", "{12345", "}12345",
            ";12345", ",12345", ".12345",
            "/12345", "?12345", "|12345",
            "\\12345", "<12345", ">12345",
            "\'12345", "\"12345", "abc"
        };

    }

    [TestFixture]
    [Category("UsersService.SignUpUser")]
    public class SignUpUserTests : UsersServiceTests
    {

        [Test]
        public async Task TestSignUpUser(
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidUsernames))] string username,
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidPasswords))] string password,
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidEmails))] string? email,
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidPhones))] string? phone)
        {
            UserInputModel userInput = new(
                Username: username,
                Password: password,
                Email: email,
                Phone: phone
            );
            RefreshTokenModel returnedToken = new("test", 1, DateTime.UtcNow.AddDays(7));
            UserModel? addedUser = null;
            int activeTransactions = 0;
            RefreshTokenModel? addedToken = null;
            _usersMock
                .Setup(obj => obj.AddUserAsync(It.IsAny<UserModel>()))
                .Callback<UserModel>(m => addedUser = m)
                .ReturnsAsync((UserModel m) =>
                {
                    var res = m.Clone();
                    res.Id = 1;
                    return res;
                });
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
            Assert.AreEqual(1, response.User.Id);
            _usersMock.Verify(
                x => x.AddUserAsync(It.IsAny<UserModel>()),
                Times.Once());
            ValidateUserModel(
                    addedUser,
                    username,
                    password,
                    email,
                    phone is null ? null : "12345");
            Assert.AreEqual(addedUser!.Username, response.User.Username);
            Assert.AreEqual(addedUser!.Email, response.User.Email);
            Assert.AreEqual(addedUser!.Phone, response.User.Phone);
            ValidateTokenModel(addedToken);
            ValidateAccessToken(response.AccessToken, "1");
            Assert.AreEqual(0, activeTransactions);
            _contextMock.Verify(x => x.RollbackTransactionAsync(), Times.Never());
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidUsernames))]
        public void TestSignUpUserInvalidUsername(string username)
        {
            var request = new UserInputModel(
                    Username: username,
                    Password: "test",
                    Phone: null,
                    Email: null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate signUp = async () => await usersService.SignUpUserAsync(request);

            Assert.ThrowsAsync<ValidationException>(signUp);
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidPasswords))]
        public void TestSignUpUserInvalidPassword(string password)
        {
            var request = new UserInputModel(
                    Username: "test",
                    Password: password,
                    Phone: null,
                    Email: null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate signUp = async () => await usersService.SignUpUserAsync(request);

            Assert.ThrowsAsync<ValidationException>(signUp);
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidPasswords))]
        public void TestSignUpUserInvalidEmail(string email)
        {
            var request = new UserInputModel(
                    Username: "test",
                    Password: "test",
                    Phone: null,
                    Email: email);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate signUp = async () => await usersService.SignUpUserAsync(request);

            Assert.ThrowsAsync<ValidationException>(signUp);
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidPasswords))]
        public void TestSignUpUserInvalidPhone(string phone)
        {
            var request = new UserInputModel(
                    Username: "test",
                    Password: "test",
                    Phone: phone,
                    Email: null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate signUp = async () => await usersService.SignUpUserAsync(request);

            Assert.ThrowsAsync<ValidationException>(signUp);
        }

        [Test]
        public void TestSignUpUserDbException()
        {
            var inputModel = new UserInputModel(
                    Username: "test",
                    Password: "testtest",
                    Email: null,
                    Phone: null);
            var sequence = new List<int>();
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => sequence.Add(1));
            _usersMock
                .Setup(x => x.AddUserAsync(It.IsAny<UserModel>()))
                .Callback(() => sequence.Add(2))
                .ThrowsAsync(new Exception());
            _contextMock
                .Setup(x => x.RollbackTransactionAsync())
                .Callback(() => sequence.Add(3));
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate signUp = async () => await usersService.SignUpUserAsync(inputModel);

            Assert.ThrowsAsync<Exception>(signUp);
            var expectedSequence = new List<int>{ 1, 2, 3 };
            Assert.AreEqual(expectedSequence, sequence);
            _contextMock.Verify(x => x.CommitTransactionAsync(), Times.Never());
        }

    }

    [TestFixture]
    [Category("UsersService.LogInUser")]
    public class LogInUserTests : UsersServiceTests
    {
        [Test]
        public async Task TestLogInUser(
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidUsernames))] string login,
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidPasswords))] string password)
        {
            LogInRequest request = new(login, password);
            UserModel returnedUser = new(login, "hashed") { Id = 1 };
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
            ValidateUserModel(loggedUser, request.Login, request.Password, null, null);
            ValidateTokenModel(addedToken);
            ValidateAccessToken(response.AccessToken, returnedUser.Id.ToString());
            Assert.AreEqual(0, activeTransactions);
            _contextMock.Verify(x => x.RollbackTransactionAsync(), Times.Never());
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidUsernames))]
        public void TestLogInUserInvalidLogin(string login)
        {
            var request = new LogInRequest(login, "test");
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate logIn = async () => await usersService.LogInUserAsync(request);

            Assert.ThrowsAsync<ValidationException>(logIn);
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidPasswords))]
        public void TestLogInUserInvalidPassword(string password)
        {
            var request = new LogInRequest("test", password);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate logIn = async () => await usersService.LogInUserAsync(request);

            Assert.ThrowsAsync<ValidationException>(logIn);
        }

        [Test]
        public void TestLogInUserFailed()
        {
            var request = new LogInRequest("test", "testtest");
            _usersMock
                .Setup(x => x.LoginUserAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((UserModel?)null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate logIn = async () => await usersService.LogInUserAsync(request);

            Assert.ThrowsAsync<LoginFailedException>(logIn);
        }

    }

    [TestFixture]
    [Category("UsersService.GetUser")]
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

        [Test]
        public void TestGetUserInvalidId()
        {
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate getUser = async () => await usersService.GetUserAsync(-1);

            Assert.ThrowsAsync<ValidationException>(getUser);
        }

        [Test]
        public void TestGetUserNotFound()
        {
            _usersMock
                .Setup(x => x.GetUserAsync(It.IsAny<int>()))
                .ReturnsAsync((UserModel?)null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate getUser = async () => await usersService.GetUserAsync(1);

            Assert.ThrowsAsync<EntityNotFoundException>(getUser);
        }

    }

    [TestFixture]
    [Category("UsersService.UpdateUser")]
    public class UpdateUserTests : UsersServiceTests
    {
        [Test]
        public async Task TestUpdateUser(
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidUpdateUsernames))] string? username,
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidUpdatePasswords))] string? password,
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidEmails))] string? email,
                [ValueSource(typeof(TestValues), nameof(TestValues.ValidPhones))] string? phone)
        {
            // Set up
            var updateModel = new UserUpdateModel(
                Username: username,
                Password: password,
                Email: email,
                Phone: phone
            );
            var returnedUser = new UserModel(
                    username: "test",
                    passwordHash: "test",
                    email: "test@test.com",
                    phone: "1234567")
            { Id = 1 };
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
            ValidateUserModel(
                    updatedUser,
                    updateModel.Username ?? returnedUser.Username,
                    updateModel.Password ?? "hash",
                    updateModel.Email ?? returnedUser.Email,
                    updateModel.Phone is null ? returnedUser.Phone : "12345");
            if (updateModel.Password is not null)
                Assert.AreNotEqual(updateModel.Password, updatedUser!.PasswordHash);
            _contextMock
                .Verify(x => x.RollbackTransactionAsync(), Times.Never());
            _contextMock
                .Verify(x => x.Tokens, Times.Never());
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidUsernames))]
        public void TestUpdateUserInvalidUsername(string username)
        {
            var updateModel = new UserUpdateModel(
                    Username: username,
                    Password: null,
                    Email: null,
                    Phone: null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate updateUser = async () => await usersService.UpdateUserAsync(1, updateModel);

            Assert.ThrowsAsync<ValidationException>(updateUser);
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidPasswords))]
        public void TestUpdateUserInvalidPassword(string password)
        {
            var updateModel = new UserUpdateModel(
                    Username: null,
                    Password: password,
                    Email: null,
                    Phone: null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate updateUser = async () => await usersService.UpdateUserAsync(1, updateModel);

            Assert.ThrowsAsync<ValidationException>(updateUser);
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidEmails))]
        public void TestUpdateUserInvalidEmail(string email)
        {
            var updateModel = new UserUpdateModel(
                    Username: null,
                    Password: null,
                    Email: email,
                    Phone: null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate updateUser = async () => await usersService.UpdateUserAsync(1, updateModel);

            Assert.ThrowsAsync<ValidationException>(updateUser);
        }

        [TestCaseSource(typeof(TestValues), nameof(TestValues.InvalidPhones))]
        public void TestUpdateUserInvalidPhone(string phone)
        {
            var updateModel = new UserUpdateModel(
                    Username: null,
                    Password: null,
                    Email: null,
                    Phone: phone);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate updateUser = async () => await usersService.UpdateUserAsync(1, updateModel);

            Assert.ThrowsAsync<ValidationException>(updateUser);
        }

        [Test]
        public void TestUpdateUserInvalidId()
        {
            var updateModel = new UserUpdateModel(
                    Username: "test",
                    Password: null,
                    Email: null,
                    Phone: null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate updateUser = async () => await usersService.UpdateUserAsync(-1, updateModel);

            Assert.ThrowsAsync<ValidationException>(updateUser);
        }

        [Test]
        public void TestUpdateUserNotFound()
        {
            var request = new UserUpdateModel("test", "testtest", null, null);
            _usersMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserModel>()))
                .ThrowsAsync(new UserNotFoundException());
            _usersMock
                .Setup(x => x.GetUserAsync(It.IsAny<int>()))
                .ReturnsAsync((UserModel?) null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate updateUser = async () => await usersService.UpdateUserAsync(1, request);

            Assert.ThrowsAsync<ValidationException>(updateUser);
        }

        [Test]
        public void TestUpdateUserDbException()
        {
            var updateModel = new UserUpdateModel(
                    Username: "test",
                    Password: null,
                    Email: null,
                    Phone: null);
            var sequence = new List<int>();
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => sequence.Add(1));
            _usersMock
                .Setup(x => x.UpdateUserAsync(It.IsAny<UserModel>()))
                .Callback(() => sequence.Add(2))
                .ThrowsAsync(new Exception());
            _contextMock
                .Setup(x => x.RollbackTransactionAsync())
                .Callback(() => sequence.Add(3));
            _usersMock
                .Setup(x => x.GetUserAsync(1))
                .ReturnsAsync(new UserModel("test1", "testtest"){ Id = 1 });
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate updateUser = async () => await usersService.UpdateUserAsync(1, updateModel);

            Assert.ThrowsAsync<Exception>(updateUser);
            var expectedSequence = new List<int>{ 1, 2, 3 };
            Assert.AreEqual(expectedSequence, sequence);
            _contextMock.Verify(x => x.CommitTransactionAsync(), Times.Never());
        }

    }

    [TestFixture]
    [Category("UsersService.DeleteUser")]
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

        [Test]
        public void TestDeleteUserInvalidId()
        {
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate deleteUser = async () => await usersService.DeleteUserAsync(-1);

            Assert.ThrowsAsync<ValidationException>(deleteUser);
        }

        [Test]
        public void TestDeleteUserNotFound()
        {
            _usersMock
                .Setup(x => x.DeleteUserAsync(It.IsAny<int>()))
                .ThrowsAsync(new UserNotFoundException());
            _usersMock
                .Setup(x => x.GetUserAsync(It.IsAny<int>()))
                .ReturnsAsync((UserModel?) null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate deleteUser = async () => await usersService.DeleteUserAsync(1);

            Assert.ThrowsAsync<ValidationException>(deleteUser);
        }

        [Test]
        public void TestDeleteUserDbException()
        {
            var sequence = new List<int>();
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => sequence.Add(1));
            _usersMock
                .Setup(x => x.DeleteUserAsync(It.IsAny<int>()))
                .Callback(() => sequence.Add(2))
                .ThrowsAsync(new Exception());
            _contextMock
                .Setup(x => x.RollbackTransactionAsync())
                .Callback(() => sequence.Add(3));
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate deleteUser = async () => await usersService.DeleteUserAsync(1);

            Assert.ThrowsAsync<Exception>(deleteUser);
            var expectedSequence = new List<int>{ 1, 2, 3 };
            Assert.AreEqual(expectedSequence, sequence);
            _contextMock.Verify(x => x.CommitTransactionAsync(), Times.Never());

        }

    }

    [TestFixture]
    [Category("UsersService.RefreshTokens")]
    public class RefreshTokensTests : UsersServiceTests
    {

        private string GetAccessToken(int userId)
        {
            JwtSecurityToken jwt = new(
                    issuer: _tokenOptions.Issuer,
                    audience: _tokenOptions.Audience,
                    claims: new List<Claim> { new Claim(ClaimTypes.Name, userId.ToString()) },
                    expires: DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)),
                    signingCredentials: new(_tokenOptions.SecurityKey, SecurityAlgorithms.HmacSha256)
                    );
            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        [Test]
        public async Task TestRefreshTokens()
        {
            string accessToken = GetAccessToken(1);
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
                .Callback<RefreshTokenModel>(token => addedToken = token)
                .ReturnsAsync((RefreshTokenModel token) => token);
            _tokensMock
                .Setup(x => x.GetTokenAsync("test"))
                .ReturnsAsync(new RefreshTokenModel("test", 1, DateTime.UtcNow.AddDays(2)));
            _usersMock
                .Setup(x => x.GetUserAsync(1))
                .ReturnsAsync(new UserModel("test", "testtest"){Id = 1});
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

        [Test]
        public void TestRefreshTokensInvalidAccessToken()
        {
            var request = new RefreshRequest("_invalid_", "test");
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate refreshTokens = async () => await usersService.RefreshTokens(request);

            Assert.ThrowsAsync<ValidationException>(refreshTokens);
        }

        [Test]
        public void TestRefreshTokensRefreshTokenNotFound()
        {
            var request = new RefreshRequest(GetAccessToken(1), "test");
            _tokensMock
                .Setup(x => x.GetTokenAsync(It.IsAny<string>()))
                .ReturnsAsync((RefreshTokenModel?) null);
            _tokensMock
                .Setup(x => x.DeleteTokenAsync(It.IsAny<string>()))
                .ThrowsAsync(new TokenNotFoundException());
            _usersMock
                .Setup(x => x.GetUserAsync(1))
                .ReturnsAsync(new UserModel("test", "testtest"){Id = 1});
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate refreshTokens = async () => await usersService.RefreshTokens(request);

            Assert.ThrowsAsync<ValidationException>(refreshTokens);
        }

        [Test]
        public void TestRefreshTokensUserNotFound()
        {
            var request = new RefreshRequest(GetAccessToken(1), "test");
            _usersMock
                .Setup(x => x.GetUserAsync(It.IsAny<int>()))
                .ReturnsAsync((UserModel?) null);
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate refreshTokens = async () => await usersService.RefreshTokens(request);

            Assert.ThrowsAsync<ValidationException>(refreshTokens);
        }

        [Test]
        public void TestRefreshTokensRefreshTokenExpired()
        {
            var request = new RefreshRequest(GetAccessToken(1), "test");
            var refreshToken = new RefreshTokenModel(
                    value: "test",
                    userId: 1,
                    expirationDate: DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(1)));
            _tokensMock
                .Setup(x => x.GetTokenAsync("test"))
                .ReturnsAsync(refreshToken);
            _usersMock
                .Setup(x => x.GetUserAsync(1))
                .ReturnsAsync(new UserModel("test", "testtest"){Id = 1});
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate refreshTokens = async () => await usersService.RefreshTokens(request);

            Assert.ThrowsAsync<ValidationException>(refreshTokens);
            _tokensMock.Verify(x => x.DeleteTokenAsync("test"), Times.Once());
        }

        [Test]
        public void TestRefreshTokensDbException()
        {
            var request = new RefreshRequest(GetAccessToken(1), "test");
            var sequence = new List<int>();
            _contextMock
                .Setup(x => x.BeginTransactionAsync())
                .Callback(() => sequence.Add(1));
            _tokensMock
                .Setup(x => x.DeleteTokenAsync(It.IsAny<string>()))
                .Callback(() => sequence.Add(2))
                .ThrowsAsync(new Exception());
            _contextMock
                .Setup(x => x.RollbackTransactionAsync())
                .Callback(() => sequence.Add(3));
            _tokensMock
                .Setup(x => x.GetTokenAsync("test"))
                .ReturnsAsync(new RefreshTokenModel("test", 1, DateTime.UtcNow.AddDays(2)));
            _usersMock
                .Setup(x => x.GetUserAsync(1))
                .ReturnsAsync(new UserModel("test", "testtest"){Id = 1});
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate refreshTokens = async () => await usersService.RefreshTokens(request);

            Assert.ThrowsAsync<Exception>(refreshTokens);
            var expectedSequence = new List<int>{ 1, 2, 3 };
            Assert.AreEqual(expectedSequence, sequence);
            _contextMock.Verify(x => x.CommitTransactionAsync(), Times.Never());
        }

        [Test]
        public void TestRefreshTokensDifferentUsers()
        {
            var userTokens1 = new List<RefreshTokenModel>
            {
                new("test1", 1, DateTime.UtcNow.AddDays(1)),
                new("test2", 1, DateTime.UtcNow.AddDays(2))
            };
            var userTokens2 = new List<RefreshTokenModel>
            {
                new("test3", 2, DateTime.UtcNow.AddDays(3)),
                new("test4", 2, DateTime.UtcNow.AddDays(1))
            };
            var request = new RefreshRequest(GetAccessToken(1), userTokens2[0].Value);
            _usersMock
                .Setup(x => x.GetUserAsync(It.IsAny<int>()))
                .ReturnsAsync((int id) => new UserModel($"user{id}", "testtest"){ Id = id });
            _tokensMock
                .Setup(x => x.GetAllTokensAsync(1))
                .ReturnsAsync(userTokens1);
            _tokensMock
                .Setup(x => x.GetAllTokensAsync(2))
                .ReturnsAsync(userTokens2);
            _tokensMock
                .Setup(x => x.GetTokenAsync(It.IsIn<string>(userTokens1.Select(x => x.Value))))
                .ReturnsAsync((string val) => new RefreshTokenModel(val, 1, DateTime.UtcNow.AddDays(1)));
            _tokensMock
                .Setup(x => x.GetTokenAsync(It.IsIn<string>(userTokens2.Select(x => x.Value))))
                .ReturnsAsync((string val) => new RefreshTokenModel(val, 2, DateTime.UtcNow.AddDays(1)));
            IUsersService usersService = new UsersService(_contextMock.Object, _hashingOptions, _tokenOptions);

            AsyncTestDelegate refreshTokens = async () => await usersService.RefreshTokens(request);

            Assert.ThrowsAsync<ValidationException>(refreshTokens);
            foreach (var token in userTokens1.Concat(userTokens2))
            {
                _tokensMock.Verify(x => x.DeleteTokenAsync(token.Value), Times.Once());
            }

        }

    }

}
