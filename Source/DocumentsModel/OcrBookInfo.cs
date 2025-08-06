using DocumentsModel.Helpers;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentsModel;

[DebuggerDisplay("{Year} {ShortCode}")]
public record OcrBookInfo : IComparable<OcrBookInfo>
{
    public required int Year { get; set; }
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string ShortCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool HasSuperscripts { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool MultiColumn { get; set; }

    public PageRange[] ExcludedPages { get; set; } = [];

    public static async Task<OcrBookInfo> LoadAsync(string sourcesDirectoryPath, string editionCode)
    {
        string filePath = FilePathHelper.GetBookInfoFilePath(sourcesDirectoryPath, editionCode);
        using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
           
        OcrBookInfo result = (await JsonSerializer.DeserializeAsync(stream, ModelJsonContext.Default.OcrBookInfo))!;
        return result;
    }

    public int CompareTo(OcrBookInfo? other)
    {
        if (other == null) return 1;
        if (other.Year == Year)
            return Code.CompareTo(other.Code);
        return Year.CompareTo(other.Year);
    }
}
