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
        builder.Services.AddHostedService<BookProcessor>();
        builder.Services.AddSingleton(new HttpClient());
        builder.Services.AddSingleton<IOcrService, OcrService>();

        IHost host = builder.Build();

        await host.RunAsync();
    }
}
