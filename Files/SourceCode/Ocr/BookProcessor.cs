using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocr.Config;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Ocr;

internal class BookProcessor : IHostedService
{
    private readonly IOptions<SourceImagesSettings> SourceImagesSettings;
    private readonly IOcrService OcrService;
    private readonly ILogger<BookProcessor> Logger;
    private readonly CancellationTokenSource CancellationTokenSource;

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions { WriteIndented = true };

    public BookProcessor(
        IOptions<SourceImagesSettings> sourceImagesSettings,
        IOcrService ocrService,
        ILogger<BookProcessor> logger)
    {
        SourceImagesSettings = sourceImagesSettings ?? throw new ArgumentNullException(nameof(sourceImagesSettings));
        OcrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        CancellationTokenSource = new CancellationTokenSource();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(SourceImagesSettings.Value.Directory))
            throw new ValidationException(
                $"Invalid source images directory \"{SourceImagesSettings.Value.Directory}\"");

        string[] imagePaths = GetImagePaths();
        await ProcessFilesAsync(imagePaths);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        CancellationTokenSource.Cancel();
        return Task.CompletedTask;
    }

    private string[] GetImagePaths()
    {
        Logger.LogInformation("Discovering images");

        var result = new List<string>();
        foreach (string ext in new string[] { "png", "jpeg", "jpg" })
        {
            string[] imagePaths = Directory.GetFiles(
                path: SourceImagesSettings.Value.Directory,
                searchPattern: $"*.{ext}",
                new EnumerationOptions { RecurseSubdirectories = true });
            result.AddRange(imagePaths);
        }
        return result.OrderBy(x => x).ToArray();
    }

    private async Task ProcessFilesAsync(string[] imagePaths)
    {
        foreach (string imagePath in imagePaths)
        {
            if (CancellationTokenSource.IsCancellationRequested)
                return;
            await ProcessFileAsync(imagePath);
        }
    }


    private async Task ProcessFileAsync(string imagePath)
    {
        string textFilePath = Path.ChangeExtension(imagePath, ".txt");
        if (File.Exists(textFilePath))
        {
            Logger.LogInformation("Skipping already processed file {imagePath}", imagePath);
            return;
        }

        ImmutableArray<Word> words = await OcrService.GetOcrAsync(imagePath, CancellationTokenSource.Token);

        if (words.Length == 0)
        {
            Logger.LogError("No words in file {imagePath}", imagePath);
            return;
        }

        string json = JsonSerializer.Serialize(words, JsonOptions);
        await File.WriteAllTextAsync(textFilePath, json);
    }


}