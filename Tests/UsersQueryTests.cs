using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using MeerkatDotnet.Database;
using MeerkatDotnet.Services.Database;
using MeerkatDotnet.Services.Database.Exceptions;
using MeerkatDotnet.Database.Models;

namespace MeerkatDotnet.Tests;

[TestFixture]
public class UsersQueryTests
{
    private static readonly UserModel defaultUser = new(
        username: "test",
        passwordHash: "test"
    );
    private static readonly UserModel alternativeUser = new(
        username: "test_alt",
        passwordHash: "test"
    );
    private static readonly DbContextOptions<AppDbContext> _options;

    static UsersQueryTests()
    {
        var builder = WebApplication.CreateBuilder();
        var config = builder.Configuration;
        var connectionString = config.GetConnectionString("DefaultConnection");
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    [Test]
    public async Task TestAddUser()
    {
        using (var context = new AppDbContext(_options))
        {
            context.Database.BeginTransaction();
            var userModel = defaultUser.Clone();
            var usersQuery = new UsersQuery(context);
            try
            {
                var addedUser = await usersQuery.AddUserAsync(userModel);

                Assert.AreEqual(userModel.Username, addedUser.Username);
                Assert.Greater(userModel.Id, 0);
                Assert.IsNull(addedUser.Email);
                Assert.IsNull(addedUser.Phone);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestAddUserDuplicate()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel1 = defaultUser.Clone();
            var userModel2 = defaultUser.Clone();
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                await usersQuery.AddUserAsync(userModel1);
                AsyncTestDelegate addUser = async () => await usersQuery.AddUserAsync(userModel2);

                Assert.ThrowsAsync<UsernameTakenException>(addUser);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestGetUser()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel = defaultUser.Clone();
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                var addedUser = await usersQuery.AddUserAsync(userModel);
                var user = await usersQuery.GetUserAsync(addedUser.Id);

                Assert.IsNotNull(user);
                Assert.AreEqual(user!.Username, addedUser.Username);
                Assert.AreEqual(user!.PasswordHash, addedUser.PasswordHash);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestGetWrongUser()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel = defaultUser.Clone();
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                var addedUser = await usersQuery.AddUserAsync(userModel);
                var user = await usersQuery.GetUserAsync(addedUser.Id + 10);

                Assert.IsNull(user);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public void TestGetUserInvalidId()
    {
        using (var context = new AppDbContext(_options))
        {
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();

            try
            {
                AsyncTestDelegate getUser =
                    async () => await usersQuery.GetUserAsync(-1);

                Assert.ThrowsAsync<ArgumentOutOfRangeException>(getUser);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestLoginUser()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel = defaultUser.Clone();
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                await usersQuery.AddUserAsync(userModel);
                var user = await usersQuery.LoginUserAsync(
                    userModel.Username, userModel.PasswordHash);

                Assert.NotNull(user);
                Assert.AreEqual(user!.Username, userModel.Username);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestUpdateUser()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel = defaultUser.Clone();
            var updateModel = new UserModel(
                username: userModel.Username,
                passwordHash: userModel.PasswordHash,
                email: "meerkat@meerkatboss.com",
                phone: null
            );
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                var addedUser = await usersQuery.AddUserAsync(userModel);
                updateModel.Id = userModel.Id;
                var result = await usersQuery.UpdateUserAsync(updateModel);

                Assert.AreEqual(result.Username, userModel.Username);
                Assert.AreEqual(result.Email, updateModel.Email);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestUpdateUserExistingName()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel1 = defaultUser.Clone();
            var userModel2 = alternativeUser.Clone();
            var updateModel = new UserModel(
                username: userModel2.Username,
                passwordHash: userModel1.PasswordHash,
                email: userModel1.Email,
                phone: userModel1.Phone
            );
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                var user = await usersQuery.AddUserAsync(userModel1);
                await usersQuery.AddUserAsync(userModel2);
                updateModel.Id = user.Id;
                AsyncTestDelegate updateUser =
                    async () => await usersQuery.UpdateUserAsync(updateModel);

                Assert.ThrowsAsync<UsernameTakenException>(updateUser);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestUpdateWrongUser()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel = defaultUser.Clone();
            var updateModel = new UserModel(
                username: "test_new",
                passwordHash: userModel.PasswordHash,
                email: userModel.Email,
                phone: userModel.Phone
            );
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                var user = await usersQuery.AddUserAsync(userModel);
                updateModel.Id = user.Id + 10;
                AsyncTestDelegate updateUser =
                    async () => await usersQuery.UpdateUserAsync(updateModel);

                Assert.ThrowsAsync<UserNotFoundException>(updateUser);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public void TestUpdateUserInvalidId()
    {
        using (var context = new AppDbContext(_options))
        {
            var updateModel = defaultUser.Clone();
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                updateModel.Id = -1;
                AsyncTestDelegate updateUser =
                    async () => await usersQuery.UpdateUserAsync(updateModel);
                Assert.ThrowsAsync<ArgumentOutOfRangeException>(updateUser);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestAddUserImmutable()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel = defaultUser.Clone();
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                var user = await usersQuery.AddUserAsync(userModel);
                user.Username = "other_name";
                var user2 = await usersQuery.GetUserAsync(user.Id);

                Assert.AreEqual(userModel.Username, user2!.Username);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestGetUserImmutable()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel = defaultUser.Clone();
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                var id = (await usersQuery.AddUserAsync(userModel)).Id;
                var user1 = await usersQuery.GetUserAsync(id);
                user1!.Username = "other_name";
                var user2 = await usersQuery.GetUserAsync(id);

                Assert.AreEqual(userModel.Username, user2!.Username);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestUpdateUserImmutable()
    {
        using (var context = new AppDbContext(_options))
        {
            var userModel = defaultUser.Clone();
            var updateModel = alternativeUser.Clone();
            var usersQuery = new UsersQuery(context);
            context.Database.BeginTransaction();
            try
            {
                var id = (await usersQuery.AddUserAsync(userModel)).Id;
                updateModel.Id = id;
                var updated = await usersQuery.UpdateUserAsync(updateModel);
                updateModel.Username = "other_name";
                updated.Username = "some_other_name";
                var user = await usersQuery.GetUserAsync(id);

                Assert.AreEqual(user!.Username, alternativeUser.Username);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

}
