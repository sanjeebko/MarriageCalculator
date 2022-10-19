using MarriageCalculator.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<PlayerModel> Players => Set<PlayerModel>();
    public DbSet<MarriageGameModel> MarriageGame => Set<MarriageGameModel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}