using Microsoft.EntityFrameworkCore;

namespace MeerkatDotnet.Repositories;

public class TransactionQueryService : IQueryService
{
    public TransactionQueryService()
    {
    }

    public async Task<T> ExecuteQueryAsync<T>(DbContext context, Query<T> query)
    {
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                var result = await query();
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