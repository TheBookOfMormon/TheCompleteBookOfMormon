using DocumentsModel;
using DocumentsModel.Helpers;
using System.Collections.Immutable;
using System.Diagnostics;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

[DebuggerDisplay("{BookInfo.Code}")]
public record EditionState
{
    public OcrBookInfo BookInfo { get; private init; }
    public ImmutableDictionary<int, PageState> LoadedPages { get; private init; }
    public ImmutableDictionary<int, int> PageNumberToWordIndex { get; private init; }
    public ImmutableList<int> WordIndexToPageNumber { get; private init; }

    public EditionState(OcrBookInfo bookInfo, IEnumerable<OcrPageMeta> pageMetas)
    {
        BookInfo = bookInfo;

        var pageMetasArray = pageMetas.ToArray();
        LoadedPages = ImmutableDictionary<int, PageState>.Empty;
        PageNumberToWordIndex = BuildPageNumberToWordIndex(pageMetasArray);
        WordIndexToPageNumber = BuildWordIndexToPageNumber(pageMetasArray);
    }

    public static EditionState AddWord(EditionState originalEditionState, WordReference existingWordReference, OcrWord? newWord, bool after)
    {
        return EditionState.AddWordInternal(originalEditionState, existingWordReference, newWord, after);
    }

    public static EditionState AddWords(EditionState editionState, WordReference selectedWordReference, IEnumerable<OcrWord> words)
    {
        foreach(OcrWord word in words.Reverse())
            editionState = EditionState.AddWordInternal(editionState, selectedWordReference, word, true);
        return editionState;
    }


    public static EditionState AddSpacers(EditionState originalEditionState, WordReference existingWordReference, bool after, int count)
    {
        if (count <= 0) return originalEditionState;

        EditionState newEditionState = originalEditionState;
        for(int i = 0; i < count; i++)
            newEditionState = EditionState.AddWordInternal(newEditionState, existingWordReference, null, after);
        return newEditionState;
    }

    public bool CanMergeWords(Tuple<WordReference, WordReference, WordReference> wordReferencesTuple)
    {
        IReadOnlyList<OcrWord?> ocrWords = GetWordsInOrder(wordReferencesTuple);
        if (ocrWords.Any(x => x == null || x.IsComposite())) return false;
        if (ocrWords[1]!.Elements[0].Text != "-") return false;
        return true;
    }

    public static EditionState DeleteWords(EditionState originalEditionState, IEnumerable<WordReference> wordReferences)
    {
        EditionState newEditionState = originalEditionState;
        ImmutableDictionary<int, int> newPageNumberToWordIndex = newEditionState.PageNumberToWordIndex;
        ImmutableList<int> newWordIndexToPageNumber = newEditionState.WordIndexToPageNumber;
        ImmutableDictionary<int, PageState> newLoadedPages = newEditionState.LoadedPages;

        wordReferences = wordReferences.OrderByDescending(x => x.PageNumber).ThenByDescending(x => x.WordIndex);
        foreach (WordReference wordReference in wordReferences)
        {
            int pageNumber = wordReference.PageNumber;
            int wordIndex = wordReference.WordIndex;

            PageState newPageState = newLoadedPages[pageNumber];
            OcrPage newOcrPage = newPageState.Page;
            newOcrPage = OcrPage.DeleteWord(newOcrPage, wordIndex);
            newPageState = new PageState(newOcrPage);
            newLoadedPages = newLoadedPages.SetItem(pageNumber, newPageState);

            int absoluteWordIndex = newPageNumberToWordIndex[pageNumber] + wordIndex;
            newWordIndexToPageNumber = newWordIndexToPageNumber.RemoveAt(absoluteWordIndex);

            // Every page after this word's page
            // TODO: Perhaps group by page number at the end and subtract number of removed words?
            IEnumerable<int> pageNumbersToModify = newPageNumberToWordIndex.Keys.Where(x => x > pageNumber);
            foreach (int pageNumberToModify in pageNumbersToModify)
                newPageNumberToWordIndex = newPageNumberToWordIndex.SetItem(pageNumberToModify, newPageNumberToWordIndex[pageNumberToModify] - 1);

            newEditionState = newEditionState with {
                LoadedPages = newLoadedPages,
                PageNumberToWordIndex = newPageNumberToWordIndex,
                WordIndexToPageNumber = newWordIndexToPageNumber
            };
        }

        return newEditionState;
    }

    public int GetFirstWordIndexForPage(int pageNumber)
    {
        return PageNumberToWordIndex[pageNumber];
    }

    public int GetPageNumberForWord(int wordIndex)
    {
        if (wordIndex < 0 || wordIndex >= WordIndexToPageNumber.Count)
            return -1;
        return WordIndexToPageNumber[wordIndex];
    }

