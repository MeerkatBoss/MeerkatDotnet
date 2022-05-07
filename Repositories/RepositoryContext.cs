using MeerkatDotnet.Database;
using Microsoft.EntityFrameworkCore.Storage;

namespace MeerkatDotnet.Repositories;

public class RepositoryContext : IRepositoryContext, IDisposable
{
    private readonly AppDbContext _dbContext;

    private IDbContextTransaction? _transaction = null;
    private bool disposedValue;

    public IUsersRepository Users { get; init; }

    public IRefreshTokensRepository Tokens { get; init; }

    public RepositoryContext(
        AppDbContext dbContext,
        IUsersRepository usersRepository,
        IRefreshTokensRepository tokensRepository
    )
    {
        _dbContext = dbContext;
        Users = usersRepository;
        Tokens = tokensRepository;
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction is null)
        {
            _transaction = await _dbContext.Database.BeginTransactionAsync();
        }
        else
            throw new InvalidOperationException(
                "Cannot start transaction: transaction already started"
            );
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        else
            throw new InvalidOperationException(
                "Cannot commit transaction: transaction was not started"
            );
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
        else
            throw new InvalidOperationException(
                "Cannot rollback transaction: transaction was not started"
            );

    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                if (_transaction is not null)
                {
                    _transaction.Dispose();
                    _transaction = null;
                }
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}