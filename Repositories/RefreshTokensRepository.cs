using MeerkatDotnet.Database;
using MeerkatDotnet.Database.Models;
using MeerkatDotnet.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace MeerkatDotnet.Repositories;

public class RefreshTokensRepository : IRefreshTokensRepository
{
    private readonly AppDbContext _context;

    public RefreshTokensRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshTokenModel> AddTokenAsync(RefreshTokenModel token)
    {
        if (!await TokenValueAvailable(token.Value))
        {
            throw new TokenExistsException(
                    $"Cannot add token: token with value={token.Value} already exists");
        }
        if (token.IsExpired)
        {
            throw new TokenExpiredException(
                    $"Cannot add token: token already expired");
        }
        if (token.UserId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                    $"Cannot add token: user id={token.UserId} is not a positive integer");
        }
        if (!await UserExists(token.UserId))
        {
            throw new UserNotFoundException(
                    $"Cannot add token: user with id={token.UserId} does not exist");
        }

        RefreshTokenModel addToken = token.Clone();
        await _context.Tokens.AddAsync(addToken);
        await _context.SaveChangesAsync();
        return addToken;
    }

    public async Task DeleteTokenAsync(string tokenValue)
    {
        RefreshTokenModel? token = await GetTokenAsync(tokenValue);

        if (token is null)
            throw new TokenNotFoundException(
                $"Cannot delete token: no token with value={tokenValue} found"
            );

        _context.Tokens.Remove(token!);
        await _context.SaveChangesAsync();
    }

    public async Task<ICollection<RefreshTokenModel>?> GetAllTokensAsync(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(
                    $"Cannot get tokens: user id={userId} is not a positive integer");
        }
        if (!await UserExists(userId))
        {
            throw new UserNotFoundException(
                    $"Cannor get tokens: user with id={userId} does not exist");
        }

        return await _context.Tokens.Where(t => t.UserId == userId).ToListAsync();
    }

    public Task<RefreshTokenModel?> GetTokenAsync(string tokenValue)
    {
        return _context.Tokens.Where(t => t.Value == tokenValue).FirstOrDefaultAsync();
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