    public static async Task<(EditionState edition, ImmutableList<WordReference> words)> GetWordsAsync(EditionState originalEditionState, int globalFirstWordIndex, int count)
    {
        EditionState newEditionState = originalEditionState;
        int lastWordIndex = globalFirstWordIndex + count - 1;
        int previousPageNumber = -1;
        var words = new List<WordReference>(count);
        OcrPage page = null!;
        for (int absoluteWordIndex = globalFirstWordIndex; absoluteWordIndex <= lastWordIndex; absoluteWordIndex++)
        {
            int pageNumber = newEditionState.GetPageNumberForWord(absoluteWordIndex);
            if (pageNumber < 1)
                break;

            if (pageNumber != previousPageNumber)
                (newEditionState, page) = await EditionState.LoadPageAsync(newEditionState, pageNumber);
            int indexOfFirstWordOnPage = newEditionState.GetFirstWordIndexForPage(pageNumber);
            int relativeWordIndex = absoluteWordIndex - indexOfFirstWordOnPage;
            var wordReference = new WordReference(newEditionState.BookInfo, page!.PageNumber, relativeWordIndex);
            words.Add(wordReference);
        }
        return (newEditionState, words.ToImmutableList());
    }

    public int GetWordCount() => WordIndexToPageNumber.Count;

    public static async Task<EditionState> LoadAsync(string editionCode)
    {
        OcrBookInfo bookInfo = (await OcrBookInfo.LoadAsync(Constants.Data.SourcesDirectoryPath, editionCode))!;

        string ocrDirectoryPath = FilePathHelper.GetOcrDirectoryPath(Constants.Data.SourcesDirectoryPath, bookInfo);
        IEnumerable<int> metaFilePaths = Directory.GetFiles(ocrDirectoryPath, $"*.{DocumentsModel.Constants.PageFileNameExtension}").Select(x => int.Parse(Path.GetFileNameWithoutExtension(x)!));
        var pageMetas = new List<OcrPageMeta>();
        IEnumerable<Task<OcrPageMeta>> tasks = metaFilePaths.Select(x => OcrPageMeta.LoadAsync(Constants.Data.SourcesDirectoryPath, bookInfo, x));
        await foreach (var item in Task.WhenEach(tasks))
        {
            OcrPageMeta meta = item.Result;
            pageMetas.Add(meta);
        }
        return new EditionState(bookInfo, pageMetas);
    }

    public static EditionState MergeWords(EditionState originalEditionState, Tuple<WordReference, WordReference, WordReference> wordReferences)
    {
        if (!originalEditionState.CanMergeWords(wordReferences)) return originalEditionState;

        EditionState newEditionState = originalEditionState;

        IReadOnlyList<OcrWord> ocrWords = newEditionState.GetWordsInOrder(wordReferences).OfType<OcrWord>().ToArray();
        bool isOnNextPage = wordReferences.Item1.PageNumber != wordReferences.Item2.PageNumber || wordReferences.Item2.PageNumber != wordReferences.Item3.PageNumber;
        OcrElement firstElement = ocrWords[0].Elements[0];
        OcrElement hyphenElement = ocrWords[1].Elements[0];
        OcrElement lastElement = ocrWords[2].Elements[0] with { IsOnNextPage = isOnNextPage };
        var newWord = new OcrWord { Elements = [firstElement, hyphenElement, lastElement] };

        newEditionState = EditionState.ReplaceWord(newEditionState, wordReferences.Item1, newWord);
        newEditionState = EditionState.DeleteWords(newEditionState, [wordReferences.Item2, wordReferences.Item3]);
        return newEditionState;
    }

    public static EditionState ReplaceWord(EditionState originalEditionState, WordReference wordReference, OcrWord newWord)
    {
        EditionState newEditionState = originalEditionState;
        PageState newPageState = newEditionState.LoadedPages[wordReference.PageNumber];
        OcrPage newOcrPage = newPageState.Page;
        newOcrPage = OcrPage.ReplaceWord(newOcrPage, wordReference.WordIndex, newWord);
        newPageState = new PageState(newOcrPage);
        newEditionState = newEditionState with { 
            LoadedPages = newEditionState.LoadedPages.SetItem(wordReference.PageNumber, newPageState)
        };
        return newEditionState;
    }

