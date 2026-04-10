using ConvertImagesToText;
using DocumentsModel;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Services;

namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

internal readonly record struct WordReferenceAndColumnIndex(WordReference WordReference, int ColumnIndex);

public class SyncDocumentsViewModel
{
    private const string LastEditedCellClass = "--last-edited-cell";
    private const string LastEditedRowClass = "--last-edited-row";

    public bool CanAlignSelectedWords => State.CanAlignSelectedWords();
    public bool CanDeletedWords => State.SelectedWords.Count > 0;
    public bool CanMergeWords => State.CanMergeWords();
    public bool CanNukeTheRestOfThePage => State.CanNukeTheRestOfThePage();
    public bool CanRescanArea => State.SelectedWords.Count == 1;
    public bool CanRedo => RedoStack.Count > 0;
    public bool CanUndo => UndoStack.Count > 0;
    public ImmutableArray<ColumnData> ColumnData => State.ColumnData;
    public ImmutableDictionary<OcrBookInfo, EditionState> Editions => State.Editions;
    public int FirstWordIndex => (SectionIndex * FeatureState.WordsInSection) + 1;
    public bool HasSelectedWords => SelectedWords.Any();
    public bool IsDirty => GetDirtyPages().Any();
    public int? LastEditedColumnIndex => State.LastEditedColumnIndex;
    public OcrBookInfo? LastEditedEdition => State.LastEditedEdition;
    public int MostWords => State.Editions.Values.Max(x => x.GetWordCount());
    public string? RedoActionDescription => RedoStack.Count == 0 ? null : $"Reapply {RedoStack.Peek().Description}";
    public ImmutableArray<RowData> RowData => State.RowData;
    public int SectionCount => (int)Math.Ceiling(MostWords / (double)FeatureState.WordsInSection);
    public int SectionIndex => State.SectionIndex;
    public bool ShowBenefitOfDoubt;
    public ImmutableHashSet<WordReference> SelectedWords => State.SelectedWords;
    public HashSet<OcrBookInfo> SelectedEditions { get; } = [];
    public string? UndoActionDescription => UndoStack.Count == 0 ? null : $"Undo {UndoStack.Peek().Description}";

    private readonly ISyncDocumentsDialogService DialogService;
    private readonly Stack<(string Description, FeatureState State)> RedoStack = [];
    private readonly IDictionaryService DictionaryService;
    private readonly IWordGridService WordGridService;
    private readonly IDataPaths DataPaths;
    private readonly INotificationService NotificationService;
    private readonly Dictionary<OcrBookInfoAndPageNumber, Guid> SavedPageVersions = [];
    private FeatureState State;
    private readonly Func<Task> StateHasChanged;
    private readonly Stack<(string Description, FeatureState State)> UndoStack = [];
    private WordReference? WordPreviouslySelected;

    public SyncDocumentsViewModel(
        FeatureState state,
        ISyncDocumentsDialogService dialogService,
        IDictionaryService dictionaryService,
        IWordGridService wordGridService,
        INotificationService notificationService,
        IDataPaths dataPaths,
        Func<Task> stateHasChanged)
    {
        State = state;
        DialogService = dialogService;
        DictionaryService = dictionaryService;
        WordGridService = wordGridService;
        NotificationService = notificationService;
        DataPaths = dataPaths;
        StateHasChanged = stateHasChanged;
    }

    public async Task AddWordAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;

        WordReference existingWordReference = wordInfo.Value.WordReference;
        int columnIndex = wordInfo.Value.ColumnIndex;

        EditionState edition = State.Editions[existingWordReference.BookInfo];
        OcrWord? word = existingWordReference.GetWord(edition);
        if (word == null)
        {
            NotificationService.ShowError("Cannot add a word after a missing word, try going to the next word and adding a word before it.", timeoutMs: 5000);
            return;
        }

