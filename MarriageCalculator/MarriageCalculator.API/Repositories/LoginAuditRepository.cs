using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Repositories;

public class LoginAuditRepository : ILoginAuditRepository
{
    private readonly IMongoCollection<LoginAudit> _collection;

    public LoginAuditRepository(MongoDbContext context)
    {
        _collection = context.LoginAudits;
    }

    public async Task RecordLoginAsync(LoginAudit audit)
    {
        await _collection.InsertOneAsync(audit);
    }

    public async Task<IEnumerable<LoginAudit>> GetRecentLoginsAsync(int limit = 50)
    {
        var safeLimit = Math.Clamp(limit, 1, 200);
        return await _collection.Find(_ => true)
            .SortByDescending(l => l.TimestampUtc)
            .Limit(safeLimit)
            .ToListAsync();
    }
}
