namespace MeerkatDotnet.Repositories;

/// <summary>
/// Represents asynchronous query to the database
/// </summary>
/// <typeparam name="T">Query return type</typeparam>
/// <returns></returns>
public delegate Task<T> Query<T>(IRepositoryContext context);

/// <summary>
/// Wrapper for queries against repository
/// </summary>
public interface IRepositoryAccessor
{
    /// <summary>
    /// Executes given query to the database
    /// </summary>
    /// <param name="query">
    /// Delegate, representing query to the database
    /// </param>
    /// <typeparam name="T">Return type of a query</typeparam>
    /// <returns></returns>
    Task<T> ExecuteQueryAsync<T>(Query<T> query);
}