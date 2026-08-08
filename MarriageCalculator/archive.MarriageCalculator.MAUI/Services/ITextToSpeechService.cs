
namespace MarriageCalculator
{
    public interface ITextToSpeechService
    {
        bool Mute { get; set; }

        Task InitializeAsync();
        Task SpeakAsync(Player[] players);
        Task SpeakAsync(string text);
    }
}