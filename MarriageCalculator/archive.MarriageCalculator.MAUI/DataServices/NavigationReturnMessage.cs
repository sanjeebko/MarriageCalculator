using CommunityToolkit.Mvvm.Messaging.Messages;

namespace MarriageCalculator.DataServices;

public class NavigationReturnMessage : ValueChangedMessage<string>
{
    public NavigationReturnMessage(string value) : base(value) { }
}
