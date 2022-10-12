using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MarriageCalculator.Core.Models;

public class Player : INotifyPropertyChanged
{
    [Key]
    public int Id { get; set; }

    private string name;
    private int position;
    private string email;

    public int Position
    {
        get => position;
        set
        {
            if (position < 0)
                throw new ArgumentException($"{nameof(Position)} can not be negative.");
            position = value;
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

    public string Email
    {
        get => email;
        set
        {
            if (email == value || string.IsNullOrEmpty(value)) return;

            email = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Email)));
        }
    }

    private string GetDefaultName() => $"Player {Position}";

    public void SetPosition(int newPosition)
    {
        if (newPosition == 0) return;
        Position = newPosition;
    }

    public event PropertyChangedEventHandler PropertyChanged;
}