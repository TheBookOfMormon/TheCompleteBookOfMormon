using DocumentsModel;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Extensions;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Extensions;
using WordsAnalysis.Services;

namespace WordsAnalysis.Components;

public partial class EditWordDialog : IAsyncDisposable
{

    [Parameter]
    public EditWordDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private EditForm EditForm = null!;
    private MagickImage FilteredPageImage = null!;
    private MagickImage PageImage = null!;
    private MagickImage PageDisplayImage => ViewModel.ShowHighContrast ? FilteredPageImage : PageImage;
    private string? PageImageData;
    private string PageImageFilePath => DocumentsModel.Helpers.FilePathHelper.GetScansDeskewedImageFilePath(AppLayer.Constants.Data.SourcesDirectoryPath, Content.WordReference.BookInfo, Content.WordReference.PageNumber);
    private string? WordImageData;

    private EditWordDialogViewModel ViewModel = null!;

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        PageImage?.Dispose();
        FilteredPageImage?.Dispose();
        return ValueTask.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
            await CenterImagePointAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        ViewModel = new EditWordDialogViewModel();
        ViewModel.SetContent(Content);
        ViewModel.ReadAppSettings(AppPreferences, Content);

        bool hasSampleImages = TextSamplesDialog.GetImageFilePaths(Content.Edition.BookInfo).Any();
        ViewModel.Initialize(Content, hasSampleImages);

        LoadPageImage();
    }

    private async Task CancelAsync()
    {
        var result = new EditWordDialogResult(null, false);
        await Dialog.CancelAsync(result);
    }

    private async Task CenterImagePointAsync(OcrRect? rect = null)
    {
        rect ??= ViewModel.Texts.Last(x => !x.IsOnNextPage).Bounds;
        (int x, int y) = rect.GetCenter();
        await HtmlService.CenterImagePointInParent("page-image", x, y);
    }

    private async Task ConfirmAsync()
    {
        if (!EditForm.EditContext!.Validate()) return;
        ViewModel.WriteAppSettings(AppPreferences, Content);
        ImageRepository.SetFilteredPageImage(PageImageFilePath, FilteredPageImage);

        OcrWord? newWord = ViewModel.CreateWord();

        var result = new EditWordDialogResult(newWord, ViewModel.AddWordAfter);
        await Dialog.CloseAsync(result);
    }

    private void ConvertAmpersand()
    {
        ViewModel.ConvertAmpersand();
        UpdateWordImageData();
    }

    private MagickImage CreateFilteredPageImage()
    {
        var result = new MagickImage(PageImage.Clone());
        result.ApplyImageOptions(ViewModel.GetImageOptions());
        FilteredPageImage = result;
        return result;
    }

    private void DropFirstLetter(int elementIndex)
    {
        ViewModel.DropFirstLetter(elementIndex);
        UpdateWordImageData();
    }

    private void EstimateWordSize(int elementIndex)
    {
        ViewModel.EstimateWordSize(elementIndex);
        UpdateWordImageData();
    }

    private void LoadPageImage()
    {
        if (PageImage != null)
            throw new InvalidOperationException("Page image already loaded.");

        PageImage = ImageRepository.GetPageImage(PageImageFilePath);
        FilteredPageImage = ImageRepository.GetFilteredPageImage(PageImageFilePath, CreateFilteredPageImage);
        UpdatePageImageData();
    }

    private async Task MoveAsync(MouseEventArgs e, int elementIndex, int xFactor, int yFactor)
    {
        var page = Content.Edition.LoadedPages[Content.WordReference.PageNumber].Page;
        CalculateMoveResult moveResult = ViewModel.CalculateMove(e.CtrlKey, e.ShiftKey, e.AltKey, elementIndex, xFactor, yFactor, page, Content.WordReference.WordIndex);
        UpdateWordImageData();

        if (moveResult.ShouldCenter)
            await CenterImagePointAsync(moveResult.NewBounds);
    }

    private async Task MoveDownAsync(MouseEventArgs e, int elementIndex)
    {
        await MoveAsync(e, elementIndex, 0, 1);
    }

    private async Task MoveLeftAsync(MouseEventArgs e, int elementIndex)
    {
        await MoveAsync(e, elementIndex, -1, 0);
    }

    private async Task MoveRightAsync(MouseEventArgs e, int elementIndex)
    {
        await MoveAsync(e, elementIndex, 1, 0);
    }

    private async Task MoveUpAsync(MouseEventArgs e, int elementIndex)
    {
        await MoveAsync(e, elementIndex, 0, -1);
    }

    private void PageFilterChanged()
    {
        FilteredPageImage?.Dispose();
        FilteredPageImage = CreateFilteredPageImage();
        UpdatePageImageData();
    }

    private async Task ShowTextSamplesAsync()
    {
        var content = new TextSamplesDialog.TextSamplesDialogContent { BookInfo = Content.WordReference.BookInfo };
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        await DialogService.ShowDialogAsync<TextSamplesDialog, TextSamplesDialog.TextSamplesDialogContent>(content, dialogParameters);
    }

    private void ThresholdLowerChanged()
    {
        ViewModel.ThresholdLowerChanged();
        PageFilterChanged();
    }

    private void ThresholdUpperChanged()
    {
        ViewModel.ThresholdUpperChanged();
        PageFilterChanged();
    }

    private void UpdatePageImageData()
    {
        PageImageData = PageDisplayImage.ToEmbeddedHtmlImage();
        UpdateWordImageData();
    }

    private void UpdateWordImageData()
    {
        OcrWord tempWord = ViewModel.CreateWord();
        using MagickImage lineImage = PageState.GetWordImage(PageDisplayImage, tempWord);
        WordImageData = lineImage.ToEmbeddedHtmlImage();
    }
}
