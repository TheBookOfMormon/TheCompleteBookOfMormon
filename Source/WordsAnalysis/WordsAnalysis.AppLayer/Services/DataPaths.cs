namespace WordsAnalysis.AppLayer.Services;

public class DataPaths : IDataPaths
{
    public string RootDataPath { get; }
    public string SourcesDirectoryPath { get; }

    public DataPaths()
    {
        RootDataPath = Path.GetFullPath(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\..\..\..\..\Data"));
        SourcesDirectoryPath = Path.Combine(RootDataPath, "Sources");
    }

    public DataPaths(string rootDataPath)
    {
        RootDataPath = rootDataPath;
        SourcesDirectoryPath = Path.Combine(rootDataPath, "Sources");
    }
}
