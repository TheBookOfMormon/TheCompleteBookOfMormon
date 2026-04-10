using DocumentsModel;
using DocumentsModel.Helpers;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Extensions;

namespace WordsAnalysis.Components;

public partial class ViewColumnImagesDialog
{

    [Parameter]
    public ViewColumnImagesDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private ConcurrentDictionary<string, MagickImage> PageImages = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        PageImages.Clear();
    }

    private async Task ConfirmAsync()
    {
        await Dialog.CloseAsync();
    }

    private string? GetImageData(WordReference wordReference)
    {
        string sourceDirectoryPath = WordsAnalysis.AppLayer.Constants.Data.SourcesDirectoryPath;
        string imagePath = FilePathHelper.GetScansDeskewedImageFilePath(sourceDirectoryPath, wordReference.BookInfo, wordReference.PageNumber);
        MagickImage pageImage = PageImages.GetOrAdd(imagePath, path => new MagickImage(path));
        OcrWord? word = wordReference.GetWord(Content.Editions[wordReference.BookInfo]);
        if (word == null) return null;
        using MagickImage wordImage = PageState.GetWordImage(pageImage, word);
        return wordImage.ToEmbeddedHtmlImage();
    }

    //private void LoadPageImage()
    //{
    //    string imageFilePath = FilePathHelper.GetScansDeskewedImageFilePath(AppLayer.Constants.Data.SourcesDirectoryPath, Content.WordReference.BookInfo, Content.WordReference.PageNumber);
    //    PageImage = new MagickImage(imageFilePath);
    //    PageImageData = PageImage.ToEmbeddedHtmlImage();
    //    UpdateWordImageData();
    //}

    //private void UpdateWordImageData()
    //{
    //    OcrWord tempWord = CreateWord();
    //    using MagickImage lineImage = PageState.GetWordImage(PageImage, tempWord, ShowSurroundingText);
    //    WordImageData = lineImage.ToEmbeddedHtmlImage();
    //}
}
