using System.ComponentModel.DataAnnotations;

namespace Ocr.Config;

internal class OcrSettings : IConfigSettings
{
    public static string GetSectionName() => "Ocr";

    [Required]
    public string ApiKey { get; init; } = null!;

    [Range(1, int.MaxValue)]
    public decimal ApiMaxFileSizeInMB { get; init; } = 1;

    [Required]
    public string ScanDirectory { get; init; } = null!;

    [Required]
    public string OcrDirectory {  get; init; } = null!;
}
