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
    public async Task CleanMarriageGameSet()
    {
        if (_dbConnection == null)
        {
            return;
        }
        await _dbConnection.DeleteAllAsync<GameSettings>();
        await _dbConnection.DeleteAllAsync<MarriageGame>();        
        await _dbConnection.DeleteAllAsync<MarriageGameRound>();
        await _dbConnection.DeleteAllAsync<MarriageGameScore>();
        await _dbConnection.DeleteAllAsync<MarriageGameSetPlayer>();
        await _dbConnection.DeleteAllAsync<MarriageGameSet>();

        await _dbConnection.DropTableAsync<GameSettings>();
        await _dbConnection.DropTableAsync<MarriageGame>();        
        await _dbConnection.DropTableAsync<MarriageGameRound>();
        await _dbConnection.DropTableAsync<MarriageGameScore>();
        await _dbConnection.DropTableAsync<MarriageGameSetPlayer>();
        await _dbConnection.DropTableAsync<MarriageGameSet>();

        await _dbConnection.CreateTableAsync<GameSettings>();
        await _dbConnection.CreateTableAsync<MarriageGame>();        
        await _dbConnection.CreateTableAsync<MarriageGameRound>();
        await _dbConnection.CreateTableAsync<MarriageGameScore>();
        await _dbConnection.CreateTableAsync<MarriageGameSetPlayer>();
        await _dbConnection.CreateTableAsync<MarriageGameSet>();

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

        var latestMarriageGameSet = await marriageGameSetTable.Where(a=>a.IsActive)
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
        var result = await _dbConnection.UpdateAsync(model);
        if(result == 0)
        {
           result= await _dbConnection.InsertAsync(model);
        }

        return result;
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
        var gameSetPlayerTable = _dbConnection?.Table<MarriageGameSetPlayer>();
        var gameSetPlayer = await gameSetPlayerTable?.FirstOrDefaultAsync(a=>a.PlayerId==model.PlayerId && a.MarriageGameSetId == model.MarriageGameSetId);
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
        if(_dbConnection is null)
            throw new Exception("DB Connection is null");

        await _dbConnection.InsertAsync(model);
        return model;
    }
    public async Task<GameSettings?> GetGameSettingsAsync(int id )
    {
         var gameSettingsTable = _dbConnection?.Table<GameSettings>();
        if (gameSettingsTable == null)
            return null;
         var settings=  await gameSettingsTable.Where(a => a.Id == id).FirstOrDefaultAsync();

       return await gameSettingsTable.Where(a => a.Id == id).FirstOrDefaultAsync();
    }
    public async Task DeleteMarriageGameSettings(GameSettings gameSettings)
    {
        if (_dbConnection == null)
            return;

      var result =  await _dbConnection.Table<GameSettings>().DeleteAsync(gs => gs.Id == gameSettings.Id);
    } 
    public async Task<Dictionary<int,GameSettings>> GetAllGameSettingsAsync()
    {
        var gameSettingsTable = _dbConnection?.Table<GameSettings>();
        if (gameSettingsTable == null)
            return new Dictionary<int, GameSettings>();

        var gameSettingsList = await gameSettingsTable.ToListAsync();
        var gameSettingsDict = new Dictionary<int, GameSettings>();
        foreach (var gameSettings in gameSettingsList)
        {
            if (
                gameSettingsDict.ContainsKey(gameSettings.Id)){
                await DeleteMarriageGameSettings(gameSettings); 
            }
            else
                gameSettingsDict.Add(gameSettings.Id, gameSettings);
        }
        return gameSettingsDict;

    }
    public async Task<GameSettings?> GetGameSettingsByGameSetIdAsync(int gameSetId)
    {
        var gameSetTable = _dbConnection?.Table<MarriageGameSet>();
        if (gameSetTable is null)
            return null;

        var gameSet = await gameSetTable.Where(a => a.Id == gameSetId).FirstOrDefaultAsync();
        if (gameSet is null)
            return null;

        var gameSettingsId = gameSet.GameSettingsId;

        var gameSettingsTable = _dbConnection?.Table<GameSettings>();
        if (gameSettingsTable == null)
            return null;

        return await gameSettingsTable.Where(a => a.Id == gameSettingsId).FirstOrDefaultAsync();
    }
    public async Task<GameSettings?> GetLastGameSettingsAsync()
    {
        var gameSettingsTable =   _dbConnection?.Table<GameSettings>();
        if(gameSettingsTable == null)
        {
            return null;
        }
         
        return await gameSettingsTable.OrderByDescending(a => a.Id).FirstOrDefaultAsync();
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
     
    public async Task UpdateMarriageGameRoundAsync(MarriageGameRound currentMarriageGameRound)
    {
        await _dbConnection?.UpdateAsync(currentMarriageGameRound);
    }

    //create a function that will return list of MarriageGameRound by MarriageGameSetId
    public async Task<List<MarriageGameRound>> GetMarriageGameRoundsByMarriageGameSetIdAsync(int marriageGameSetId)
    {
        var marriageGameRoundTable = _dbConnection?.Table<MarriageGameRound>();
        if (marriageGameRoundTable == null)
            return [];


        var marriageGameRoundList = await marriageGameRoundTable.Where(a => a.MarriageGameSetId == marriageGameSetId).ToListAsync();

        return marriageGameRoundList ?? [];
    }
    public async Task<List<MarriageGame>> GetMarriageGamesByRoundIdAsync(int marriageRoundId)
    {
        var marriageGameTable = _dbConnection?.Table<MarriageGame>();
        if (marriageGameTable == null)
            return [];


        var marriageGameList = await marriageGameTable.Where(a => a.MarriageGameRoundId == marriageRoundId).ToListAsync();

        return marriageGameList ?? [];
    }
    public async Task<List<MarriageGameSetPlayer>> GetMarriageGameSetPlayersByMarriageGameSetIdAsync(int id)
    {
        var marriageGameSetPlayerTable = _dbConnection?.Table<MarriageGameSetPlayer>();
        if (marriageGameSetPlayerTable == null)
            return [];
        var allPlayerSet = await marriageGameSetPlayerTable.ToListAsync();
        if (allPlayerSet.Count == 0)
        {
            //find all game round ids
            var allGameRoundIds = await GetMarriageGameRoundsByGameSetIdAsync(id);
            List<int> playerSetIds = [];
            foreach (var gameround in allGameRoundIds)
            {
                //find all marriagegame ids
                var allMarriageGameIds = await GetMarriageGamesByRoundIdAsync(gameround.Id);
                // find all scores for the game
                foreach (var game in allMarriageGameIds)
                {
                    var allScores = await GetMarriageGameScoresByMarriageGameIdAsync(game.Id);
                    foreach (var score in allScores)
                    {
                        
                        if (!playerSetIds.Contains(score.PlayerId))
                        {
                            playerSetIds.Add(score.PlayerId);
                        }
                    }
                }
            }
            foreach(var playerid in playerSetIds)
            {
                await AddMarriageGameSetPlayerAsync(new MarriageGameSetPlayer { PlayerId = playerid, MarriageGameSetId = id });
            }
            marriageGameSetPlayerTable = _dbConnection?.Table<MarriageGameSetPlayer>();
        }
        if (marriageGameSetPlayerTable == null) return [];

        var marriageGameSetPlayerList = await marriageGameSetPlayerTable.Where(a => a.MarriageGameSetId == id).ToListAsync();

        return marriageGameSetPlayerList ?? [];

    }

    public async Task<MarriageGameScore> AddMarriageGameScoreAsync(MarriageGameScore model)
    {
        if (_dbConnection == null)
        {
            throw new Exception("DB Connection is null");
        }

        var existingScore = await _dbConnection.Table<MarriageGameScore>()
                                               .Where(s => s.MarriageGameId == model.MarriageGameId && s.PlayerId == model.PlayerId)
                                               .FirstOrDefaultAsync();

        if (existingScore == null)
        {
            await _dbConnection.InsertAsync(model);
        }
        else
        {
            model.Id = existingScore.Id;
            await _dbConnection.UpdateAsync(model);
        }

        return model;
    } 

    public async Task<List<MarriageGameScore>> GetMarriageGameScoresByMarriageGameIdAsync(int id)
    {
        var marriageGameScoreTable = _dbConnection?.Table<MarriageGameScore>();
        if (marriageGameScoreTable == null)
            return []; 

        var marriageGameScoreList = await marriageGameScoreTable.Where(a => a.MarriageGameId == id).ToListAsync();

        return marriageGameScoreList  ??[];
    }

  


    #endregion MarriageGameScore
}