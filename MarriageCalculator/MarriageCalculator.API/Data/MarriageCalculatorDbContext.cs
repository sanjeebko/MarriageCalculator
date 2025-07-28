using MarriageCalculator.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Data;

public class MarriageCalculatorDbContext : DbContext
{
    public MarriageCalculatorDbContext(DbContextOptions<MarriageCalculatorDbContext> options) : base(options)
    {
    }

    // DbSets for all models
    public DbSet<Player> Players => Set<Player>();
    public DbSet<GameSettings> GameSettings => Set<GameSettings>();
    public DbSet<MarriageGameSet> MarriageGameSets => Set<MarriageGameSet>();
    public DbSet<MarriageGameSetPlayer> MarriageGameSetPlayers => Set<MarriageGameSetPlayer>();
    public DbSet<MarriageGameRound> MarriageGameRounds => Set<MarriageGameRound>();
    public DbSet<MarriageGame> MarriageGames => Set<MarriageGame>();
    public DbSet<MarriageGameScore> MarriageGameScores => Set<MarriageGameScore>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Player entity
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Player");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Ignore(e => e.Selected); // Ignore ObservableProperty
            entity.HasIndex(e => new { e.Name, e.Email }).IsUnique();
        });

        // Configure GameSettings entity
        modelBuilder.Entity<GameSettings>(entity =>
        {
            entity.ToTable("GameSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PointRate).HasPrecision(18, 2);
            entity.Property(e => e.Currency).HasConversion<int>();
            entity.Property(e => e.FoulPointBonus).HasConversion<int>();
        });

        // Configure MarriageGameSet entity
        modelBuilder.Entity<MarriageGameSet>(entity =>
        {
            entity.ToTable("MarriageGameSet");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Created).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LastPlayed).HasDefaultValueSql("GETUTCDATE()");
            entity.Ignore(e => e.GameSettings); // Ignore navigation property
            entity.Ignore(e => e.GameSetPlayers); // Ignore navigation property
            entity.Ignore(e => e.Rounds); // Ignore navigation property
            
            // Foreign key to GameSettings
            entity.HasOne<GameSettings>()
                  .WithMany()
                  .HasForeignKey(e => e.GameSettingsId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MarriageGameSetPlayer (Junction table)
        modelBuilder.Entity<MarriageGameSetPlayer>(entity =>
        {
            entity.ToTable("MarriageGameSetPlayer");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Ignore(e => e.Player); // Ignore navigation property
            
            // Foreign key to MarriageGameSet
            entity.HasOne<MarriageGameSet>()
                  .WithMany()
                  .HasForeignKey(e => e.MarriageGameSetId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Player
            entity.HasOne<Player>()
                  .WithMany()
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Unique constraint to prevent duplicate player-gameset combinations
            entity.HasIndex(e => new { e.MarriageGameSetId, e.PlayerId }).IsUnique();
        });

        // Configure MarriageGameRound entity
        modelBuilder.Entity<MarriageGameRound>(entity =>
        {
            entity.ToTable("MarriageGameRound");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Ignore(e => e.MarriageGames); // Ignore navigation property
            entity.Ignore(e => e.TotalScore); // Ignore navigation property
            
            // Foreign key to MarriageGameSet
            entity.HasOne<MarriageGameSet>()
                  .WithMany()
                  .HasForeignKey(e => e.MarriageGameSetId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MarriageGame entity
        modelBuilder.Entity<MarriageGame>(entity =>
        {
            entity.ToTable("MarriageGame");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedTime).HasDefaultValueSql("GETUTCDATE()");
            entity.Ignore(e => e.MarriageGameScores); // Ignore navigation property
            
            // Foreign key to MarriageGameRound
            entity.HasOne<MarriageGameRound>()
                  .WithMany()
                  .HasForeignKey(e => e.MarriageGameRoundId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Player (Winner)
            entity.HasOne<Player>()
                  .WithMany()
                  .HasForeignKey(e => e.WinnerId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            // Foreign key to Player (Dealer)
            entity.HasOne<Player>()
                  .WithMany()
                  .HasForeignKey(e => e.DealerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure MarriageGameScore entity
        modelBuilder.Entity<MarriageGameScore>(entity =>
        {
            entity.ToTable("MarriageGameScore");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.MoneyWon).HasPrecision(18, 2);
            entity.Ignore(e => e.MarriageGame); // Ignore navigation property
            
            // Map the backing fields for ObservableProperty
            entity.Property(e => e.Seen).HasColumnName("Seen");
            entity.Property(e => e.Playing).HasColumnName("Playing");
            entity.Property(e => e.Maal).HasColumnName("Maal");
            entity.Property(e => e.BonusPoint).HasColumnName("BonusPoint");
            entity.Property(e => e.Duply).HasColumnName("Duply");
            entity.Property(e => e.Winner).HasColumnName("Winner");
            entity.Property(e => e.Score).HasColumnName("Score");
            entity.Property(e => e.MoneyWon).HasColumnName("MoneyWon").HasPrecision(18, 2);
            entity.Property(e => e.Deal).HasColumnName("Deal");
            entity.Property(e => e.Position).HasColumnName("Position");
            
            // Foreign key to MarriageGame
            entity.HasOne<MarriageGame>()
                  .WithMany()
                  .HasForeignKey(e => e.MarriageGameId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Foreign key to Player
            entity.HasOne<Player>()
                  .WithMany()
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            // Unique constraint to prevent duplicate player-game score combinations
            entity.HasIndex(e => new { e.MarriageGameId, e.PlayerId }).IsUnique();
        });
    }
}