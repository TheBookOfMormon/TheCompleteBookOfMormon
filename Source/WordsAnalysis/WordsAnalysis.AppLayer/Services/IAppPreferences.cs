using DocumentsModel;

namespace WordsAnalysis.AppLayer.Services;

public interface IAppPreferences
{
    IEditionPreferences Editions { get; }
    IEditWordDialogPreferences EditWordDialog { get; }
}

public interface IEditionPreferences
{
    int GetLineHeight(OcrBookInfo bookInfo);
    void SetLineHeight(OcrBookInfo bookInfo, int value);
}

public interface IEditWordDialogPreferences
{
    bool ApplyThreshold { get; set; }
    bool ShowHighContrast { get; set; }
    int ThresholdLower { get; set; }
    int ThresholdUpper { get; set; }
}
