using System.Transactions;
using MeerkatDotnet.Database;

namespace MeerkatDotnet.Services.Database;

public class TransactionQueryService : IQueryService
{
    private AppDbContext _database;

    public TransactionQueryService(AppDbContext database)
    {
        _database = database;
    }

    public async Task<T> ExecuteQueryAsync<T>(Query<T> query)
    {
        using (var transaction = await _database.Database.BeginTransactionAsync())
        {
            try
            {
                var result = await query(_database);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}