using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocr.Config;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;

namespace Ocr;

internal class BookProcessor
{
    private readonly IOptions<SourceImagesSettings> SourceImagesSettings;
    private readonly IOcrService OcrService;
    private readonly ImageRepository ImageRepository;
    private readonly ILogger<BookProcessor> Logger;

    public BookProcessor(
        IOptions<SourceImagesSettings> sourceImagesSettings,
        ImageRepository imageRepository,
        IOcrService ocrService,
        ILogger<BookProcessor> logger)
    {
        SourceImagesSettings = sourceImagesSettings ?? throw new ArgumentNullException(nameof(sourceImagesSettings));
        ImageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        OcrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(SourceImagesSettings.Value.Directory))
            throw new ValidationException(
                $"Invalid source images directory \"{SourceImagesSettings.Value.Directory}\"");

        string[] imagePaths = ImageRepository.GetRootImageFilePaths();
        await ProcessFilesAsync(imagePaths, cancellationToken);
    }

    private async Task ProcessFilesAsync(string[] imagePaths, CancellationToken cancellationToken)
    {
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 2,
            CancellationToken = cancellationToken
        };
        await Parallel.ForEachAsync(
            imagePaths,
            parallelOptions,
            async (imagePath, cancellationToken) =>
            {
                await ProcessFileAsync(imagePath, cancellationToken);
            });
    }

    private async Task ProcessFileAsync(string imagePath, CancellationToken cancellationToken)
    {
        string textFilePath = Path.ChangeExtension(imagePath, ".ocr.txt");
        string emptyFilePath = Path.ChangeExtension(imagePath, ".ocr.empty");
        if (File.Exists(textFilePath) || File.Exists(emptyFilePath))
            return;

        ImmutableArray<Word> words = await OcrService.GetOcrAsync(imagePath, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        if (words.Length == 0)
        {
            Logger.LogWarning("No words detected in file {imagePath}", imagePath);
            await File.WriteAllTextAsync(emptyFilePath, "");
            return;
        }

        var resultBuilder = new StringBuilder();
        resultBuilder.AppendLine("ID\tX\tY\tWidth\tHeight\tText");
        foreach (var word in words)
            resultBuilder.AppendLine($"{Guid.NewGuid()}\t{word.Left}\t{word.Top}\t{word.Width}\t{word.Height}\t{word.WordText}");

        if (!cancellationToken.IsCancellationRequested)
            await File.WriteAllTextAsync(textFilePath, resultBuilder.ToString());
    }


}