namespace WordsAnalysis.AppLayer.Constants;

// TODO: Migrate callers to use IDataPaths from DI instead. This static class will be removed in a future release.
public static class Data
{
    public static readonly string RootDataPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\..\..\Data"));
    public static readonly string SourcesDirectoryPath = Path.Combine(RootDataPath, "Sources");
}
