using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoDbContext context)
    {
        _collection = context.Users;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUserIdAsync(string userId)
    {
        return await _collection.Find(u => u.UserId == userId).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email.ToLower() == email.ToLower()).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<User>> SearchUsersAsync(string query)
    {
        var lowerQuery = query.ToLower();
        return await _collection.Find(u => u.DisplayName.ToLower().Contains(lowerQuery) || u.Email.ToLower().Contains(lowerQuery)).ToListAsync();
    }

    public async Task<User> CreateAsync(User user)
    {
        await _collection.InsertOneAsync(user);
        return user;
    }

    public async Task<User?> UpdateAsync(string id, User user)
    {
        var update = Builders<User>.Update
            .Set(u => u.DisplayName, user.DisplayName)
            .Set(u => u.Email, user.Email);

        var result = await _collection.FindOneAndUpdateAsync(
            u => u.Id == id,
            update,
            new FindOneAndUpdateOptions<User> { ReturnDocument = ReturnDocument.After });

        return result;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var result = await _collection.DeleteOneAsync(u => u.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _collection.CountDocumentsAsync(u => u.Id == id) > 0;
    }
}
