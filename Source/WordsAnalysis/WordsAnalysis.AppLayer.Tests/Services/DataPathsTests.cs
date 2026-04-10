using WordsAnalysis.AppLayer.Services;

namespace WordsAnalysis.AppLayer.Tests.Services;

public class DataPathsTests
{
    // --- Constructor with rootDataPath ---

    [Fact]
    public void Constructor_WithRootDataPath_SetsRootDataPath()
    {
        var dataPaths = new DataPaths("/test/data/path");

        Assert.Equal("/test/data/path", dataPaths.RootDataPath);
    }

    [Fact]
    public void Constructor_WithRootDataPath_SetsSourcesDirectoryPath()
    {
        var dataPaths = new DataPaths("/test/data/path");

        string expected = Path.Combine("/test/data/path", "Sources");
        Assert.Equal(expected, dataPaths.SourcesDirectoryPath);
    }

    [Theory]
    [InlineData("C:\\Data")]
    [InlineData("/home/user/data")]
    [InlineData("relative/path")]
    public void Constructor_VariousPaths_SourcesIsSubdirectory(string rootPath)
    {
        var dataPaths = new DataPaths(rootPath);

        Assert.Equal(rootPath, dataPaths.RootDataPath);
        Assert.Equal(Path.Combine(rootPath, "Sources"), dataPaths.SourcesDirectoryPath);
    }

    [Fact]
    public void Constructor_ImplementsIDataPaths()
    {
        var dataPaths = new DataPaths("/some/path");

        Assert.IsAssignableFrom<IDataPaths>(dataPaths);
    }

    [Fact]
    public void Constructor_WithTrailingSeparator_CombinesCorrectly()
    {
        string rootPath = Path.Combine("C:", "Data") + Path.DirectorySeparatorChar;
        var dataPaths = new DataPaths(rootPath);

        Assert.Equal(rootPath, dataPaths.RootDataPath);
        Assert.Equal(Path.Combine(rootPath, "Sources"), dataPaths.SourcesDirectoryPath);
    }
}
