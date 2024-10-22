//using Javax.Security.Auth;

using MarriageCalculator.Core.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.Services;

public partial class MarriageGameServices : IMarriageGameServices
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
            await _dbConnection.CreateTableAsync<MarriageGame>();
            await _dbConnection.CreateTableAsync<MarriageGameRound>();
            await _dbConnection.CreateTableAsync<GameSettings>();
        }
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