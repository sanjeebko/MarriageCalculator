using MarriageCalculator.Core.Models;

namespace MarriageCalculator.DataServices;

public class RestDataService : IRestDataService
{
    private readonly HttpClient _httpClient;
    //private readonly string _baseAddress;

    private RestDataService()
    {
        _httpClient = new HttpClient();
        //  _baseAddress = "x";
    }

    public Task AddMarriageGameAsync(MarriageGameModel marriageGame)
    {
        throw new NotImplementedException();
    }

    public Task DeleteMarriageGameAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<MarriageGameModel>> GetAllMarriageGamesAsync()
    {
        throw new NotImplementedException();
    }

    public Task UpdateMarriageGameAsync(MarriageGameModel marriageGame)
    {
        throw new NotImplementedException();
    }
}