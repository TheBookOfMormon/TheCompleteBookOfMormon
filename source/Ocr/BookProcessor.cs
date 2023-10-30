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
        return result
            .Where(x => !x.Contains("Cow", StringComparison.InvariantCultureIgnoreCase))
            .OrderBy(x => x).ToArray();
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
        string textFilePath = Path.ChangeExtension(imagePath, ".ocr.txt");
        string emptyFilePath = Path.ChangeExtension(imagePath, ".ocr.empty");
        if (File.Exists(textFilePath) || File.Exists(emptyFilePath))
        {
            Logger.LogInformation("Skipping already processed file {imagePath}", imagePath);
            return;
        }

        ImmutableArray<Word> words = await OcrService.GetOcrAsync(imagePath, CancellationTokenSource.Token);
        if (words.Length == 0)
        {
            Logger.LogWarning("No words detected in file {imagePath}", imagePath);
            await File.WriteAllTextAsync(emptyFilePath, "");
            return;
        }

        try
        {
            using var fileStream = File.CreateText(textFilePath);
            await fileStream.WriteLineAsync("ID\tX\tY\tWidth\tHeight\tText");
            foreach (var word in words)
            {
                await fileStream.WriteLineAsync($"{Guid.NewGuid()}\t{word.Left}\t{word.Top}\t{word.Width}\t{word.Height}\t{word.WordText}");
            }
            await fileStream.FlushAsync();
        }
        catch
        {
            File.Delete(textFilePath);
        }
    }


}