using MarriageCalculator.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.Services;

public interface IMarriageGameServices
{
    Task<List<MarriageGame>> GetMarriageGames();

    Task<int> AddMarriageGame(MarriageGame model);

    Task<int> DeleteMarriageGame(MarriageGame model);

    Task<int> UpdateMarriageGame(MarriageGame model);

    Task<List<string>> GetCurrency();
}