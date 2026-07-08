using System.Collections.Generic;
using System.Threading.Tasks;

namespace MarriageCalculator.API.Services;

public interface IFcmService
{
    Task SendNotificationAsync(string token, string title, string body, Dictionary<string, string>? data = null);
}
