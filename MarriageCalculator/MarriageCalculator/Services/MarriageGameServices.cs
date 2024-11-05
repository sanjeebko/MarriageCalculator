using MarriageCalculator.Core.Models;
using SQLite;

namespace MarriageCalculator.Services;

public partial class MarriageGameServices : IMarriageGameServices
{
    public List<Player> Players { get; } = new();
    public List<MarriageGame> MarriageGames { get; } = new();
    public List<MarriageGameRound> MarriageGameRounds { get; } = new();
    public List<MarriageGamePlayer> MarriageGamePlayers { get; } = new();
   // public List<GameSettings> GameSettings { get; } = new();


    private SQLiteAsyncConnection? _dbConnection;

    public MarriageGameServices()
    {
        SetupDB();
    }

    private async void SetupDB()
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
                await _dbConnection.CreateTableAsync<MarriageGamePlayer>();
                               
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    //Create a function to get all players ordered by name
    public Task<List<Player>> GetPlayers()
    {
        var playerList = _dbConnection?.Table<Player>().Where(a=>!a.Deleted).ToListAsync();

        return playerList ?? Task.Run(static () => { return new List<Player>(); });
    }

    public async Task<int> AddPlayerAsync(Player model)
    {
        if (_dbConnection == null)
        {
            return 0;        
        }
        var allPlayers = await GetPlayers();
        var oldPlayer = allPlayers?.FirstOrDefault  (a => a.name.ToLower() == model.name.ToLower());

        if(oldPlayer is not null)
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


    public async Task<int> DeletePlayer(Player model)
    {
        if (_dbConnection == null)
        {
            return 0;
        }

        model.Deleted = true;
        return await _dbConnection.UpdateAsync(model);
    }

    public Task<List<MarriageGame>> GetMarriageGames()
    {
        var marriageGameList = _dbConnection?.Table<MarriageGame>().ToListAsync();

        return marriageGameList ?? Task.Run(() => { return new List<MarriageGame>(); });
    }

    public Task<int> AddMarriageGame(MarriageGame model)
    {
        return _dbConnection?.InsertAsync(model) ?? Task.Run(() => { return 0; });
    }

    public Task<int> DeleteMarriageGame(MarriageGame model)
    {
        return _dbConnection?.DeleteAsync(model) ?? Task.Run(() => { return 0; });
    }

    public Task<int> UpdateMarriageGame(MarriageGame model)
    {
        return _dbConnection?.UpdateAsync(model) ?? Task.Run(() => { return 0; });
    }

    public Task<List<Currency>> GetCurrency()
    {
        var currencyList = Enum.GetValues(typeof(Currency)).Cast<Currency>().ToList();
        return Task.FromResult(currencyList);
    }
        
}