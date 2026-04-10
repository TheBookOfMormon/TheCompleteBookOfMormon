using DocumentsModel;
using WordsAnalysis.AppLayer.Services;

namespace WordsAnalysis.Services;

internal class AppPreferences : IAppPreferences
{
    public IEditionPreferences Editions { get; } = new EditionPreferencesImpl();
    public IEditWordDialogPreferences EditWordDialog { get; } = new EditWordDialogPreferencesImpl();

    private class EditionPreferencesImpl : IEditionPreferences
    {
        public int GetLineHeight(OcrBookInfo bookInfo) => Preferences.Get(GetLineHeightKey(bookInfo), 12);
        public void SetLineHeight(OcrBookInfo bookInfo, int value) => Preferences.Set(GetLineHeightKey(bookInfo), value);

        private static string GetLineHeightKey(OcrBookInfo bookInfo) => $"{bookInfo.Code}-LineHeight";
    }

    private class EditWordDialogPreferencesImpl : IEditWordDialogPreferences
    {
        private const string Base = "EditWordDialog";
        private const string ApplyThresholdKey = $"{Base}-ApplyThreshold";
        private const string ShowHighContrastKey = $"{Base}-ShowHighContrast";
        private const string ThresholdLowerKey = $"{Base}-ThresholdLower";
        private const string ThresholdUpperKey = $"{Base}-ThresholdUpper";

        public bool ApplyThreshold { get => Preferences.Get(ApplyThresholdKey, false); set => Preferences.Set(ApplyThresholdKey, value); }
        public bool ShowHighContrast { get => Preferences.Get(ShowHighContrastKey, false); set => Preferences.Set(ShowHighContrastKey, value); }
        public int ThresholdLower { get => (byte)Preferences.Get(ThresholdLowerKey, 0); set => Preferences.Set(ThresholdLowerKey, value); }
        public int ThresholdUpper { get => (byte)Preferences.Get(ThresholdUpperKey, 100); set => Preferences.Set(ThresholdUpperKey, value); }
    }
}
