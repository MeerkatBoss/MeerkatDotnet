namespace MeerkatDotnet.Repositories;

public interface IRepositoryContext
{
    IUsersRepository Users { get; }
    IRefreshTokensRepository Tokens { get; }

    Task BeginTransactionAsync();

    Task CommitTransactionAsync();

    Task RollbackTransactionAsync();
}