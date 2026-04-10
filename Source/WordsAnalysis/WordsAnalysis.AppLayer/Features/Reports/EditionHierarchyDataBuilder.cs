using DocumentsModel;

namespace WordsAnalysis.AppLayer.Features.Reports;
internal static class EditionHierarchyDataBuilder
{
    public static EditionHierarchyData Build(Dictionary<OcrBookInfo, Dictionary<OcrBookInfo, decimal>> data)
    {
        Dictionary<OcrBookInfo, EditionHierarchyData> items = data
            .ToDictionary(
                x => x.Key,
                x => new EditionHierarchyData { BookInfo = x.Key });

        foreach (var edition in data.OrderBy(x => x.Key))
        {
            OcrBookInfo? baseEdition = edition.Value
                .Where(x => x.Key.Year < edition.Key.Year)
                .OrderByDescending(x => x.Value)
                .ThenBy(x => x.Key.Year)
                .Select(x => x.Key)
                .FirstOrDefault();
            if (baseEdition != null)
            {
                EditionHierarchyData currentEditionData = items[edition.Key];
                var baseEditionData = items[baseEdition];
                baseEditionData.Children.Add(currentEditionData);
            }
        }

        return items.OrderBy(x => x.Key.Year).Select(x => x.Value).First();
    }
}
