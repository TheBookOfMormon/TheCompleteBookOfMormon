using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocr.Config;
using System.Collections.Immutable;
using System.Text.Json;
using TheCompleteBookOfMormon.Domain;
using TheCompleteBookOfMormon.Domain.Editions;
using TheCompleteBookOfMormon.Domain.Pages;

namespace Ocr.Persistence;

internal class DbUpdater
{
    private readonly IEditionsRepository EditionsRepository;
    private readonly IPageRepository PageRepository;
    private readonly ImageRepository ImageRepository;
    private readonly IOptions<SourceImagesSettings> SourceImagesSettings;
    private readonly HashService HashService;

    private readonly IUnitOfWork UnitOfWork;
    private readonly ILogger<DbUpdater> Logger;

    public DbUpdater(
        IEditionsRepository editionsRepository,
        ImageRepository imageRepository,
        IPageRepository pageRepository,
        IOptions<SourceImagesSettings> sourceImagesSettings,
        HashService hashService,
        IUnitOfWork unitOfWork,
        ILogger<DbUpdater> logger)
    {
        EditionsRepository = editionsRepository ?? throw new ArgumentNullException(nameof(editionsRepository));
        ImageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
        PageRepository = pageRepository ?? throw new ArgumentNullException(nameof(pageRepository));
        SourceImagesSettings = sourceImagesSettings ?? throw new ArgumentNullException(nameof(sourceImagesSettings));
        HashService = hashService ?? throw new ArgumentNullException(nameof(hashService));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(CancellationToken cancellationToken)
    {
        string[] editionIndexFilePaths = Directory.GetFiles(SourceImagesSettings.Value.Directory, "index.json", SearchOption.AllDirectories);
        foreach (string editionIndexFilePath in editionIndexFilePaths)
        {
            if (cancellationToken.IsCancellationRequested) return;

            Edition edition = await ProcessEditionAsync(editionIndexFilePath);
            await ProcessPagesAsync(edition, Path.GetDirectoryName(editionIndexFilePath)!, cancellationToken);
        }
    }

    private async ValueTask<Edition> ProcessEditionAsync(string editionIndexFilePath)
    {
        Logger.LogInformation("Processing {EditionIndexFilePath}", editionIndexFilePath);

        using Stream jsonStream = File.OpenRead(editionIndexFilePath);
        var editionOcrInfo = JsonSerializer.Deserialize<EditionOcrInfo>(jsonStream, DefaultJsonSerializerOptions.Value)!;

        Edition edition = await EditionsRepository.GetByCodeAsync(editionOcrInfo.Code) ?? new();
        edition.Code = editionOcrInfo.Code;
        edition.ExcludeFromUI = editionOcrInfo.ExcludeFromUI;
        edition.Name = editionOcrInfo.Name;
        edition.Year = editionOcrInfo.Year;

        EditionsRepository.Save(edition);
        await UnitOfWork.CommitAsync();

        return edition;
    }

    private async Task ProcessPagesAsync(Edition edition, string filesPath, CancellationToken cancellationToken)
    {
        ImmutableArray<Page> pages = await PageRepository.GetAllAsync(edition.Id);
        ImmutableDictionary<int, Page> pagesByNumber = pages.ToImmutableDictionary(x => x.Number);

        string[] imageFilePaths = ImageRepository.GetImageFilePaths(filesPath);
        foreach (string imageFilePath in imageFilePaths)
        {
            if (cancellationToken.IsCancellationRequested) return;
            await ProcessPageAsync(edition, imageFilePath, pagesByNumber, cancellationToken);
        }
    }

    private async Task ProcessPageAsync(
        Edition edition,
        string imageFilePath,
        ImmutableDictionary<int, Page> pagesByNumber,
        CancellationToken cancellationToken)
    {
        int pageNumber = int.Parse(Path.GetFileNameWithoutExtension(imageFilePath));

        if (!pagesByNumber.TryGetValue(pageNumber, out Page? page))
        {
            page =
                 await PageRepository.GetAsync(edition.Id, pageNumber)
                 ?? new Page
                 {
                     EditionId = edition.Id,
                     Number = pageNumber,
                     Scan = new()
                 };
        }

        string fileExt = Path.GetExtension(imageFilePath)[1..].ToLowerInvariant();
        var fileInfo = new FileInfo(imageFilePath);

        if (page.Matches(fileInfo.Length, fileExt, fileInfo.LastWriteTimeUtc)) return;

        page.FileExtension = fileExt;
        page.FileSize = fileInfo.Length;
        page.LastWrittenUtc = fileInfo.LastWriteTimeUtc;
        page.Scan.Data = await File.ReadAllBytesAsync(imageFilePath);

        Logger.LogInformation("Updating {EditionCode} page {PageNumber}", edition.Code, pageNumber);
        PageRepository.Save(page);
        await UnitOfWork.CommitAsync();
    }
}

