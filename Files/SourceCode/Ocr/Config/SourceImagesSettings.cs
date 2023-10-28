using System.ComponentModel.DataAnnotations;

namespace Ocr.Config;

internal class SourceImagesSettings : IConfigSettings
{
    public static string GetSectionName() => "SourceImages";

    [Required]
    public string Directory { get; init; } = null!;
}
