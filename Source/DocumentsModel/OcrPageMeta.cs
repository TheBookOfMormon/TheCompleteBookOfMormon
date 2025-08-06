using DocumentsModel.Helpers;
using System.Text.Json;

namespace DocumentsModel;

public record OcrPageMeta
{
    public required int NumberOfWords { get; init; }
    public required int PageNumber { get; init; }

    public static async Task<OcrPageMeta> LoadAsync(string sourcesDirectoryPath, OcrBookInfo bookInfo, int pageNumber)
    {
        string filePath = FilePathHelper.GetPageMetaFilePath(sourcesDirectoryPath, bookInfo, pageNumber);

        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        OcrPageMeta result = (await JsonSerializer.DeserializeAsync(stream, ModelJsonContext.Default.OcrPageMeta))!;
        return result;
    }

    public async Task SaveAsync(string sourcesDirectoryPath, OcrBookInfo bookInfo)
    {
        string filePath = FilePathHelper.GetPageMetaFilePath(sourcesDirectoryPath, bookInfo, PageNumber);
        using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, this, ModelJsonContext.Default.OcrPageMeta);
    }

}
