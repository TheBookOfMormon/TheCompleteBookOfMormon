using DocumentsModel;

namespace ConvertImagesToText;

internal class EditionPages
{
    public string EditionCode { get; set; } = string.Empty;
    public OcrBookInfo BookInfo { get; set; } = null!;
    public string ScansDirectoryPath { get; set; } = string.Empty;
    public string ScansDeskewedDirectoryPath { get; set; } = string.Empty;
    public string OcrDirectoryPath { get; set; } = string.Empty;
    public string[] ImageFilePaths { get; set; } = null!;
    public int CompletedPageCount;
    public int TotalPageCount;
}
