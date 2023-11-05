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
    private readonly IOptions<OcrSettings> OcrSettings;
    private readonly IOcrService OcrService;
    private readonly ImageRepository ImageRepository;
    private readonly ILogger<BookProcessor> Logger;

    public BookProcessor(
        IOptions<OcrSettings> ocrSettings,
        ImageRepository imageRepository,
        IOcrService ocrService,
        ILogger<BookProcessor> logger)
    {
        OcrSettings = ocrSettings ?? throw new ArgumentNullException(nameof(ocrSettings));
        ImageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        OcrService = ocrService ?? throw new ArgumentNullException(nameof(ocrService));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!Directory.Exists(OcrSettings.Value.ScanDirectory))
            throw new ValidationException(
                $"Invalid source images directory \"{OcrSettings.Value.ScanDirectory}\"");
        Directory.CreateDirectory(OcrSettings.Value.OcrDirectory);

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
        string imageFileName = Path.GetFileNameWithoutExtension(imagePath);
        string imageFileFolderName = new DirectoryInfo(imagePath).Parent!.Name;
        string ocrFilePath = Path.Combine(OcrSettings.Value.OcrDirectory, imageFileFolderName, imageFileName + ".ocr.txt");
        string emptyOcrFilePath = Path.ChangeExtension(ocrFilePath, ".empty");
        if (File.Exists(ocrFilePath) || File.Exists(emptyOcrFilePath))
            return;

        ImmutableArray<Word> words = await OcrService.GetOcrAsync(imagePath, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
            return;

        if (words.Length == 0)
        {
            Logger.LogWarning("No words detected in file {imagePath}", imagePath);
            await File.WriteAllTextAsync(emptyOcrFilePath, "");
            return;
        }

        var resultBuilder = new StringBuilder();
        resultBuilder.AppendLine("ID\tX\tY\tWidth\tHeight\tText");
        foreach (var word in words)
            resultBuilder.AppendLine($"{Guid.NewGuid()}\t{word.Left}\t{word.Top}\t{word.Width}\t{word.Height}\t{word.WordText}");

        if (!cancellationToken.IsCancellationRequested)
        {
            string folderPath = Path.GetDirectoryName(ocrFilePath)!;
            Directory.CreateDirectory(folderPath);
            await File.WriteAllTextAsync(ocrFilePath, resultBuilder.ToString());
        }
    }


}