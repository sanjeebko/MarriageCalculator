using MarriageCalculator.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.DataServices;

public interface IRestDataService
{
    Task<List<MarriageGameModel>> GetAllMarriageGamesAsync();

    Task AddMarriageGameAsync(MarriageGameModel marriageGame);

    Task UpdateMarriageGameAsync(MarriageGameModel marriageGame);

    Task DeleteMarriageGameAsync(int id);
}