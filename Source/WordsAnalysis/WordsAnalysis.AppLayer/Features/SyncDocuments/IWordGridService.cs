namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public interface IWordGridService
{
    Task<WordGridLocation> GetWordGridLocationAsync();
}