    public static EditionState AddWordInternal(EditionState editionState, WordReference existingWordReference, OcrWord? newWord, bool after)
    {
        OcrPage newPage = editionState.LoadedPages[existingWordReference.PageNumber].Page;
        int indexOffset = after ? 1 : 0;
        newPage = OcrPage.AddWord(newPage, newWord, existingWordReference.WordIndex + indexOffset);
        var newPageState = new PageState(newPage);

        ImmutableDictionary<int, int> newPageNumberToWordIndex = editionState.PageNumberToWordIndex;
        int absoluteWordIndex = newPageNumberToWordIndex[existingWordReference.PageNumber] + existingWordReference.WordIndex;
        var newWordIndexToPageNumber = editionState.WordIndexToPageNumber;

        if (after) absoluteWordIndex++;
        newWordIndexToPageNumber = newWordIndexToPageNumber.Insert(absoluteWordIndex, existingWordReference.PageNumber);

        // Every page after this word's page
        // TODO: Perhaps group by page number at the end and subtract number of removed words?
        IEnumerable<int> pageNumbersToModify = newPageNumberToWordIndex.Keys.Where(x => x > existingWordReference.PageNumber);
        foreach (int pageNumberToModify in pageNumbersToModify)
            newPageNumberToWordIndex = newPageNumberToWordIndex.SetItem(pageNumberToModify, newPageNumberToWordIndex[pageNumberToModify] + 1);

        editionState = editionState with {
            LoadedPages = editionState.LoadedPages.SetItem(existingWordReference.PageNumber, newPageState),
            WordIndexToPageNumber = newWordIndexToPageNumber,
            PageNumberToWordIndex = newPageNumberToWordIndex
        };
        return editionState;
    }

    public KeyValuePair<WordReference, string?>[] GetFollowingTextOnPage(WordReference selectedWordReference)
    {
        var words = new List<KeyValuePair<WordReference, string?>>();
        VisitRemainingWordsOnPage(selectedWordReference, (wordReference, word) => words.Add(new KeyValuePair<WordReference, string?>(wordReference, word)));
        if (words.Count == 0) return [];
        return words.ToArray();
    }

    public static EditionState NukeTheRestOfThePage(EditionState editionState, WordReference selectedWordReference)
    {
        var wordReferences = new List<WordReference>();
        editionState.VisitRemainingWordsOnPage(selectedWordReference, (wr, _) => wordReferences.Add(wr));
        return EditionState.DeleteWords(editionState, wordReferences);
    }

    private static ImmutableDictionary<int, int> BuildPageNumberToWordIndex(OcrPageMeta[] pageMetas)
    {
        int previousWords = 0;
        var result = new Dictionary<int, int>();
        foreach (OcrPageMeta pageMeta in pageMetas.OrderBy(x => x.PageNumber))
        {
            result[pageMeta.PageNumber] = previousWords;
            previousWords += pageMeta.NumberOfWords;
        }
        return result.ToImmutableDictionary();
    }

    private static ImmutableList<int> BuildWordIndexToPageNumber(OcrPageMeta[] pageMetas)
    {
        var result = new List<int>(pageMetas.Sum(x => x.NumberOfWords));
        var sortedMetas = pageMetas.OrderBy(x => x.PageNumber);
        foreach (var meta in sortedMetas)
            result.AddRange(Enumerable.Repeat(meta.PageNumber, meta.NumberOfWords));
        return result.ToImmutableList();
    }

    private void VisitRemainingWordsOnPage(WordReference selectedWordReference, Action<WordReference, string> action)
    {
        OcrPage page = LoadedPages[selectedWordReference.PageNumber].Page;
        IEnumerable<int> wordIndexes = Enumerable.Range(selectedWordReference.WordIndex, page.Words.Count - selectedWordReference.WordIndex);
        IEnumerable<(WordReference WordReference, string Text)> wordReferences = wordIndexes
            .Select(x => (selectedWordReference with { WordIndex = x }, page.Words[x]?.GetCombinedText()!))
            .Where(x => x.Item2 != null);
        foreach (var item in wordReferences)
            action(item.WordReference, item.Text);
    }


    private IReadOnlyList<OcrWord?> GetWordsInOrder(Tuple<WordReference, WordReference, WordReference> wordReferences)
    {
        WordReference[] wordReferencesArray = [wordReferences.Item1, wordReferences.Item2, wordReferences.Item3];
        return wordReferencesArray.OrderBy(x => x).Select(x => x.GetWord(this)).ToArray();
    }

    private static async Task<(EditionState edition, OcrPage page)> LoadPageAsync(EditionState originalEditionState, int pageNumber)
    {
        if (originalEditionState.LoadedPages.TryGetValue(pageNumber, out PageState? existingPageState))
            return (originalEditionState, existingPageState.Page);

        EditionState newEditionState = originalEditionState;

        var ocrPage = await OcrPage.LoadAsync(Constants.Data.SourcesDirectoryPath, newEditionState.BookInfo, pageNumber);
        newEditionState = newEditionState with {
            LoadedPages = newEditionState.LoadedPages.SetItem(pageNumber, new PageState(ocrPage))
        };
        return (newEditionState, ocrPage);
    }

}
