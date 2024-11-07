
namespace MarriageCalculator
{
    public interface ITextToSpeechService
    {
        Task InitializeAsync();
        Task SpeakAsync(Player[] players);
        Task SpeakAsync(string text);
    }
}