using MeerkatDotnet.Database;

namespace MeerkatDotnet.Services.Database;

/// <summary>
/// Represents asynchronous query to the database
/// </summary>
/// <param name="context">Database context for query</param>
/// <typeparam name="T">Query return type</typeparam>
/// <returns></returns>
public delegate Task<T> Query<T>(AppDbContext context);

/// <summary>
/// Represents abstract service able to execute queries to the database
/// </summary>
public interface IQueryService
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