using DocumentsModel;

namespace WordsAnalysis.Services;

internal static class AppPreferences
{
    public static class Editions
    {
        public static int GetLineHeight(OcrBookInfo bookInfo) => Preferences.Get(GetLineHeightKey(bookInfo), 12);
        public static void SetLineHeight(OcrBookInfo bookInfo, int value) => Preferences.Set(GetLineHeightKey(bookInfo), value);

        private static string GetLineHeightKey(OcrBookInfo bookInfo) => $"{bookInfo.Code}-LineHeight";
    }

    public static class EditWordDialog
    {
        private const string Base = "EditWordDialog";
        private const string ApplyThresholdKey = $"{Base}-ApplyThreshold";
        private const string ShowHighContrastKey = $"{Base}-ShowHighContrast";
        private const string ShowSurroundingTextKey = $"{Base}-ShowSurroundingText";
        private const string ThresholdLowerKey = $"{Base}-ThresholdLower";
        private const string ThresholdUpperKey = $"{Base}-ThresholdUpper";

        public static bool ApplyThreshold { get => Preferences.Get(ApplyThresholdKey, false); set => Preferences.Set(ApplyThresholdKey, value); }
        public static bool ShowHighContrast { get => Preferences.Get(ShowHighContrastKey, false); set => Preferences.Set(ShowHighContrastKey, value); }
        public static bool ShowSurroundingText { get => Preferences.Get(ShowSurroundingTextKey, true); set => Preferences.Set(ShowSurroundingTextKey, value); }
        public static int ThresholdLower { get => (byte)Preferences.Get(ThresholdLowerKey, 0); set => Preferences.Set(ThresholdLowerKey, value); }
        public static int ThresholdUpper { get => (byte)Preferences.Get(ThresholdUpperKey, 100); set => Preferences.Set(ThresholdUpperKey, value); }
    }
}
