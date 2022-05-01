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
    Task BeginTransactionAsync();

    /// <summary>
    /// Commits database transaction
    /// </summary>
    Task CommitTransactionAsync();

    /// <summary>
    /// Rolls database transaction back
    /// </summary>
    Task RollbackTransactionAsync();
}