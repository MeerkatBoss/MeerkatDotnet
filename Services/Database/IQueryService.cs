using Microsoft.EntityFrameworkCore;

namespace MeerkatDotnet.Services.Database;

/// <summary>
/// Represents asynchronous query to the database
/// </summary>
/// <typeparam name="T">Query return type</typeparam>
/// <returns></returns>
public delegate Task<T> Query<T>();

/// <summary>
/// Represents abstract service able to execute queries to the database
/// </summary>
public interface IQueryService
{
    /// <summary>
    /// Executes given query to the database
    /// </summary>
    /// <param name="context">Database context used by query</param>
    /// <param name="query">
    /// Delegate, representing query to the database
    /// </param>
    /// <typeparam name="T">Return type of a query</typeparam>
    /// <returns></returns>
    Task<T> ExecuteQueryAsync<T>(DbContext context, Query<T> query);
}