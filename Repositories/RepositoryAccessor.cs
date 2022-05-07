namespace MeerkatDotnet.Repositories;

public class RepositoryAccessor : IRepositoryAccessor
{
    private readonly IRepositoryContext _context;

    public RepositoryAccessor(IRepositoryContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteQueryAsync<T>(Query<T> query)
    {
        await _context.BeginTransactionAsync();
        try
        {
            var result = await query(_context);
            await _context.CommitTransactionAsync();
            return result;
        }
        catch
        {
            await _context.RollbackTransactionAsync();
            throw;
        }
    }
}