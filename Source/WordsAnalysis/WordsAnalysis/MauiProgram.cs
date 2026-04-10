using Microsoft.FluentUI.AspNetCore.Components;
using Microsoft.Maui.LifecycleEvents;
#if DEBUG
using Microsoft.Extensions.Logging;
#endif

namespace WordsAnalysis;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("TimesNewRoman-Regular.ttf", "TimesNewRomanRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddFluentUIComponents();
        Bootstrapper.RegisterServices(builder.Services);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddDebug();
#endif

        builder.ConfigureLifecycleEvents(events =>
        {
        });

        return builder.Build();
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"Unhandled exception: {e.ExceptionObject}");
        if (e.ExceptionObject is Exception ex)
        {
            System.Diagnostics.Trace.TraceError($"Unhandled exception: {ex}");
        }
    }
}
