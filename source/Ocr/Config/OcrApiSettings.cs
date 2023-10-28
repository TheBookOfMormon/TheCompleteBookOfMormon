using System.ComponentModel.DataAnnotations;

namespace Ocr.Config;

internal class OcrApiSettings : IConfigSettings
{
    public static string GetSectionName() => "OcrApi";

    [Required]
    public string Key { get; init; } = null!;

    [Range(1, int.MaxValue)]
    public decimal MaxFileSizeInMB { get; init; } = 1;
}
