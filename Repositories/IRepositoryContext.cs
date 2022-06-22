namespace MeerkatDotnet.Repositories;

/// <summary>
/// Context for managing access to repositories
/// </summary>
public interface IRepositoryContext
{
    /// <summary>
    /// Repository for "users" table
    /// </summary>
    /// <value><c>IUsersRepository</c> instance</value>
    IUsersRepository Users { get; }

    /// <summary>
    /// Repository for "refresh_tokens" table
    /// </summary>
    /// <value><c>IRefreshTokensRepository</c> instance</value>
    IRefreshTokensRepository Tokens { get; }

    /// <summary>
    /// Starts database transaction
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Trying to start new transaction withot finishing previous one
    /// </exception>
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits database transaction
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Trying to commit transaction without starting it
    /// </exception>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls database transaction back
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Trying to rollback transaction without starting it
    /// </exception>
    Task RollbackTransactionAsync();
}
