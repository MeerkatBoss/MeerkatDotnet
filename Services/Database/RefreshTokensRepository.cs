using MeerkatDotnet.Contracts;
using MeerkatDotnet.Database;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Services.Database.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MeerkatDotnet.Services.Database;

public class RefreshTokensRepository : IRefreshTokensRepository
{
    private readonly AppDbContext _context;

    public RefreshTokensRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshTokenModel> AddTokenAsync(RefreshTokenModel token)
    {
        CodeContract.Requires<TokenExistsException>(
            await TokenValueAvailable(token.Value),
            String.Format(
                "Cannot add token: token with value = {0} already exists",
                token.Value
            )
        );
        CodeContract.Requires<TokenExpiredException>(
            token.ExpirationDate > DateTime.UtcNow,
            String.Format(
                "Cannot add token with expiration time = {0}: token already expired",
                token.ExpirationDate
            )
        );
        CodeContract.Requires<ArgumentOutOfRangeException>(
            token.UserId > 0,
            String.Format(
                "Cannot add token: user id={0} is not a positive integer",
                token.UserId
            )
        );
        CodeContract.Requires<UserNotFoundException>(
            await UserExists(token.UserId),
            String.Format(
                "Cannot add token: user with id={0} does not exist",
                token.UserId
            )
        );

        RefreshTokenModel addToken = token.Clone();
        await _context.Tokens.AddAsync(addToken);
        await _context.SaveChangesAsync();
        return addToken;
    }

    public async Task DeleteTokenAsync(int tokenId)
    {
        CodeContract.Requires<ArgumentOutOfRangeException>(
            tokenId > 0,
            String.Format(
                "Cannot delete token with id={0}: id must be a positive integer",
                tokenId
            )
        );
        RefreshTokenModel? token = await GetTokenAsync(tokenId);

        if (token is null)
            throw new TokenNotFoundException(
                String.Format(
                    "Cannot delete token with id={0}: no such token",
                    tokenId
                )
            );

        _context.Tokens.Remove(token!);
        await _context.SaveChangesAsync();
    }

    public async Task<ICollection<RefreshTokenModel>?> GetAllTokensAsync(int userId)
    {
        CodeContract.Requires<ArgumentOutOfRangeException>(
            userId > 0,
            String.Format(
                "Cannot get tokens of user with id={0}: user id must be a positive integer",
                userId
            )
        );
        CodeContract.Requires<UserNotFoundException>(
            await UserExists(userId),
            String.Format(
                "Cannot get tokens of user with id={0}: no such user",
                userId
            )
        );

        return await _context.Tokens.Where(t => t.UserId == userId).ToListAsync();
    }

    public Task<RefreshTokenModel?> GetTokenAsync(int tokenId)
    {
        CodeContract.Requires<ArgumentOutOfRangeException>(
            tokenId > 0,
            String.Format(
                "Cannot get token with id={0}: id must be a positive integer",
                tokenId
            )
        );

        return _context.Tokens.FindAsync(tokenId).AsTask();
    }

    private Task<bool> TokenValueAvailable(string value)
    {
        return _context.Tokens
            .Where(t => t.Value == value)
            .FirstOrDefaultAsync()
            .ContinueWith(t => t.Result is null);
    }

    private Task<bool> UserExists(int userId)
    {
        return _context.Users
            .FindAsync(userId)
            .AsTask()
            .ContinueWith(task => task.Result is not null);
    }
}