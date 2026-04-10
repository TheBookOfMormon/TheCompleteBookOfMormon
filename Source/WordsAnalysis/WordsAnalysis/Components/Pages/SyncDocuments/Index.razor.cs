using DocumentsModel;
using DocumentsModel.Helpers;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.AppLayer.Services;
using WordsAnalysis.Services;
using WordsAnalysis.AppLayer.Extensions;

namespace WordsAnalysis.Components.Pages.SyncDocuments;

public partial class Index : IDisposable
{
    private bool IsSearchingForNextError;
    private int LoadingCount;
    private bool Loading => LoadingCount > 0;
    private ElementReference SectionNumberElement;
    private bool ShowLoadingIndicator;
    private SyncDocumentsViewModel ViewModel = null!;

    [Inject]
    private IAppPreferences AppPreferences { get; set; } = null!;

    [Inject]
    private IDataPaths DataPaths { get; set; } = null!;

    [Inject]
    private IHtmlService HtmlService { get; set; } = null!;

    [Inject]
    private INotificationService NotificationService { get; set; } = null!;

    [Inject]
    private ISyncDocumentsDialogService SyncDocumentsDialogService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        ShowLoadingIndicator = true;
        LoadingCount++;
        await base.OnInitializedAsync();
        FeatureState state = await Task.Run(() => FeatureState.LoadAsync());
        ViewModel = new SyncDocumentsViewModel(
            state, SyncDocumentsDialogService, DictionaryService, HtmlService,
            NotificationService, DataPaths,
            async () => { await Task.Yield(); StateHasChanged(); });
        await ViewModel.LoadRowDataAsync(0);
        LoadingCount--;
        ShowLoadingIndicator = false;
        _ = PreloadPageImages();
    }

    void IDisposable.Dispose()
    {
        StopSearchingForNextError();
    }

    private void StopSearchingForNextError()
    {
        IsSearchingForNextError = false;
    }

    private async Task PreloadPageImages()
    {
        IEnumerable<OcrBookInfoAndPageNumber> visiblePages = ViewModel.GetVisiblePages();
        await Task.Delay(500);
        IEnumerable<OcrBookInfoAndPageNumber> newPages = ViewModel.GetVisiblePages();
        if (!Enumerable.SequenceEqual(visiblePages, newPages))
            return;

        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 2) };
        await Parallel.ForEachAsync(visiblePages, parallelOptions, (x, _) =>
        {
            string filePath = FilePathHelper.GetScansDeskewedImageFilePath(
                sourcesDirectoryPath: DataPaths.SourcesDirectoryPath,
                bookInfo: x.BookInfo,
                pageNumber: x.PageNumber);
            MagickImage image = ImageRepository.GetPageImage(filePath);
            ImageRepository.GetFilteredPageImage(filePath, () =>
            {
                var filteredImage = new MagickImage(image.Clone());
                var options = new PageState.ImageOptions {
                    ApplyThreshold = AppPreferences.EditWordDialog.ApplyThreshold,
                    ShowHighContrast = AppPreferences.EditWordDialog.ShowHighContrast,
                    ThresholdLower = AppPreferences.EditWordDialog.ThresholdLower,
                    ThresholdUpper = AppPreferences.EditWordDialog.ThresholdUpper
                };
                filteredImage.ApplyImageOptions(options);
                return filteredImage;
            });
            return default;
        });
    }

    private async Task SaveChangesAsync()
    {
        await ViewModel.SaveChangesAsync();
        await SectionNumberElement.FocusAsync();
    }

    private async Task SelectedSectionIndexChanged(ChangeEventArgs e)
    {
        int newIndex = Convert.ToInt32(e.Value);
        LoadingCount++;
        StateHasChanged();
        await Task.Yield();
        await ViewModel.LoadRowDataAsync(newIndex);
        await HtmlService.ScrollBodyToTopLeftAsync();
        StateHasChanged();
        await Task.Yield();
        LoadingCount--;
        _ = PreloadPageImages();
    }

    private async Task ScrollToNextWarningOrErrorAsync()
    {
        IsSearchingForNextError = true;
        LoadingCount++;
        try
        {
            while (IsSearchingForNextError)
            {
                bool hasWarningOrError = await HtmlService.ScrollToNextErrorAsync();
                if (hasWarningOrError)
                    break;

                if (ViewModel.SectionIndex < ViewModel.SectionCount - 1)
                {
                    if (IsSearchingForNextError)
                    {
                        await SelectedSectionIndexChanged(new ChangeEventArgs { Value = ViewModel.SectionIndex + 1 });
                        StateHasChanged();
                        await Task.Yield();
                    }
                    if (IsSearchingForNextError)
                    {
                        bool firstColumnHasErrorOrWarning = await HtmlService.FirstColumnHasErrorAsync();
                        if (firstColumnHasErrorOrWarning)
                            break;
                    }
                }
                else
                {
                    if (IsSearchingForNextError)
                    {
                        NotificationService.ClearAll();
                        NotificationService.ShowWarning("No more warnings or errors.", timeoutMs: 3000);
                        break;
                    }
                }
            }
            if (IsSearchingForNextError)
            {
                await SectionNumberElement.FocusAsync();
            }
        }
        finally
        {
            LoadingCount--;
            IsSearchingForNextError = false;
        }
        await Task.Delay(100);
    }

    private void WordClicked(MouseEventArgs e, WordReference wordReference)
    {
        ViewModel.HandleWordClicked(e.ShiftKey, e.AltKey, wordReference);
    }
}
