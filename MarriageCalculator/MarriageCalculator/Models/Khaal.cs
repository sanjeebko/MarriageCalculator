using CommunityToolkit.Mvvm.ComponentModel;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.Models;

public partial class KhaalModel : ObservableObject
{
    public int KhaalSequence { get; set; }

    [ObservableProperty]
    public int _player1Seat;

    [ObservableProperty]
    public int _player2Seat;

    [ObservableProperty]
    public int _player3Seat;

    [ObservableProperty]
    public int _player4Seat;

    [ObservableProperty]
    public int _player5Seat;

    [ObservableProperty]
    public int _player6Seat;

    [ObservableProperty]
    public int _player1Maal;

    [ObservableProperty]
    public int _player2Maal;

    [ObservableProperty]
    public int _player3Maal;

    [ObservableProperty]
    public int _player4Maal;

    [ObservableProperty]
    public int _player5Maal;

    [ObservableProperty]
    public int _player6Maal;

    [ObservableProperty]
    public int _totalMaal;

    [ObservableProperty]
    public bool _player1Seen;

    [ObservableProperty]
    public bool _player2Seen;

    [ObservableProperty]
    public bool _player3Seen;

    [ObservableProperty]
    public bool _player4Seen;

    [ObservableProperty]
    public bool _player5Seen;

    [ObservableProperty]
    public bool _player6Seen;

    public int Player1Score { get; set; }
    public int Player2Score { get; set; }
    public int Player3Score { get; set; }
    public int Player4Score { get; set; }
    public int Player5Score { get; set; }
    public int Player6Score { get; set; }

    public bool IsPlayingPlayer1 { get; set; }
    public bool IsPlayingPlayer2 { get; set; }
    public bool IsPlayingPlayer3 { get; set; }
    public bool IsPlayingPlayer4 { get; set; }
    public bool IsPlayingPlayer5 { get; set; }
    public bool IsPlayingPlayer6 { get; set; }

    public bool IsDupleePlayer1 { get; set; }
    public bool IsDupleePlayer2 { get; set; }
    public bool IsDupleePlayer3 { get; set; }
    public bool IsDupleePlayer4 { get; set; }
    public bool IsDupleePlayer5 { get; set; }
    public bool IsDupleePlayer6 { get; set; }

    public bool Player1Foul { get; set; }
    public bool Player2Foul { get; set; }
    public bool Player3Foul { get; set; }
    public bool Player4Foul { get; set; }
    public bool Player5Foul { get; set; }
    public bool Player6Foul { get; set; }

    public string WinnerName { get; set; }
    public string DealerName { get; set; }
    public bool FinalKhaal { get; set; }
}