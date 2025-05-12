using System.Text.Json;

namespace DocumentsModel;

public static class Constants
{
    public static readonly string ScansDirectoryName = "01-Scans";
    public static readonly string ScansDeskewedDirectoryName = "02-ScansDeskewed";
    public static readonly string OcrDirectoryName = "03-OCR";
    public static readonly string OcrBookInfoFileName = "index.json";
    public static readonly string PageMetaFileNameExtension = "PageMetaJson";
    public static readonly string PageFileNameExtension = "PageJson";

    public static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new JsonSerializerOptions {
        WriteIndented = true
    };
}
