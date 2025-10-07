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
    
    // User Authentication
    public DbSet<User> Users => Set<User>();
    public DbSet<UserEmailVerification> UserEmailVerifications => Set<UserEmailVerification>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Player entity
        modelBuilder.Entity<Player>(entity =>
        {
            entity.ToTable("Player");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasColumnName("Name").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("Email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.Deleted).HasColumnName("Deleted").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.CreatedByUserId).HasColumnName("CreatedByUserId");
            
            // Handle the Selected column if it exists in database but is ignored in the model
            // This prevents SQL errors when Selected column exists but model doesn't have it
            entity.Property(e => e.Selected).HasColumnName("Selected").HasDefaultValue(false);
            
            // Index for creator user id (GUID)
            entity.HasIndex(e => e.CreatedByUserId);

            // Configure optional relationship from Player.CreatedByUserId (GUID) -> User.Id (GUID)
            entity.HasOne(e => e.CreatedByUser)
                  .WithMany(u => u.CreatedPlayers)
                  .HasForeignKey(e => e.CreatedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);
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
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            
            // Foreign key to User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.GameSettings)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MarriageGameSet entity
        modelBuilder.Entity<MarriageGameSet>(entity =>
        {
            entity.ToTable("MarriageGameSet");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Created).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LastPlayed).HasDefaultValueSql("GETUTCDATE()" );
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
            // Composite primary key
            entity.HasKey(e => new { e.MarriageGameSetId, e.PlayerId });

            // Relationships with proper navigation mapping
            entity.HasOne(e => e.MarriageGameSet)
                  .WithMany()
                  .HasForeignKey(e => e.MarriageGameSetId)
                  .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Player)
                  .WithMany()
                  .HasForeignKey(e => e.PlayerId)
                  .OnDelete(DeleteBehavior.Cascade);
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

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(e => e.DisplayName).HasColumnName("DisplayName").IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).HasColumnName("Email").IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasColumnName("PasswordHash").IsRequired();
            entity.Property(e => e.Salt).HasColumnName("Salt").IsRequired();
            entity.Property(e => e.IsEmailVerified).HasColumnName("IsEmailVerified");
            entity.Property(e => e.IsActive).HasColumnName("IsActive");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LastLoginAt).HasColumnName("LastLoginAt");
            
            // Unique email constraint
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configure UserEmailVerification entity
        modelBuilder.Entity<UserEmailVerification>(entity =>
        {
            entity.ToTable("UserEmailVerification");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.VerificationCode).HasColumnName("VerificationCode").IsRequired().HasMaxLength(5);
            entity.Property(e => e.ExpiresAt).HasColumnName("ExpiresAt");
            entity.Property(e => e.IsUsed).HasColumnName("IsUsed");
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.UsedAt).HasColumnName("UsedAt");
            
            // Foreign key to User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.EmailVerifications)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Index for faster lookups
            entity.HasIndex(e => new { e.UserId, e.VerificationCode, e.IsUsed });
        });

        // Configure RefreshToken entity
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshToken");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("UserId");
            entity.Property(e => e.Token).HasColumnName("Token").IsRequired().HasMaxLength(256);
            entity.Property(e => e.ExpiresAt).HasColumnName("ExpiresAt");
            entity.Property(e => e.IsActive).HasColumnName("IsActive");
            entity.Property(e => e.RevokedAt).HasColumnName("RevokedAt");
            entity.Property(e => e.ReplacedByToken).HasColumnName("ReplacedByToken").HasMaxLength(256);
            entity.Property(e => e.RevokedReason).HasColumnName("RevokedReason").HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasColumnName("CreatedAt").HasDefaultValueSql("GETUTCDATE()");
            
            // Foreign key to User
            entity.HasOne(e => e.User)
                  .WithMany(u => u.RefreshTokens)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            // Index for faster token lookups
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => new { e.UserId, e.IsActive });
        });
    }
}