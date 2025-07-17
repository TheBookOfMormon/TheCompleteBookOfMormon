namespace DocumentsModel.Helpers;

public static class FilePathHelper
{
    public static string GetBookInfoFilePath(string sourcesDirectoryPath, string editionCode)
    {
        string editionDirectoryPath = GetEditionDirectoryPath(sourcesDirectoryPath, editionCode);
        string result = Path.Combine(editionDirectoryPath, DocumentsModel.Constants.OcrBookInfoFileName);
        return result;
    }

    public static string GetEditionDirectoryPath(string sourcesDirectoryPath, OcrBookInfo bookInfo)
    {
        string result = GetEditionDirectoryPath(sourcesDirectoryPath, bookInfo.Code);
        return result;
    }

    public static string GetEditionDirectoryPath(string sourcesDirectoryPath, string editionCode)
    {
        string result = Path.Combine(sourcesDirectoryPath, editionCode);
        return result;
    }

    public static string GetOcrDirectoryPath(string sourcesDirectoryPath, OcrBookInfo bookInfo)
    {
        string editionDirectoryPath = GetEditionDirectoryPath(sourcesDirectoryPath, bookInfo);
        string result = Path.Combine(editionDirectoryPath, Constants.OcrDirectoryName);
        return result;
    }

    public static string GetPageFilePath(string sourcesDirectoryPath, OcrBookInfo bookInfo, int pageNumber)
    {
        return GetPageOrPageMetaFilePath(sourcesDirectoryPath, bookInfo, pageNumber, Constants.PageFileNameExtension);
    }

    public static string GetPageMetaFilePath(string sourcesDirectoryPath, OcrBookInfo bookInfo, int pageNumber)
    {
        return GetPageOrPageMetaFilePath(sourcesDirectoryPath, bookInfo, pageNumber, Constants.PageMetaFileNameExtension);
    }

    public static string GetScansDeskewedImageFilePath(string sourcesDirectoryPath, OcrBookInfo bookInfo, int pageNumber)
    {
        string editionDirectoryPath = GetEditionDirectoryPath(sourcesDirectoryPath, bookInfo);
        string scansDeskewedImagesDirectoryPath = Path.Combine(editionDirectoryPath, Constants.ScansDeskewedDirectoryName);
        string fileName = GetFileName(pageNumber, ".tif");
        string result = Path.Combine(scansDeskewedImagesDirectoryPath, fileName);
        return result;
    }

    private static string GetFileName(int pageNumber, string fileExtension) =>
        Path.ChangeExtension($"{pageNumber:D3}", fileExtension);

    private static string GetPageOrPageMetaFilePath(string sourcesDirectoryPath, OcrBookInfo bookInfo, int pageNumber, string fileExtension)
    {
        string ocrDirectoryPath = GetOcrDirectoryPath(sourcesDirectoryPath, bookInfo);
        string fileName = GetFileName(pageNumber, fileExtension);
        string result = Path.Combine(ocrDirectoryPath, fileName);
        return result;
    }
}
