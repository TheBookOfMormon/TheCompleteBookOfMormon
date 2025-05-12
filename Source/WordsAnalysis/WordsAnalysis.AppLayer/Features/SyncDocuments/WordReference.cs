using DocumentsModel;
using System.Diagnostics;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

[DebuggerDisplay("Book {BookInfo}, Page {PageNumber}, Word {WordIndex}")]
public record WordReference(OcrBookInfo BookInfo, int PageNumber, int WordIndex) : IComparable<WordReference>
{
    public int CompareTo(WordReference? other)
    {
        if (other is null) return 1;

        int result = BookInfo.CompareTo(other.BookInfo);
        if (result != 0) return result;

        result = PageNumber.CompareTo(other.PageNumber);
        if (result != 0) return result;

        return WordIndex.CompareTo(other.WordIndex);
    }

    public string GetGlobalReference()
    {
        return $"{BookInfo.Year}{BookInfo.ShortCode}:{PageNumber}:{WordIndex}";
    }

    public OcrWord? GetWord(EditionState edition)
    {
        if (edition.LoadedPages.TryGetValue(PageNumber, out PageState? pageState) && pageState.Page.Words.Count > WordIndex)
            return pageState.Page.Words[WordIndex];
        return null;
    }
}
