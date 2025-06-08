using DocumentsModel.Helpers;
using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentsModel;

public record OcrPage
{
    public required int ImageHeight { get; init; }
    public required int ImageWidth { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ManuallyEdited { get; init; }

    public required int PageNumber { get; init; }
    public required ImmutableList<OcrWord?> Words { get; init; }

    public static OcrPage AddWord(OcrPage originalOcrPage, OcrWord? word, int wordIndex)
    {
        OcrPage newOcrPage = originalOcrPage;
        newOcrPage = newOcrPage with {
            Words = newOcrPage.Words.Insert(wordIndex, word)
        };
        return newOcrPage;
    }

    public static OcrPage DeleteWord(OcrPage originalOcrPage, int wordIndex)
    {
        OcrPage newOcrPage = originalOcrPage;
        newOcrPage = newOcrPage with {
            ManuallyEdited = true,
            Words = newOcrPage.Words.RemoveAt(wordIndex)
        };
        return newOcrPage;
    }

    public static OcrPage ReplaceWord(OcrPage originalOcrPage, int wordIndex, IEnumerable<OcrWord> newWords)
    {
        OcrPage newOcrPage = originalOcrPage;
        newOcrPage = newOcrPage with {
            ManuallyEdited = true,
            Words = newOcrPage.Words.RemoveAt(wordIndex)
        };
        foreach(var newWord in newWords.Reverse())
        {
            newOcrPage = newOcrPage with {
                Words = newOcrPage.Words.Insert(wordIndex, newWord)
            };
        }
        return newOcrPage;
    }

    public static async Task<OcrPage> LoadAsync(string sourcesDirectoryPath, OcrBookInfo bookInfo, int pageNumber)
    {
        string filePath = FilePathHelper.GetPageFilePath(sourcesDirectoryPath, bookInfo, pageNumber);
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = (await JsonSerializer.DeserializeAsync<OcrPage>(stream))!;
        return result;
    }

    public async Task SaveAsync(string sourcesDirectoryPath, OcrBookInfo bookInfo)
    {
        string filePath = FilePathHelper.GetPageFilePath(sourcesDirectoryPath, bookInfo, PageNumber);
        using var stream = File.Create(filePath);
        Task savePageTask = JsonSerializer.SerializeAsync(stream, this, Constants.DefaultJsonSerializerOptions);

        var pageMeta = new OcrPageMeta { PageNumber = PageNumber, NumberOfWords = Words.Count };
        var savePageMetaTask = pageMeta.SaveAsync(sourcesDirectoryPath, bookInfo);

        await Task.WhenAll([savePageTask, savePageMetaTask]);
    }
}
