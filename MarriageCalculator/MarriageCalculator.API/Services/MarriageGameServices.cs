using MarriageCalculator.API.Data;
using Microsoft.EntityFrameworkCore;

namespace MarriageCalculator.API.Services;


public class MarriageGameServices
{

    private readonly AppDbContext _context;

    public MarriageGameServices(AppDbContext context)
    {
        _context = context;
    }

    public async Task SetupDB()
    {
        // Ensure the database is created
        await _context.Database.EnsureCreatedAsync();

        // Check if the new table already exists
        var tableExists = await _context.Database.ExecuteSqlRawAsync(
            "SELECT name FROM sqlite_master WHERE type='table' AND name='NewMarriageGame';");

        if (tableExists == 0)
        {
            // Step 1: Create a new table with the desired schema
            await _context.Database.ExecuteSqlRawAsync(
                "CREATE TABLE NewMarriageGame (Id INTEGER PRIMARY KEY, MarriageGameRoundId INTEGER, DealerPlayerId INTEGER, WinnerId INTEGER, DealerPlayer INTEGER, TotalMaal BOOLEAN, ClosedRound BOOLEAN)");

            // Step 2: Copy data from the old table to the new table
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO NewMarriageGame (MarriageGameRoundId, DealerPlayerId, WinnerId, DealerPlayer, TotalMaal, ClosedRound) SELECT MarriageGameRoundId, DealerPlayerId, WinnerId, DealerPlayer, TotalMaal, ClosedRound FROM MarriageGame");

            // Step 3: Drop the old table
            await _context.Database.ExecuteSqlRawAsync("DROP TABLE MarriageGame");

            // Step 4: Rename the new table to the original table name
            await _context.Database.ExecuteSqlRawAsync("ALTER TABLE NewMarriageGame RENAME TO MarriageGame");
        }

        var tables = await _context.Database.ExecuteSqlRawAsync(
            "SELECT name FROM sqlite_master WHERE type='table';");



    }

}
