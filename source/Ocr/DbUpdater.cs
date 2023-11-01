using Microsoft.Extensions.Options;
using Ocr.Config;
using System.Runtime.CompilerServices;
using TheCompleteBookOfMormon.Domain;
using TheCompleteBookOfMormon.Domain.Editions;

namespace Ocr;

internal class DbUpdater
{
    private readonly IEditionsRepository EditionsRepository;
    private readonly IOptions<SourceImagesSettings> SourceImagesSettings;
    private readonly IUnitOfWork UnitOfWork;

    public DbUpdater(
        IOptions<SourceImagesSettings> sourceImagesSettings,
        IEditionsRepository editionsRepository,
        IUnitOfWork unitOfWork)
    {
        EditionsRepository = editionsRepository ?? throw new ArgumentNullException(nameof(editionsRepository));
        UnitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        SourceImagesSettings = sourceImagesSettings ?? throw new ArgumentNullException(nameof(sourceImagesSettings));
    }

    public async ValueTask ExecuteAsync(CancellationToken cancellationToken)
    {
        string[] editionIndexFilePaths = Directory.GetFiles(SourceImagesSettings.Value.Directory, "index.json", SearchOption.AllDirectories);
        foreach(string editionIndexFilePath in editionIndexFilePaths)
        {
            if (cancellationToken.IsCancellationRequested) return;

            await ProcessEditionAsync(editionIndexFilePath);
        }
    }

    private async ValueTask ProcessEditionAsync(string editionIndexFilePath)
    {
        throw new NotImplementedException();
    }
}
