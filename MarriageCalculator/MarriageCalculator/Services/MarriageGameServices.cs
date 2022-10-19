//using Javax.Security.Auth;

using MarriageCalculator.Core.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.Services;

public class MarriageGameServices : IMarriageGameServices
{
    public List<PlayerModel> Players { get; } = new();

    private SQLiteAsyncConnection? _dbConnection;

    public MarriageGameServices()
    {
        SetupDB();
    }

    private async void SetupDB()
    {
        if (_dbConnection is null)
        {
            string dbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MarriageCalculator.db3");
            _dbConnection = new SQLiteAsyncConnection(dbPath);
            await _dbConnection.CreateTableAsync<MarriageGameModel>();
            await _dbConnection.CreateTableAsync<MarriageGameRoundModel>();
            await _dbConnection.CreateTableAsync<GameSettingsModel>();
        }
    }

    public Task<List<MarriageGameModel>> GetMarriageGames()
    {
        var marriageGameList = _dbConnection?.Table<MarriageGameModel>().ToListAsync();

        return marriageGameList ?? Task.Run(() => { return new List<MarriageGameModel>(); });
    }

    public Task<int> AddMarriageGame(MarriageGameModel model)
    {
        return _dbConnection?.InsertAsync(model) ?? Task.Run(() => { return 0; });
    }

    public Task<int> DeleteMarriageGame(MarriageGameModel model)
    {
        return _dbConnection?.DeleteAsync(model) ?? Task.Run(() => { return 0; });
    }

    public Task<int> UpdateMarriageGame(MarriageGameModel model)
    {
        return _dbConnection?.UpdateAsync(model) ?? Task.Run(() => { return 0; });
    }
}