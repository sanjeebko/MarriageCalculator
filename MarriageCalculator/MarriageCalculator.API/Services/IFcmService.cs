using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

public interface IFcmService
{
    Task SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null);

    /// <summary>
    /// Sends a data-only FCM message (no Notification block). Required for messages the client
    /// must render itself - e.g. an actionable notification with buttons - since a message that
    /// includes a Notification block is auto-displayed by the OS when the app is backgrounded or
    /// killed, and does not reliably invoke the client's onMessageReceived in that state.
    /// </summary>
    Task SendDataMessageAsync(string token, Dictionary<string, string> data);
}
