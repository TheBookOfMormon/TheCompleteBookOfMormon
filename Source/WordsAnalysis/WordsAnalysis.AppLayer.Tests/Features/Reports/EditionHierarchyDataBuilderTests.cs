using DocumentsModel;
using WordsAnalysis.AppLayer.Features.Reports;
using WordsAnalysis.AppLayer.Tests.Helpers;

namespace WordsAnalysis.AppLayer.Tests.Features.Reports;

public class EditionHierarchyDataBuilderTests
{
    private readonly OcrBookInfo _book1830 = TestDataBuilder.CreateBookInfo(1830, "Edition1830", "E1");
    private readonly OcrBookInfo _book1837 = TestDataBuilder.CreateBookInfo(1837, "Edition1837", "E2");
    private readonly OcrBookInfo _book1840 = TestDataBuilder.CreateBookInfo(1840, "Edition1840", "E3");
    private readonly OcrBookInfo _book1849 = TestDataBuilder.CreateBookInfo(1849, "Edition1849", "E4");

    // --- Single edition ---

    [Fact]
    public void Build_SingleEdition_ReturnsRootWithNoChildren()
    {
        var data = new Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>>
        {
            [_book1830] = new Dictionary<OcrBookInfo, decimal>()
        };

        EditionHierarchyData result = EditionHierarchyDataBuilder.Build(data);

        Assert.Equal(_book1830, result.BookInfo);
        Assert.Empty(result.Children);
    }

    // --- Two editions ---

    [Fact]
    public void Build_TwoEditions_RootHasOneChild()
    {
        var data = new Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>>
        {
            [_book1830] = new Dictionary<OcrBookInfo, decimal>(),
            [_book1837] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 95m
            }
        };

        EditionHierarchyData result = EditionHierarchyDataBuilder.Build(data);

        Assert.Equal(_book1830, result.BookInfo);
        Assert.Single(result.Children);
        Assert.Equal(_book1837, result.Children[0].BookInfo);
    }

    // --- Linear chain A -> B -> C ---

    [Fact]
    public void Build_ThreeEditionsLinearDescent_FormsChain()
    {
        // 1837 is most similar to 1830 (90%)
        // 1840 is most similar to 1837 (95%) rather than 1830 (80%)
        var data = new Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>>
        {
            [_book1830] = new Dictionary<OcrBookInfo, decimal>(),
            [_book1837] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 90m
            },
            [_book1840] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 80m,
                [_book1837] = 95m
            }
        };

        EditionHierarchyData result = EditionHierarchyDataBuilder.Build(data);

        // Root is 1830
        Assert.Equal(_book1830, result.BookInfo);
        // 1830 has one child: 1837
        Assert.Single(result.Children);
        Assert.Equal(_book1837, result.Children[0].BookInfo);
        // 1837 has one child: 1840
        Assert.Single(result.Children[0].Children);
        Assert.Equal(_book1840, result.Children[0].Children[0].BookInfo);
    }

    // --- Branching: A -> B and A -> C ---

    [Fact]
    public void Build_BranchingHierarchy_BothChildrenUnderRoot()
    {
        // Both 1837 and 1840 are most similar to 1830
        var data = new Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>>
        {
            [_book1830] = new Dictionary<OcrBookInfo, decimal>(),
            [_book1837] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 90m
            },
            [_book1840] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 85m,
                [_book1837] = 70m
            }
        };

        EditionHierarchyData result = EditionHierarchyDataBuilder.Build(data);

        Assert.Equal(_book1830, result.BookInfo);
        Assert.Equal(2, result.Children.Count);
        var childBookInfos = result.Children.Select(c => c.BookInfo).ToHashSet();
        Assert.Contains(_book1837, childBookInfos);
        Assert.Contains(_book1840, childBookInfos);
    }

    // --- Hierarchy with deeper nesting ---

    [Fact]
    public void Build_FourEditions_FormsCorrectTree()
    {
        // 1837 -> 1830 (only option)
        // 1840 -> 1837 (more similar than 1830)
        // 1849 -> 1840 (most similar to 1840)
        var data = new Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>>
        {
            [_book1830] = new Dictionary<OcrBookInfo, decimal>(),
            [_book1837] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 90m
            },
            [_book1840] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 70m,
                [_book1837] = 95m
            },
            [_book1849] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 60m,
                [_book1837] = 80m,
                [_book1840] = 98m
            }
        };

        EditionHierarchyData result = EditionHierarchyDataBuilder.Build(data);

        // Tree: 1830 -> 1837 -> 1840 -> 1849
        Assert.Equal(_book1830, result.BookInfo);
        Assert.Single(result.Children);
        Assert.Equal(_book1837, result.Children[0].BookInfo);
        Assert.Single(result.Children[0].Children);
        Assert.Equal(_book1840, result.Children[0].Children[0].BookInfo);
        Assert.Single(result.Children[0].Children[0].Children);
        Assert.Equal(_book1849, result.Children[0].Children[0].Children[0].BookInfo);
    }

    // --- Root is always the earliest year ---

    [Fact]
    public void Build_RootIsAlwaysEarliestEdition()
    {
        var data = new Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>>
        {
            [_book1840] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 80m,
                [_book1837] = 90m
            },
            [_book1830] = new Dictionary<OcrBookInfo, decimal>(),
            [_book1837] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 85m
            }
        };

        EditionHierarchyData result = EditionHierarchyDataBuilder.Build(data);

        Assert.Equal(_book1830, result.BookInfo);
    }

    // --- Tie-breaking: when scores are equal, prefers earlier year ---

    [Fact]
    public void Build_EqualScores_PrefersEarlierYear()
    {
        // 1840 has equal similarity to both 1830 and 1837
        // Should pick 1830 (earlier year) as parent due to ThenBy(x => x.Key.Year)
        var data = new Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>>
        {
            [_book1830] = new Dictionary<OcrBookInfo, decimal>(),
            [_book1837] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 90m
            },
            [_book1840] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 90m,
                [_book1837] = 90m
            }
        };

        EditionHierarchyData result = EditionHierarchyDataBuilder.Build(data);

        // 1840 should be under 1830 (earlier year wins the tie)
        Assert.Equal(_book1830, result.BookInfo);
        var childBookInfos = result.Children.Select(c => c.BookInfo).ToList();
        Assert.Contains(_book1837, childBookInfos);
        Assert.Contains(_book1840, childBookInfos);
    }

    // --- Leaf nodes have no children ---

    [Fact]
    public void Build_LeafNodes_HaveNoChildren()
    {
        var data = new Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>>
        {
            [_book1830] = new Dictionary<OcrBookInfo, decimal>(),
            [_book1837] = new Dictionary<OcrBookInfo, decimal>
            {
                [_book1830] = 90m
            }
        };

        EditionHierarchyData result = EditionHierarchyDataBuilder.Build(data);

        Assert.Empty(result.Children[0].Children);
    }
}
