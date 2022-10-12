using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarriageCalculator.Core.Models;

public class MarriageGame : INotifyPropertyChanged
{
    private int id;
    private string name;

    [Key]
    public int Id
    {
        get => id;
        set
        {
            if (id == value) return;
            id = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Id)));
        }
    }

    public string Name
    {
        get => name ?? GetDefaultName();
        set
        {
            if (name == value || string.IsNullOrEmpty(value)) return;

            name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    }

    public List<Player> Players { get; set; } = new();

    public MarriageGame(string name)
    {
        if (string.IsNullOrEmpty(name))
            Name = GetDefaultName();
        Name = name;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private static string GetDefaultName() => $"New Game {DateTime.Now:f}";
}