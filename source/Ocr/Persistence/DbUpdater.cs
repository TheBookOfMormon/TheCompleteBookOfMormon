using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocr.Config;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using TheCompleteBookOfMormon.Domain;
using TheCompleteBookOfMormon.Domain.Editions;

namespace Ocr.Persistence;

internal class DbUpdater
{
    private readonly IEditionsRepository EditionsRepository;
    private readonly IOptions<SourceImagesSettings> SourceImagesSettings;
    private readonly IUnitOfWork UnitOfWork;
    private readonly ILogger<DbUpdater> Logger;

    public DbUpdater(
        IOptions<SourceImagesSettings> sourceImagesSettings,
        IEditionsRepository editionsRepository,
        IUnitOfWork unitOfWork,
        ILogger<DbUpdater> logger)
    {
        EditionsRepository = editionsRepository ?? throw new ArgumentNullException(nameof(editionsRepository));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        SourceImagesSettings = sourceImagesSettings ?? throw new ArgumentNullException(nameof(sourceImagesSettings));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask ExecuteAsync(CancellationToken cancellationToken)
    {
        string[] editionIndexFilePaths = Directory.GetFiles(SourceImagesSettings.Value.Directory, "index.json", SearchOption.AllDirectories);
        foreach (string editionIndexFilePath in editionIndexFilePaths)
        {
            if (cancellationToken.IsCancellationRequested) return;

            Edition edition = await ProcessEditionAsync(editionIndexFilePath);
            await ProcessPagesAsync(edition, Path.GetDirectoryName(editionIndexFilePath)!);

            await UnitOfWork.CommitAsync(); 
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
        return edition;
    }

    private async Task ProcessPagesAsync(Edition edition, string filesPath)
    {
        await Task.Yield();   
    }

}

