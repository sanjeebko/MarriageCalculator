using MarriageCalculator.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.Services;

public interface IMarriageGameServices
{
    Task<List<MarriageGameModel>> GetMarriageGames();

    Task<int> AddMarriageGame(MarriageGameModel model);

    Task<int> DeleteMarriageGame(MarriageGameModel model);

    Task<int> UpdateMarriageGame(MarriageGameModel model);
}