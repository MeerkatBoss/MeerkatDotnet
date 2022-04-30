using MeerkatDotnet.Database;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Services.Database;
using MeerkatDotnet.Services.Database.Exceptions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace MeerkatDotnet.Tests;

[TestFixture]
public class RefreshTokensRepositoryTests
{
    private static readonly UserModel defaultUser = new(
        username: "test",
        passwordHash: "test"
    );

    private readonly DbContextOptions<AppDbContext> _options;

    public RefreshTokensRepositoryTests()
    {
        var builder = WebApplication.CreateBuilder();
        var config = builder.Configuration;
        var connectionString = config.GetConnectionString("DefaultConnection");
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connectionString)
            .Options;
    }

    private static async Task<UserModel> AddDefaultUserAsync(AppDbContext context)
    {
        var user = defaultUser.Clone();
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        return user.Clone();
    }

    private static RefreshTokenModel GetDefaultToken(int userId)
    {
        return new(
            "test",
            userId,
            DateTime.Today.AddDays(7).ToUniversalTime()
        );
    }

    private static RefreshTokenModel GetAlternativeToken(int userId)
    {
        return new("test_alt", userId, DateTime.Today.AddDays(7).ToUniversalTime());
    }

    [Test]
    public async Task TestAddToken()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                var token = GetDefaultToken(user.Id);
                RefreshTokenModel addedToken = await tokensQuery.AddTokenAsync(token);

                Assert.AreEqual((int)default, token.Id);
                Assert.AreNotEqual((int)default, addedToken.Id);
                Assert.AreEqual(token.Value, addedToken.Value);
                Assert.AreEqual(token.UserId, addedToken.UserId);
                Assert.AreEqual(token.ExpirationDate, addedToken.ExpirationDate);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestGetToken()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                var token = GetDefaultToken(user.Id);
                int id = (await tokensQuery.AddTokenAsync(token)).Id;
                RefreshTokenModel? testToken = await tokensQuery.GetTokenAsync(id);

                Assert.NotNull(testToken);
                Assert.AreEqual(token.Value, testToken!.Value);
                Assert.AreEqual(token.UserId, testToken!.UserId);
                Assert.AreEqual(token.ExpirationDate, testToken!.ExpirationDate);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestGetAllTokens()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                RefreshTokenModel token1 = GetDefaultToken(user.Id);
                RefreshTokenModel token2 = GetAlternativeToken(user.Id);
                await tokensQuery.AddTokenAsync(token1);
                await tokensQuery.AddTokenAsync(token2);
                ICollection<RefreshTokenModel>? tokens = await tokensQuery.GetAllTokensAsync(user.Id);

                Assert.NotNull(tokens);
                Assert.AreEqual(2, tokens!.Count);
                Assert.NotNull(tokens.Where(t => t.Value == token1.Value).FirstOrDefault());
                Assert.NotNull(tokens.Where(t => t.Value == token2.Value).FirstOrDefault());
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestDeleteToken()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                RefreshTokenModel token = GetDefaultToken(user.Id);
                int id = (await tokensQuery.AddTokenAsync(token)).Id;
                AsyncTestDelegate deleteUser =
                    async () => await tokensQuery.DeleteTokenAsync(id);

                Assert.DoesNotThrowAsync(deleteUser);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestAddTokenDuplicate()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                var token = GetDefaultToken(user.Id);
                await tokensQuery.AddTokenAsync(token);
                AsyncTestDelegate addToken =
                    async () => await tokensQuery.AddTokenAsync(token);

                Assert.ThrowsAsync<TokenExistsException>(addToken);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestAddExpiredToken()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                var token = new RefreshTokenModel(
                    value: "test",
                    userId: user.Id,
                    expirationDate: DateTime.UtcNow - TimeSpan.FromDays(2)
                );
                AsyncTestDelegate addToken =
                    async () => await tokensQuery.AddTokenAsync(token);

                Assert.ThrowsAsync<TokenExpiredException>(addToken);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public void TestGetTokenInvalidId()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                AsyncTestDelegate getToken =
                    async () => await tokensQuery.GetTokenAsync(-1);

                Assert.ThrowsAsync<ArgumentOutOfRangeException>(getToken);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public void TestDeleteTokenInvalidId()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                AsyncTestDelegate deleteToken =
                    async () => await tokensQuery.DeleteTokenAsync(-1);

                Assert.ThrowsAsync<ArgumentOutOfRangeException>(deleteToken);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }

    }

    [Test]
    public void TestAddTokenInvalidUserId()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                RefreshTokenModel token = GetDefaultToken(-1);

                AsyncTestDelegate addToken =
                    async () => await tokensQuery.AddTokenAsync(token);

                Assert.ThrowsAsync<ArgumentOutOfRangeException>(addToken);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public void TestGetAllTokensInvalidUserId()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                AsyncTestDelegate getAllTokens =
                    async () => await tokensQuery.GetAllTokensAsync(-1);

                Assert.ThrowsAsync<ArgumentOutOfRangeException>(getAllTokens);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestAddUserWrongUserId()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                RefreshTokenModel token = GetDefaultToken(user.Id + 10);
                AsyncTestDelegate addToken =
                    async () => await tokensQuery.AddTokenAsync(token);

                Assert.ThrowsAsync<UserNotFoundException>(addToken);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestGetAllTokensWrongUserId()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                AsyncTestDelegate getAllTokens =
                    async () => await tokensQuery.GetAllTokensAsync(user.Id + 10);

                Assert.ThrowsAsync<UserNotFoundException>(getAllTokens);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestGetUserWrongId()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                RefreshTokenModel token = GetDefaultToken(user.Id);
                RefreshTokenModel addedToken = await tokensQuery.AddTokenAsync(token);
                RefreshTokenModel? getToken = await tokensQuery.GetTokenAsync(addedToken.Id + 10);

                Assert.IsNull(getToken);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }
    }

    [Test]
    public async Task TestDeleteUserWrongId()
    {
        using (var context = new AppDbContext(_options))
        {
            var tokensQuery = new RefreshTokensRepository(context);
            context.Database.BeginTransaction();
            try
            {
                UserModel user = await AddDefaultUserAsync(context);
                RefreshTokenModel token = GetDefaultToken(user.Id);
                RefreshTokenModel addedToken = await tokensQuery.AddTokenAsync(token);
                AsyncTestDelegate deleteToken =
                    async () => await tokensQuery.DeleteTokenAsync(addedToken.Id + 10);

                Assert.ThrowsAsync<TokenNotFoundException>(deleteToken);
            }
            finally
            {
                context.Database.RollbackTransaction();
            }
        }

    }

}