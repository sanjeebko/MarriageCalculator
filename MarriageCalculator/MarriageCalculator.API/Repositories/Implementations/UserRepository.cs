using MarriageCalculator.API.Data;
using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;
using MarriageCalculator.API.Repositories.Interfaces;

namespace MarriageCalculator.API.Repositories.Implementations;

public class UserRepository : IUserRepository
{
    private readonly MarriageCalculatorDbContext _context;

    public UserRepository(MarriageCalculatorDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.Where(u => u.IsActive).ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // Use SQL-translatable case-insensitive comparison
        var normalizedEmail = email.Trim().ToLower();
        return await _context.Users
            .FirstOrDefaultAsync(u => u.IsActive && u.Email.ToLower() == normalizedEmail);
    }

    public async Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User?> UpdateAsync(Guid id, User user)
    {
        var existingUser = await GetByIdAsync(id);
        if (existingUser == null) return null;

        existingUser.DisplayName = user.DisplayName;
        existingUser.Email = user.Email;
        existingUser.IsEmailVerified = user.IsEmailVerified;
        
        await _context.SaveChangesAsync();
        return existingUser;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false; // Soft delete
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        // Use SQL-translatable case-insensitive comparison
        var normalizedEmail = email.Trim().ToLower();
        return await _context.Users
            .AnyAsync(u => u.IsActive && u.Email.ToLower() == normalizedEmail);
    }

    public async Task<User?> UpdateLastLoginAsync(Guid userId)
    {
        var user = await GetByIdAsync(userId);
        if (user == null) return null;

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return user;
    }
}