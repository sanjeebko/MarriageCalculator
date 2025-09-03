namespace MarriageCalculator.Helpers;

public static class ServiceHelper
{
    public static T GetService<T>() where T : notnull
        => Current.GetService<T>() ?? throw new InvalidOperationException($"Service of type {typeof(T)} not found.");

    public static IServiceProvider Current =>
#if ANDROID || IOS || WINDOWS || MACCATALYST
        Microsoft.Maui.Controls.Application.Current?.Handler?.MauiContext?.Services
        ?? throw new InvalidOperationException("MauiContext Services are not available yet.");
#else
        throw new PlatformNotSupportedException();
#endif
}