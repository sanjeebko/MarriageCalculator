using MarriageCalculator.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Player> Players => Set<Player>();
    public DbSet<MarriageGame> MarriageGame => Set<MarriageGame>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}