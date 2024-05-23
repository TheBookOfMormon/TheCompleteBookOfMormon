using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ocr.Config;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ocr.Persistence;

namespace Ocr;

internal class Program
{
    static async Task Main(string[] args)
    {
        HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
        var environment = builder.Environment;
        builder.Configuration.AddJsonFile("appsettings.json");
        builder.Configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true);

        builder.RegisterOptions<OcrSettings>();
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<IOcrService, OcrService>();
        builder.Services.AddScoped<BookProcessor>();
        builder.Services.AddScoped<DbUpdater>();
        builder.Services.AddScoped<ImageRepository>();
        builder.Services.AddScoped<HashService>();
        TheCompleteBookOfMormon.Domain.Services.Register(builder.Services, builder.Configuration);

        IHost host = builder.Build();

        var cancellationTokenSource = new CancellationTokenSource();

        using IServiceScope serviceScope = host.Services.CreateScope();
        var backgroundTask = Task.Run(() => ExecuteAsync(serviceScope.ServiceProvider, cancellationTokenSource));

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

    private static async Task ExecuteAsync(IServiceProvider serviceProvider, CancellationTokenSource cancellationTokenSource)
    {
        var bookProcessor = serviceProvider.GetRequiredService<BookProcessor>();
        await bookProcessor.ExecuteAsync(cancellationTokenSource.Token);

        var dbUpdater = serviceProvider.GetRequiredService<DbUpdater>();
        await dbUpdater.ExecuteAsync(cancellationTokenSource.Token);
    }
}
