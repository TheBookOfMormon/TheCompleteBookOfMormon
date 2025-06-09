using DocumentsModel;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public record FeatureState
{
    public ImmutableArray<ColumnData> ColumnData { get; init; } = [];
    public required ImmutableDictionary<OcrBookInfo, EditionState> Editions { get; init; }
    public int? LastEditedColumnIndex { get; init; }
    public OcrBookInfo? LastEditedEdition { get; init; }
    public ImmutableArray<RowData> RowData { get; init; } = [];
    public required int SectionIndex { get; init; }
    public ImmutableHashSet<WordReference> SelectedWords { get; private init; } = [];
    public const int WordsInSection = 100;

    public static FeatureState AddWord(FeatureState originalFeatureState, WordReference existingWordReference, OcrWord? ocrWord, bool after)
    {
        FeatureState newFeatureState = originalFeatureState;
        EditionState newEditionState = originalFeatureState.Editions[existingWordReference.BookInfo];
        newEditionState = EditionState.AddWord(newEditionState, existingWordReference, ocrWord, after);
        newFeatureState = newFeatureState with {
            Editions = newFeatureState.Editions.SetItem(existingWordReference.BookInfo, newEditionState),
            LastEditedEdition = existingWordReference.BookInfo
        };
        return newFeatureState;
    }

    public static FeatureState AlignSelectedWords(FeatureState originalFeatureState)
    {
        FeatureState newFeatureState = originalFeatureState;
        Dictionary<OcrBookInfo, RowData> rowsByBookEdition = newFeatureState.RowData.ToDictionary(x => x.BookInfo);
        var indexsByWordReference = newFeatureState.SelectedWords
            .Select(x => new
            {
                WordReference = x,
                ColumnIndex = rowsByBookEdition[x.BookInfo].Words.IndexOf(x)
            })
            .ToArray();
        int highestColumnIndex = indexsByWordReference.Max(x => x.ColumnIndex);
        var offsetsByWordReference = indexsByWordReference
            .Select(x => new
            {
                WordReference = x.WordReference,
                Offset = highestColumnIndex - x.ColumnIndex
            });

        foreach (var item in offsetsByWordReference)
        {
            EditionState newEditionState = newFeatureState.Editions[item.WordReference.BookInfo];
            newEditionState = EditionState.AddSpacers(newEditionState, item.WordReference, false, item.Offset);
            newFeatureState = newFeatureState with {
                Editions = newFeatureState.Editions.SetItem(item.WordReference.BookInfo, newEditionState)
            };
        }
        return newFeatureState with { SelectedWords = [] };
    }

    public bool CanAlignSelectedWords()
    {
        if (SelectedWords.Count < 2) return false;
        if (SelectedWords.GroupBy(x => x.BookInfo).Any(x => x.Count() > 1)) return false;
        return true;
    }

    public bool CanMergeWords()
    {
        Tuple<WordReference, WordReference, WordReference>[] mergeableWords = GetMergeableWords();
        return mergeableWords.Any();
    }

    public bool CanNukeTheRestOfThePage()
    {
        if (SelectedWords.Count != 1) return false;
        return true;
    }

    public static FeatureState DeleteSelectedWords(FeatureState originalFeatureState)
    {
        return FeatureState.DeleteWords(originalFeatureState, originalFeatureState.SelectedWords);
    }

    public static FeatureState DeleteWords(FeatureState originalFeatureState, IEnumerable<WordReference> words)
    {
        if (!words.Any()) return originalFeatureState;

        FeatureState newState = originalFeatureState;
        var wordsGroupedByEdition = words.GroupBy(x => x.BookInfo);
        foreach (IGrouping<OcrBookInfo, WordReference> group in wordsGroupedByEdition)
        {
            EditionState newEditionState = newState.Editions[group.Key];
            newEditionState = EditionState.DeleteWords(newEditionState, group.AsEnumerable());
            newState = newState with {
                Editions = newState.Editions.SetItem(group.Key, newEditionState)
            };
        }
        return newState with {
            SelectedWords = []
        };
    }

    public static FeatureState DeselectAll(FeatureState newState)
    {
        newState = newState with { SelectedWords = [] };
        return newState;
    }

    public KeyValuePair<WordReference, string?>[] GetFollowingTextOnPage()
    {
        if (!CanNukeTheRestOfThePage()) return [];
        WordReference selectedWordReference = SelectedWords.Single();
        EditionState editionState = Editions[selectedWordReference.BookInfo];
        return editionState.GetFollowingTextOnPage(selectedWordReference);
    }


    public static async Task<FeatureState> GetWordsAsync(FeatureState originalFeatureState, int sectionIndex)
    {
        FeatureState newFeatureState = originalFeatureState;

        int firstWordIndex = sectionIndex * WordsInSection;
        var multipleWordsByBook = new Dictionary<OcrBookInfo, ImmutableList<WordReference>>();

        var tasks = newFeatureState.Editions.Values.Select(x => EditionState.GetWordsAsync(x, firstWordIndex, WordsInSection + 2));
        await foreach (var task in Task.WhenEach(tasks))
        {
            EditionState edition;
            ImmutableList<WordReference> words;

            (edition, words) = task.Result;
            newFeatureState = newFeatureState with {
                SectionIndex = sectionIndex,
                SelectedWords = sectionIndex != newFeatureState.SectionIndex ? [] : newFeatureState.SelectedWords,
                Editions = newFeatureState.Editions.SetItem(edition.BookInfo, edition)
            };
            multipleWordsByBook[edition.BookInfo] = words;
        }

        OcrBookInfo[] orderedBooks = multipleWordsByBook.Keys.OrderByDescending(x => x.Code).ToArray();
        var rowData = orderedBooks.Select(x => new RowData { BookInfo = x, Words = multipleWordsByBook[x] }).ToImmutableArray();
        newFeatureState = newFeatureState with {
            ColumnData = SyncDocuments.ColumnData.FromRowData(newFeatureState.Editions, rowData),
            RowData = rowData
        };

        return newFeatureState;
    }

    public (int ColumnIndex, int RowIndex) GetWordGridLocation(WordReference wordReference)
    {
        int rowIndex = -1;
        for (int i = 0; i < RowData.Length; i++)
        {
            if (RowData[i].BookInfo == wordReference.BookInfo)
            {
                rowIndex = i;
                break;
            }
        }
        int columnIndex = RowData[rowIndex].Words.IndexOf(wordReference);
        return (columnIndex, rowIndex);
    }

    public bool IsWordSelected(WordReference wordReference)
    {
        return SelectedWords.Contains(wordReference);
    }

    public static async Task<FeatureState> LoadAsync()
    {
        IEnumerable<string> editionCodes = Directory.GetFiles(Constants.Data.SourcesDirectoryPath, "index.json", SearchOption.AllDirectories).Select(x => Path.GetFileName(Path.GetDirectoryName(x))!);
        var tasks = editionCodes.Select(EditionState.LoadAsync);

        var editions = new List<EditionState>();
        await foreach (Task<EditionState> editionTask in Task.WhenEach(tasks))
        {
            EditionState editionMeta = editionTask.Result;
            editions.Add(editionMeta);
        }
        var newFeatureState = new FeatureState {
            SectionIndex = 0,
            Editions = editions.ToImmutableDictionary(x => x.BookInfo)
        };
        return newFeatureState;
    }

    public static FeatureState MergeWords(FeatureState featureState)
    {
        if (!featureState.CanMergeWords()) return featureState;
        Tuple<WordReference, WordReference, WordReference>[] mergeableWords = featureState.GetMergeableWords();

        foreach (var wordReferenceTuple in mergeableWords)
        {
            EditionState newEditionState = featureState.Editions[wordReferenceTuple.Item1.BookInfo];
            newEditionState = EditionState.MergeWords(newEditionState, wordReferenceTuple);
            featureState = featureState with {
                Editions = featureState.Editions.SetItem(newEditionState.BookInfo, newEditionState),
                SelectedWords = []
            };
        }
        return featureState;
    }

    public static FeatureState SelectWord(FeatureState originalFeatureState, WordReference wordReference)
    {
        if (originalFeatureState.IsWordSelected(wordReference))
            return originalFeatureState;

        FeatureState newFeatureState = originalFeatureState;
        newFeatureState = FeatureState.ToggleWordSelected(newFeatureState, wordReference);
        return newFeatureState;
    }

    public static FeatureState SelectWords(FeatureState originalFeatureState, IEnumerable<WordReference> wordsToSelect)
    {
        FeatureState newState = originalFeatureState;
        foreach(WordReference wordToSelect in wordsToSelect)
        {
            newState = FeatureState.SelectWord(newState, wordToSelect);
        }
        return newState;
    }


    public static FeatureState ToggleWordSelected(FeatureState originalFeatureState, WordReference wordReference)
    {
        FeatureState newFeatureState = originalFeatureState;

        ImmutableHashSet<WordReference> newSelectedWords;
        if (newFeatureState.IsWordSelected(wordReference))
            newSelectedWords = newFeatureState.SelectedWords.Remove(wordReference);
        else
            newSelectedWords = newFeatureState.SelectedWords.Add(wordReference);

        return newFeatureState with {
            SelectedWords = newSelectedWords
        };
    }

    public static FeatureState ReplaceWord(FeatureState originalFeatureState, WordReference wordReference, IEnumerable<OcrWord> newWords)
    {
        FeatureState newFeatureState = originalFeatureState;
        EditionState newEditionState = newFeatureState.Editions[wordReference.BookInfo];
        newEditionState = EditionState.ReplaceWord(newEditionState, wordReference, newWords);

        newFeatureState = newFeatureState with {
            Editions = newFeatureState.Editions.SetItem(wordReference.BookInfo, newEditionState),
            LastEditedEdition = wordReference.BookInfo
        };
        return newFeatureState;
    }

    public static FeatureState SelectWordRangeInColumn(FeatureState newState, int columnIndex, OcrBookInfo firstEdition, OcrBookInfo lastEdition)
    {
        if (firstEdition.CompareTo(lastEdition) == -1)
            (firstEdition, lastEdition) = (lastEdition, firstEdition);

        bool isSelecting = false;
        int rowIndex = -1;
        foreach (RowData rowData in newState.RowData)
        {
            rowIndex++;

            if (rowData.BookInfo == firstEdition)
                isSelecting = true;

            if (isSelecting)
            {
                if (columnIndex < rowData.Words.Count)
                {
                    WordReference wordReference = rowData.Words[columnIndex];
                    if (wordReference != null)
                        newState = FeatureState.SelectWord(newState, wordReference);
                }
            }

            if (rowData.BookInfo == lastEdition)
                break;
        }
        return newState;
    }

    public static FeatureState SelectWordRangeInEdition(FeatureState originalFeatureState, WordReference firstWordReference, WordReference lastWordReference)
    {
        if (firstWordReference.BookInfo != lastWordReference.BookInfo) return originalFeatureState;
        if (lastWordReference.CompareTo(firstWordReference) == -1)
            (firstWordReference, lastWordReference) = (lastWordReference, firstWordReference);

        FeatureState newFeatureState = originalFeatureState;
        EditionState newEditionState = newFeatureState.Editions[firstWordReference.BookInfo];
        int numberOfWordsOnCurrentPage = newEditionState.LoadedPages[firstWordReference.PageNumber].Page.Words.Count;
        WordReference currentWordReference = firstWordReference;
        newFeatureState = FeatureState.SelectWord(newFeatureState, lastWordReference);
        while (currentWordReference != lastWordReference)
        {
            newFeatureState = FeatureState.SelectWord(newFeatureState, currentWordReference);
            if (currentWordReference.WordIndex < numberOfWordsOnCurrentPage - 1)
                currentWordReference = currentWordReference with {
                    WordIndex = currentWordReference.WordIndex + 1
                };
            else
            {
                // Find the next page
                int pageNumber = currentWordReference.PageNumber + 1;
                while (pageNumber < 2000 && (!newEditionState.LoadedPages.TryGetValue(pageNumber, out PageState? pageState) || pageState.Page.Words.Count == 0))
                    pageNumber++;

                currentWordReference = currentWordReference with {
                    PageNumber = pageNumber,
                    WordIndex = 0
                };
                numberOfWordsOnCurrentPage = newEditionState.LoadedPages[currentWordReference.PageNumber].Page.Words.Count;
            }
        }
        return newFeatureState;
    }

    private Tuple<WordReference, WordReference, WordReference>[] GetMergeableWords()
    {
        if (SelectedWords.Count == 0) return [];
        var result = new List<Tuple<WordReference, WordReference, WordReference>>();
        foreach (WordReference selectedWordReference in SelectedWords)
        {
            EditionState editionState = Editions[selectedWordReference.BookInfo];
            (int columnIndex, int rowIndex) = GetWordGridLocation(selectedWordReference);

            RowData rowData = RowData[rowIndex];
            // Only allow merge if not in the last-2 or further
            if (columnIndex < 0 || columnIndex >= rowData.Words.Count - 2) continue;

            var tuple = new Tuple<WordReference, WordReference, WordReference>(
                rowData.Words[columnIndex],
                rowData.Words[columnIndex + 1],
                rowData.Words[columnIndex + 2]);
            if (!editionState.CanMergeWords(tuple)) continue;
            result.Add(tuple);
        }
        return result.ToArray();
    }

}
