using MarriageCalculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Repositories;

public class DatabaseRepository : IDatabaseRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public DatabaseRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanConnectAsync()
    {
        return await _context.Database.CanConnectAsync();
    }

    public async Task<int> GetTableCountAsync()
    {
        return await _context.Database.SqlQueryRaw<int>(
            "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'")
            .FirstOrDefaultAsync();
    }

    public async Task<string> GetProviderNameAsync()
    {
        return await Task.FromResult(_context.Database.ProviderName ?? "Unknown");
    }
}