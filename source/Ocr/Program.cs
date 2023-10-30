using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ocr.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ocr;

internal class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        var environment = builder.Environment;
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

        builder.RegisterOptions<OcrApiSettings>();
        builder.RegisterOptions<SourceImagesSettings>();
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<IOcrService, OcrService>();
        builder.Services.AddSingleton<BookProcessor>();

        IHost host = builder.Build();

        var bookProcessor = host.Services.GetRequiredService<BookProcessor>();

        var cancellationTokenSource = new CancellationTokenSource();

        var backgroundTask = Task.Run(() => bookProcessor.StartAsync(cancellationTokenSource.Token));

        Console.CancelKeyPress += (_, args) =>
        {
            Console.WriteLine("CTRL+C pressed. Cancelling...");
            cancellationTokenSource.Cancel();
            args.Cancel = true;
        };

        try
        {
            await backgroundTask;
        }
        catch (TaskCanceledException) { }

        if (cancellationTokenSource.IsCancellationRequested)
            Console.WriteLine("Cancelled");
    }
}
