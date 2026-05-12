using DocumentsModel.Helpers;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DocumentsModel;

[DebuggerDisplay("{Year} {ShortCode}")]
public record OcrBookInfo : IComparable<OcrBookInfo>
{
    public required int Year { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string ShortCode { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? AscenderLetters { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double? AscenderHeightFactor { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? DescenderLetters { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public double? DescenderHeightFactor { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool HasSuperscripts { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool MultiColumn { get; init; }

    public PageRange[] ExcludedPages { get; init; } = [];

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
