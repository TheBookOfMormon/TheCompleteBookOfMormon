namespace WordsAnalysis.AppLayer.Constants;

public static class Data
{
    public static readonly string RootDataPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\..\..\Data"));
    public static readonly string SourcesDirectoryPath = Path.Combine(RootDataPath, "Sources");
}
