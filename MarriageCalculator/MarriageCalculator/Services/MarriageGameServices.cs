//using Javax.Security.Auth;

using MarriageCalculator.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.Services;

public class MarriageGameServices
{
    public List<Player> Players { get; } = new();

    public MarriageGameServices()
    {
        PopulatePlayers();
    }

    private void PopulatePlayers()
    {
        const int PLAYER_COUNT = 6;
        for (int i = 0; i < PLAYER_COUNT; i++)
        {
            Players.Add(new Player
            {
                Email = $"abcd{i}@gmail.com",
                Name = $"Player {i}",
                Position = (i + 1)
            });
        }
    }
}