using MarriageCalculator.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.DataServices;

public interface IRestDataService
{
    Task<List<MarriageGame>> GetAllMarriageGamesAsync();

    Task AddMarriageGameAsync(MarriageGame marriageGame);

    Task UpdateMarriageGameAsync(MarriageGame marriageGame);

    Task DeleteMarriageGameAsync(int id);
}