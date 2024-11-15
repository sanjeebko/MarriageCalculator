using SQLite;

namespace MarriageCalculator.Services;

public partial class SqLiteDbService : IDbService
{
    public List<Player> Players { get; } = new();
    public List<MarriageGame> MarriageGames { get; } = new();
    public List<MarriageGameRound> MarriageGameRounds { get; } = new();
    public List<MarriageGameScore> MarriageGamePlayers { get; } = new();

    public List<MarriageGameSet> MarriageGameSets { get; } = new();

    private SQLiteAsyncConnection? _dbConnection;

    public SqLiteDbService()
    {
        SetupDB();

    }

    public async void SetupDB()
    {
        try
        {
            if (_dbConnection is null)
            {
                string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MarriageCalculator.db3");
                _dbConnection = new SQLiteAsyncConnection(dbPath);

                //await _dbConnection.DeleteAllAsync<MarriageGame>();
                //await _dbConnection.DeleteAllAsync<MarriageGameRound>();
                //await _dbConnection.DeleteAllAsync<GameSettings>();
                //await _dbConnection.DropTableAsync<MarriageGame>();
                //await _dbConnection.DropTableAsync<MarriageGameRound>();
                //await _dbConnection.DropTableAsync<GameSettings>();
                // await _dbConnection.DeleteAllAsync<Player>();
                // await _dbConnection.DropTableAsync<Player>(); 


                await _dbConnection.CreateTableAsync<Player>();
                await _dbConnection.CreateTableAsync<MarriageGame>();
                await _dbConnection.CreateTableAsync<MarriageGameRound>();
                await _dbConnection.CreateTableAsync<GameSettings>();
                await _dbConnection.CreateTableAsync<MarriageGameScore>();
                await _dbConnection.CreateTableAsync<MarriageGameSet>();
                await _dbConnection.CreateTableAsync<MarriageGameSetPlayer>();

                return;
            }
            //check if all the tables are available, if not create them 
            await CreateTablesForModelsWithTableAttribute();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    private async Task CreateTablesForModelsWithTableAttribute()
    {
        if (_dbConnection is null)
            return;

        var modelTypes = typeof(MarriageCalculator.Core.Models.Player).Assembly.GetTypes()
            .Where(t => t.Namespace == "MarriageCalculator.Core.Models" && t.GetCustomAttributes(typeof(TableAttribute), true).Length > 0);

        foreach (var modelType in modelTypes)
        {
            var tableName = ((TableAttribute)modelType.GetCustomAttributes(typeof(TableAttribute), true).First()).Name;
            var tableExists = await _dbConnection.ExecuteScalarAsync<int>($"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tableName}'") > 0;

            if (!tableExists)
            {
                await _dbConnection.CreateTableAsync(modelType);
            }
        }
    }

    #region Player
    public Task<List<Player>> GetPlayersAsync()
    {
        var playerList = _dbConnection?.Table<Player>().Where(a => !a.Deleted).ToListAsync();

        return playerList ?? Task.Run(static () => { return new List<Player>(); });
    }

    public async Task<int> AddPlayerAsync(Player model)
    {
        if (_dbConnection == null)
        {
            return 0;
        }
        var allPlayers = await GetPlayersAsync();
        var oldPlayer = allPlayers?.FirstOrDefault(a => a.Name.ToLower() == model.Name.ToLower());

        if (oldPlayer is not null)
        {
            model.Id = oldPlayer.Id;
            if (oldPlayer.Deleted)
            {
                model.Deleted = false;
                await _dbConnection.UpdateAsync(model);
            }

            return model.Id;
        }

        await _dbConnection.InsertAsync(model);
        return model.Id;
    }

    public async Task<int> DeletePlayerAsync(Player model)
    {
        if (_dbConnection == null)
        {
            return 0;
        }

        model.Deleted = true;
        return await _dbConnection.UpdateAsync(model);
    }

    #endregion Player

    #region MarriageGame
    public async Task<List<MarriageGame>> GetMarriageGamesAsync()
    {
        var marriageGameTable = _dbConnection?.Table<MarriageGame>();
        if (marriageGameTable == null)
        {
            return [];
        }

        var marriageGameList = await marriageGameTable.ToListAsync();

        return marriageGameList ?? [];
    }

    public Task<int> AddMarriageGameAsync(MarriageGame model)
    {
        return _dbConnection?.InsertAsync(model) ?? Task.Run(() => { return 0; });
    }

    public Task<int> DeleteMarriageGameAsync(MarriageGame model)
    {
        return _dbConnection?.DeleteAsync(model) ?? Task.Run(() => { return 0; });
    }

    public Task<int> UpdateMarriageGameAsync(MarriageGame model)
    {
        return _dbConnection?.UpdateAsync(model) ?? Task.Run(() => { return 0; });
    }

    #endregion MarriageGame

    #region MarriageGameSet

    //create function to get all marriage game sets

    public async Task<List<MarriageGameSet>> GetMarriageGameSetsAsync()
    {
        var marriageGameSetTable = _dbConnection?.Table<MarriageGameSet>();
        if (marriageGameSetTable == null)
        {
            return [];
        }

        var marriageGameSetList = await marriageGameSetTable.ToListAsync();

        return marriageGameSetList ?? [];
    }
    public async Task<MarriageGameSet?> GetLatestMarriageGameSetAsync()
    {
        var marriageGameSetTable = _dbConnection?.Table<MarriageGameSet>();
        if (marriageGameSetTable == null)
        {
            return null;
        }

        var latestMarriageGameSet = await marriageGameSetTable
            .OrderByDescending(m => m.LastPlayed)
            .FirstOrDefaultAsync();

        return latestMarriageGameSet;
    }
    public async Task<MarriageGameSet?> AddNewMarriageGameSetAsync(string? name)
    {
        if (_dbConnection == null)
        {
            return null;
        }
        var marriageGameSet = new MarriageGameSet();
        if (!string.IsNullOrEmpty(name))
        {
            marriageGameSet.Name = name;
        }

        marriageGameSet.IsActive = true;
        marriageGameSet.Created = DateTime.UtcNow;

        await _dbConnection.InsertAsync(marriageGameSet);
        return marriageGameSet;
    }

    public async Task<int> UpdateMarriageGameSetAsync(MarriageGameSet model)
    {
        if (_dbConnection == null)
            return 0;
        return await _dbConnection.UpdateAsync(model);
    }

    public async Task CloseLastMarriageGameSet()
    {
        var MarriageGameSet = await GetLatestMarriageGameSetAsync();
        if (MarriageGameSet is not null)
        {
            MarriageGameSet.IsActive = false;
            await UpdateMarriageGameSetAsync(MarriageGameSet);
        }
    }
    public async Task CloseMarriageGameSet(MarriageGameSet model) {

        if (_dbConnection == null)
            return;
        model.IsActive = false;
        await _dbConnection.UpdateAsync(model);
    }

    #endregion MarriageGameSet

    #region MarriageGameSetPlayer

    public async Task<MarriageGameSetPlayer> AddMarriageGameSetPlayerAsync(MarriageGameSetPlayer model)
    {
        var gameSetPlayer = _dbConnection?.Table<MarriageGameSetPlayer>().FirstOrDefaultAsync(a=>a.PlayerId==model.PlayerId && a.MarriageGameSetId == model.MarriageGameSetId);
        if(gameSetPlayer is  null)
        {
            await _dbConnection?.InsertAsync(model);
        }
        else
        {
            await _dbConnection?.UpdateAsync(model);
        }
        return model;
    }

    #endregion MarriageGameSetPlayer

    #region MarriageGameSettings
    public async Task<GameSettings> AddGameSettingsAsync(GameSettings model)
    {
         await _dbConnection?.InsertAsync(model);
        return model;
    }
    public async Task<GameSettings?> GetGameSettingsAsync(int id)
    {
       var result = await _dbConnection?.Table<GameSettings>().Where(a => a.Id == id).FirstOrDefaultAsync();

       return result;
    }
     
    #endregion MarriageGameSettings

    #region MarriageGameRound
    public Task<int> AddMarriageGameRoundAsync(MarriageGameRound model)
    {
        return _dbConnection?.InsertAsync(model) ?? Task.Run(() => { return 0; });
    }

    //get all marriage game rounds by marriage game set id

    public async Task<List<MarriageGameRound>> GetMarriageGameRoundsByGameSetIdAsync(int gameSetId)
    {
        var marriageGameRoundTable = _dbConnection?.Table<MarriageGameRound>();
        if (marriageGameRoundTable == null)
        {
            return [];
        }

        var marriageGameRoundList = await marriageGameRoundTable.Where(a => a.MarriageGameSetId == gameSetId).ToListAsync();

        return marriageGameRoundList ?? [];
    }
    //get marriage game round by id

    public async Task<MarriageGameRound?> GetMarriageGameRoundByIdAsync(int id)
    {
        var marriageGameRoundTable = _dbConnection?.Table<MarriageGameRound>();
        if (marriageGameRoundTable == null)
        {
            return null;
        }

        var marriageGameRound = await marriageGameRoundTable.Where(a => a.Id == id).FirstOrDefaultAsync();

        return marriageGameRound;
    }


    public Task<int> DeleteMarriageGameRoundAsync(MarriageGameRound model)
    {
        return _dbConnection?.DeleteAsync(model) ?? Task.Run(() => { return 0; });
    }

    public async Task CleanMarriageGameSet()
    {
        if (_dbConnection == null)
        {
            return;
        }
        
        await _dbConnection.DeleteAllAsync<MarriageGame>();
        await _dbConnection.DeleteAllAsync<MarriageGameRound>();
        await _dbConnection.DeleteAllAsync<MarriageGameScore>();
        await _dbConnection.DeleteAllAsync<MarriageGameSetPlayer>();

        await _dbConnection.DropTableAsync<MarriageGame>();
        await _dbConnection.DropTableAsync<MarriageGameRound>();
        await _dbConnection.DropTableAsync<MarriageGameScore>();
        await _dbConnection.DropTableAsync<MarriageGameSetPlayer>();

        await _dbConnection.CreateTableAsync<MarriageGame>();
        await _dbConnection.CreateTableAsync<MarriageGameRound>();
        await _dbConnection.CreateTableAsync<MarriageGameScore>();
        await _dbConnection.CreateTableAsync<MarriageGameSetPlayer>();

    }



    #endregion MarriageGameRound

    #region MarriageGameScore
    public async Task<MarriageGameScore> AddMarriageGameScoreAsync(MarriageGameScore model)
    {
        await _dbConnection?.InsertAsync(model);

        return model;
    }
    public async Task UpdateMarriageGameScoreAsync(MarriageGameScore marriageGameScore)
    {
        await _dbConnection?.UpdateAsync(marriageGameScore);

    }
    #endregion MarriageGameScore
}