        OcrPage page = edition.LoadedPages[existingWordReference.PageNumber].Page;
        var content = new EditWordDialogContent(State.Editions[existingWordReference.BookInfo], existingWordReference, page.ImageWidth, page.ImageHeight, true);
        EditWordDialogResult? dialogResult = await DialogService.ShowEditWordDialogAsync(content);

        FeatureState newFeatureState = State;

        newFeatureState = newFeatureState with {
            LastEditedColumnIndex = columnIndex,
            LastEditedEdition = existingWordReference.BookInfo
        };

        if (dialogResult == null) return;
        newFeatureState = FeatureState.AddWord(newFeatureState, existingWordReference, dialogResult.Word!, dialogResult.After);
        SetNewStateWithUndo("Add word", newFeatureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public async Task AlignSelectedWordsAsync()
    {
        FeatureState newFeatureState = State;
        newFeatureState = FeatureState.AlignSelectedWords(newFeatureState);
        SetNewStateWithUndo("Align editions", newFeatureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public async Task DeleteSelectedWordsAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;

        WordReference? wordReference = wordInfo?.WordReference;

        FeatureState newState = State;
        if (newState.SelectedWords.Count == 0 && wordReference != null)
            newState = FeatureState.SelectWord(newState, wordReference);

        int numberOfSelectedEditions = SelectedWords.Select(x => x.BookInfo).Distinct().Count();
        string description;
        if (numberOfSelectedEditions > 1)
            description = $"Delete {SelectedWords.Count} words from {numberOfSelectedEditions} editions";
        else
        {
            WordReference firstSelectedWordReference = newState.SelectedWords.First();
            if (newState.SelectedWords.Count > 1)
                description = $"Delete {SelectedWords.Count} words from {firstSelectedWordReference.BookInfo.Code}";
            else
            {
                OcrWord? selectedWord = firstSelectedWordReference.GetWord(newState.Editions[firstSelectedWordReference.BookInfo]);
                if (selectedWord == null)
                    description = $"Delete spacer word from {firstSelectedWordReference.BookInfo.Code}";
                else
                    description = $"Delete word {selectedWord.GetCombinedText()} from {firstSelectedWordReference.BookInfo.Code}";
            }
        }
        newState = FeatureState.DeleteSelectedWords(newState);
        SetNewStateWithUndo(description, newState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public void DeselectAll()
    {
        State = FeatureState.DeselectAll(State);
    }

    public async Task EditWordAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;
        if (wordInfo.Value.WordReference.GetWord(State.Editions[wordInfo.Value.WordReference.BookInfo]) == null) return;

        WordReference wordReference = wordInfo.Value.WordReference;
        int columnIndex = wordInfo.Value.ColumnIndex;

        OcrPage page = State.Editions[wordReference.BookInfo].LoadedPages[wordReference.PageNumber].Page;
        var content = new EditWordDialogContent(State.Editions[wordReference.BookInfo], wordReference, page.ImageWidth, page.ImageHeight, false);

        State = State with {
            LastEditedColumnIndex = columnIndex,
            LastEditedEdition = wordReference.BookInfo
        };

        EditWordDialogResult? dialogResult = await DialogService.ShowEditWordDialogAsync(content);
        if (dialogResult == null) return;
        FeatureState newFeatureState = State;
        newFeatureState = FeatureState.ReplaceWord(newFeatureState, wordReference, [dialogResult.Word!]);
        SetNewStateWithUndo("Edit word", newFeatureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public string GetEditionClass(OcrBookInfo bookInfo)
    {
        string result = "";
        if (LastEditedEdition == bookInfo)
            result = $"{result} {LastEditedRowClass}";
        if (SelectedEditions.Contains(bookInfo))
            result = $"{result} --selected";
        else
            result = $"{result} --not-selected";

        return result;
    }

    public string? GetWordStyle(OcrWord? word) =>
        (word?.Corrected == true) ? "text-decoration: line-through" : null;

    public string GetWordHint(WordReference wordReference)
    {
        return $"""
            Page {wordReference.PageNumber} Word {wordReference.WordIndex}
            =============
            Edit word (ALT E)
            Add word (ALT A)
            Delete word (ALT D)
            Reveal column word images (ALT R)
            Insert blank before word (ALT I)
            Merge composite word (ALT M)
            Split words (ALT X)
            Select column (ALT |)
            Mark words as editorial formatting change (ALT T)
            """;
    }

    public string GetWordClass(WordReference wordReference, string? displayText, int columnIndex)
    {
        ColumnData columnData = ColumnData[columnIndex];
        bool isEmpty = string.IsNullOrEmpty(displayText);
        string spacer = isEmpty ? "--spacer" : "";
        string selected = IsWordSelected(wordReference!) ? "--selected" : "";
        string lastEditedRow = LastEditedEdition == wordReference.BookInfo ? LastEditedRowClass : "";
        string outlier = "";
        string errorLevel = "";
        bool isFlagWord = displayText != null && (displayText == "{min}" || displayText == "{amp}" || displayText.ToUpper().Contains("CHAPTER"));
        string firstWordOnPage = wordReference.WordIndex == 0 ? "first-word-on-page" : "";
        if (!isEmpty && (isFlagWord || columnData.MostCommonDisplayText != displayText))
        {
            outlier = "--outlier";
            if (isFlagWord || !string.Equals(columnData.MostCommonDisplayText, displayText, StringComparison.OrdinalIgnoreCase))
                errorLevel = "--error";
            else
                errorLevel = "--warning";
        }

        string lastEditedCell = wordReference.BookInfo == LastEditedEdition && columnIndex == LastEditedColumnIndex ? LastEditedCellClass : "";
        return $"{selected} {spacer} {lastEditedRow} {lastEditedCell} {errorLevel} {outlier} {firstWordOnPage}";
    }

    public string GetWordIndexClass(int index)
    {
        return ColumnData[index].ErrorLevel switch {
            ColumnDataErrorLevel.Error => "--error",
            ColumnDataErrorLevel.Warning => "--warning",
            ColumnDataErrorLevel.WordAddedOrRemoved => "--word-added-or-removed",
            ColumnDataErrorLevel.None => "",
            _ => throw new NotImplementedException()
        };
    }

    public string GetZeroPaddedSectionNumber(int sectionNumber)
    {
        int length = SectionCount.ToString().Length;
        return string.Format("{0:D" + length + "}", sectionNumber);
    }

    public IEnumerable<OcrBookInfoAndPageNumber> GetDirtyPages()
    {
        foreach (EditionState edition in State.Editions.Values)
        {
            OcrBookInfo bookInfo = edition.BookInfo;
            foreach (PageState pageState in edition.LoadedPages.Values)
            {
                var key = new OcrBookInfoAndPageNumber(bookInfo, pageState.Page.PageNumber);
                if (SavedPageVersions.TryGetValue(key, out Guid savedVersion))
                {
                    if (pageState.ContentsVersion != savedVersion)
                        yield return key;
                }
            }
        }
    }

    public IEnumerable<OcrBookInfoAndPageNumber> GetVisiblePages() =>
        RowData
        .Select(x => new { x.BookInfo, Pages = x.Words.Select(x => x.PageNumber).Distinct() })
        .SelectMany(x => x.Pages, (x, page) => new OcrBookInfoAndPageNumber(x.BookInfo, page));

    public ImmutableArray<WordReference?> GetWordsInColumn(int columnIndex)
    {
        return SyncDocuments.ColumnData.GetColumnWords(State.Editions, RowData, columnIndex);
    }

    public void HandleWordClicked(bool shiftKey, bool altKey, WordReference wordReference)
    {
        if (shiftKey && WordPreviouslySelected == null) return;
        if (shiftKey && WordPreviouslySelected != null)
        {
            SelectWordsInRange(WordPreviouslySelected!, wordReference);
            WordPreviouslySelected = null;
        }
        else if (!shiftKey)
        {
            bool newlySelected = ToggleWordSelected(wordReference);
            if (newlySelected)
                WordPreviouslySelected = wordReference;
            else
                WordPreviouslySelected = null;
            if (altKey && newlySelected && SelectedWords.Count > 1)
            {
                DeselectAll();
                ToggleWordSelected(wordReference);
            }
        }
    }

    public async Task InsertNullWordAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;

        WordReference wordReference = wordInfo.Value.WordReference;

        FeatureState newFeatureState = State;

        newFeatureState = FeatureState.AddWord(newFeatureState, wordReference, ocrWord: null, after: false);
        SetNewStateWithUndo("Add word", newFeatureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public bool IsWordSelected(WordReference wordReference)
    {
        return State.IsWordSelected(wordReference);
    }

    public async Task LoadRowDataAsync(int sectionIndex)
    {
        FeatureState newState = State;
        newState = await FeatureState.GetWordsAsync(newState, sectionIndex, ShowBenefitOfDoubt);
        State = newState;
        UpdateSavedPageHashes(newState.RowData);
    }

    public async Task MarkSelectedWordsAsEditorialFormattingChangesAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        WordReference? wordReference = wordInfo?.WordReference;
        FeatureState newState = State;
        if (newState.SelectedWords.Count == 0 && wordReference != null)
            newState = FeatureState.SelectWord(newState, wordReference);

        newState = FeatureState.MarkSelectedWordsAsEditorialFormattingChanges(newState);
        SetNewStateWithUndo("Mark words as editorial formatting changes", newState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public async Task MergeSelectedWordsAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        WordReference? wordReference = wordInfo?.WordReference;
        FeatureState newState = State;
        if (newState.SelectedWords.Count == 0 && wordReference != null)
            newState = FeatureState.SelectWord(newState, wordReference);

        newState = FeatureState.MergeWords(newState);
        SetNewStateWithUndo("Composite words", newState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public async Task KillTheRestOfThePageAsync()
    {
        FeatureState featureState = State;

        KeyValuePair<WordReference, string?>[] remainingText = featureState.GetFollowingTextOnPage();
        if (remainingText == null) return;

        EditionState editionState = featureState.Editions[remainingText[0].Key.BookInfo];
        var content = new DeleteWordsDialogContent(editionState, remainingText);
        DeleteWordsDialogResult? dialogResult = await DialogService.ShowDeleteWordsDialogAsync(content);
        if (dialogResult == null) return;

        featureState = FeatureState.DeleteWords(featureState, dialogResult.DeletedWords);
        SetNewStateWithUndo("Nuke rest of page", featureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public async Task RedoAsync()
    {
        if (RedoStack.TryPop(out (string Description, FeatureState State) action))
        {
            UndoStack.Push((action.Description, State));
            State = action.State;
            await StateHasChanged();
        }
    }

    public async Task ReloadRowDataAsync() => await LoadRowDataAsync(SectionIndex);

    public async Task RescanAreaAsync()
    {
        WordReference selectedWordReference = SelectedWords.Single();
        var content = new RescanAreaDialogContent(State.Editions[selectedWordReference.BookInfo], selectedWordReference);
        RescanAreaDialogResult? dialogResult = await DialogService.ShowRescanAreaDialogAsync(content);
        if (dialogResult == null) return;

        IEnumerable<OcrWord> words = dialogResult.Words;

        FeatureState state = State;
        EditionState editionState = state.Editions[selectedWordReference.BookInfo];
        editionState = EditionState.AddWords(editionState, selectedWordReference, words);
        state = state with {
            Editions = state.Editions.SetItem(selectedWordReference.BookInfo, editionState)
        };
        state = FeatureState.DeselectAll(state);
        SetNewStateWithUndo("Rescan area of page", state);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public async Task SaveChangesAsync()
    {
        var updatedPageVersions = new Dictionary<OcrBookInfoAndPageNumber, Guid>();

        FeatureState state = State;
        IEnumerable<Task<(OcrBookInfoAndPageNumber, PageState)>> tasks = GetDirtyPages().Select(async x =>
        {
            EditionState edition = state.Editions[x.BookInfo];
            PageState pageState = edition.LoadedPages[x.PageNumber];
            OcrPage page = pageState.Page;

            await page.SaveAsync(DataPaths.SourcesDirectoryPath, x.BookInfo);
            return (x, pageState);
        });
        await foreach (Task<(OcrBookInfoAndPageNumber, PageState)> task in Task.WhenEach(tasks))
        {
            (OcrBookInfoAndPageNumber key, PageState pageState) = task.Result;
            updatedPageVersions.Add(key, pageState.ContentsVersion);
        }
        foreach (KeyValuePair<OcrBookInfoAndPageNumber, Guid> entry in updatedPageVersions)
            SavedPageVersions[entry.Key] = entry.Value;
    }

    public async Task SelectColumnAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;

        int columnIndex = wordInfo.Value.ColumnIndex;

        FeatureState newState = State;
        newState = FeatureState.DeselectAll(newState);
        var wordsToSelect = new List<WordReference>();
        foreach (RowData dataRow in RowData)
        {
            if (dataRow.Words.Count > columnIndex)
            {
                wordsToSelect.Add(dataRow.Words[columnIndex]);
            }
        }
        newState = FeatureState.SelectWords(newState, wordsToSelect);
        SetNewStateWithUndo("Select column", newState);
    }

    public void SelectWord(WordReference wordReference)
    {
        if (!IsWordSelected(wordReference))
            ToggleWordSelected(wordReference);
    }

    public void SelectWordsInRange(WordReference firstWordReference, WordReference lastWordReference)
    {
        FeatureState newState = State;
        (int firstColumnIndex, int firstRowIndex) = State.GetWordGridLocation(firstWordReference);
        (int lastColumnIndex, int lastRowIndex) = State.GetWordGridLocation(lastWordReference);
        if (firstColumnIndex != lastColumnIndex && firstRowIndex != lastRowIndex) return;

        if (firstRowIndex == lastRowIndex)
            newState = FeatureState.SelectWordRangeInEdition(newState, firstWordReference, lastWordReference);
        else
            newState = FeatureState.SelectWordRangeInColumn(newState, firstColumnIndex, firstWordReference.BookInfo, lastWordReference.BookInfo);
        State = newState;
    }

    public async Task ShowColumnImagesAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;

        int columnIndex = wordInfo.Value.ColumnIndex;

        ImmutableArray<WordReference?> wordReferences = GetWordsInColumn(columnIndex);
        var content = new ViewColumnImagesDialogContent(Editions, wordReferences);
        await DialogService.ShowViewColumnImagesDialogAsync(content);
    }

    public async Task SuggestSplitWordsAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;

        WordReference wordReference = wordInfo.Value.WordReference;

        EditionState edition = State.Editions[wordReference.BookInfo];
        OcrWord ocrWord = wordReference.GetWord(edition)!;
        if (ocrWord == null) return;
        if (ocrWord.IsComposite()) return;

        string displayText = ocrWord.GetDisplayText(showBenefitOfDoubt: false);

        string[][] suggestions = DictionaryService.SplitTextIntoWords(displayText).ToArray();
        if (!suggestions.Any()) return;

        OcrRect actualBounds = ocrWord.Elements[0].Bounds;

        OcrPage page = State.Editions[wordReference.BookInfo].LoadedPages[wordReference.PageNumber].Page;
        var splitWordSuggestions = new List<SplitWordSuggestion>();
        foreach (string[] words in suggestions)
        {
            int[] estimatedWordWidths = words.Select(x => OcrProcessor.EstimateWordSize(actualBounds.Height, x)).Select(x => x.Width).ToArray();
            int totalEstimatedCombinedTextWidth = estimatedWordWidths.Sum();
            double widthFactor = actualBounds.Width / (double)totalEstimatedCombinedTextWidth;
            int x = actualBounds.X;
            var splitWords = new List<SplitWord>(words.Length);
            for (int i = 0; i < words.Length; i++)
            {
                int adjustedWidth = (int)Math.Ceiling(estimatedWordWidths[i] * widthFactor);
                string text = words[i];
                var bounds = new OcrRect { X = x, Y = actualBounds.Y, Width = adjustedWidth, Height = actualBounds.Height };
                x += adjustedWidth;
                var splitWord = new SplitWord(text, bounds);
                splitWords.Add(splitWord);
            }
            splitWordSuggestions.Add(new SplitWordSuggestion(splitWords.ToArray()));
        }
        var content = new SplitWordsDialogContent(State.Editions[wordReference.BookInfo], wordReference, splitWordSuggestions.ToArray(), page.ImageWidth, page.ImageHeight);
        SplitWordSuggestion chosenSuggestion;
        if (splitWordSuggestions.Count == 1)
        {
            chosenSuggestion = splitWordSuggestions[0];
        }
        else
        {
            SplitWordsDialogResult? splitResult = await DialogService.ShowSplitWordsDialogAsync(content);
            if (splitResult == null) return;
            chosenSuggestion = splitResult.Suggestion!;
        }

        OcrWord[] newOcrWords = chosenSuggestion.Words.Select(x => new OcrWord {
            Elements = [
                new OcrElement { Bounds = x.Bounds, Text = x.Text }
            ]
        }).ToArray();

        FeatureState newState = State;
        newState = FeatureState.ReplaceWord(newState, wordReference, newOcrWords);

        string description = "Split text into: " + string.Join(' ', chosenSuggestion.Words.Select(x => x.Text));
        SetNewStateWithUndo(description, newState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged();
    }

    public void ToggleEditionSelected(OcrBookInfo edition)
    {
        if (SelectedEditions.Contains(edition))
            SelectedEditions.Remove(edition);
        else
            SelectedEditions.Add(edition);
    }

    public bool ToggleWordSelected(WordReference wordReference)
    {
        FeatureState newState = State;
        newState = FeatureState.ToggleWordSelected(newState, wordReference);
        State = newState;
        return newState.IsWordSelected(wordReference);
    }

    public async Task UndoAsync()
    {
        if (UndoStack.TryPop(out (string Description, FeatureState State) action))
        {
            RedoStack.Push((action.Description, State));
            State = action.State;
            await StateHasChanged();
        }
    }

    private async Task<WordReferenceAndColumnIndex?> GetWordReferenceUnderMouseAsync()
    {
        WordGridLocation location = await WordGridService.GetWordGridLocationAsync();
        if (location == WordGridLocation.None) return null;
        WordReference wordReference = RowData[location.RowIndex].Words[location.ColumnIndex];
        int columnIndex = location.ColumnIndex;
        return new WordReferenceAndColumnIndex(wordReference, columnIndex);
    }

    private void SetNewStateWithUndo(string description, FeatureState newState)
    {
        RedoStack.Clear();
        UndoStack.Push((description, State));
        State = newState;
    }

    private void UpdateSavedPageHashes(IEnumerable<RowData> rows)
    {
        foreach (RowData rowData in rows)
        {
            EditionState edition = State.Editions[rowData.BookInfo];
            OcrBookInfo bookInfo = edition.BookInfo;
            IEnumerable<int> pageNumbers = rowData.Words.Where(x => x != null).Select(x => x!.PageNumber).Distinct();
            foreach (int pageNumber in pageNumbers)
            {
                PageState pageState = edition.LoadedPages[pageNumber];
                var key = new OcrBookInfoAndPageNumber(bookInfo, pageNumber);
                SavedPageVersions.TryAdd(key, pageState.ContentsVersion);
            }
        }
    }
}
