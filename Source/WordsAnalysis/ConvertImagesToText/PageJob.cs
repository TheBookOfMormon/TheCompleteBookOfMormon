namespace ConvertImagesToText;

internal class PageJob
{
    public required EditionPages Edition { get; init; }
    public required string ImageFilePath { get; init; }
}
