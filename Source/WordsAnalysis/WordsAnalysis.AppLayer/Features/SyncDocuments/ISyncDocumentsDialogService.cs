namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public interface ISyncDocumentsDialogService
{
    Task<EditWordDialogResult?> ShowEditWordDialogAsync(EditWordDialogContent content);
    Task<DeleteWordsDialogResult?> ShowDeleteWordsDialogAsync(DeleteWordsDialogContent content);
    Task<RescanAreaDialogResult?> ShowRescanAreaDialogAsync(RescanAreaDialogContent content);
    Task<SplitWordsDialogResult?> ShowSplitWordsDialogAsync(SplitWordsDialogContent content);
    Task ShowViewColumnImagesDialogAsync(ViewColumnImagesDialogContent content);
}
