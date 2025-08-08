using DocumentsModel;
using DocumentsModel.Helpers;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.Concurrent;
using WordsAnalysis.Extensions;

namespace WordsAnalysis.Components;
public partial class TextSamplesDialog
{
    public record TextSamplesDialogContent
    {
        public required OcrBookInfo BookInfo { get; init; }
    }

    [Parameter]
    public TextSamplesDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private ImageInfo[] Images = [];

    private struct ImageInfo
    {
        public required string FileName { get; init; }
        public required string ImageData { get; init; }
    }

    public static IEnumerable<string> GetImageFilePaths(OcrBookInfo bookInfo)
    {
        string directoryPath = FilePathHelper.GetSamplesDirectoryPath(AppLayer.Constants.Data.SourcesDirectoryPath, bookInfo);
        if (!Directory.Exists(directoryPath))
            return [];

        return Directory.EnumerateFiles(directoryPath, "*.jpg");
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        string[] imageFilePaths = GetImageFilePaths(Content.BookInfo).ToArray();
        var images = new ConcurrentBag<ImageInfo>();
        await Parallel.ForEachAsync(imageFilePaths, async (filePath, cancellationToken) =>
        {
            var image = new MagickImage();
            await image.ReadAsync(filePath);
            var info = new ImageInfo {
                FileName = Path.GetFileNameWithoutExtension(filePath),
                ImageData = image.ToEmbeddedHtmlImage()!
            };
            images.Add(info);
        });
        Images = images.OrderBy(x => x.FileName).ToArray();
    }

    private async Task ConfirmAsync()
    {
        await Dialog.CloseAsync();
    }
}