using ConvertImagesToText;
using DocumentsModel;
using Microsoft.AspNetCore.Components;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Services;

namespace WordsAnalysis.Components.Pages.SyncDocuments;

internal readonly record struct WordReferenceAndColumnIndex(WordReference WordReference, int ColumnIndex);

public class ViewModel
{
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
    public string? UndoActionDescription => UndoStack.Count == 0 ? null : $"Undo {UndoStack.Peek().Description}";

    private readonly IDialogService DialogService;
    private readonly Stack<(string Description, FeatureState State)> RedoStack = [];
    private readonly IDictionaryService DictionaryService;
    private readonly IHtmlService HtmlService;
    private readonly Dictionary<OcrBookInfoAndPageNumber, Guid> SavedPageVersions = [];
    private FeatureState State;
    private readonly EventCallback StateHasChanged;
    private readonly IToastService ToastService;
    private readonly Stack<(string Description, FeatureState State)> UndoStack = [];

    public ViewModel(
        FeatureState state,
        IDialogService dialogService,
        IDictionaryService dictionaryService,
        IHtmlService htmlService,
        IToastService toastService,
        EventCallback stateHasChanged)
    {
        State = state;
        DialogService = dialogService;
        DictionaryService = dictionaryService;
        HtmlService = htmlService;
        ToastService = toastService;
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
            ToastService.ShowError("Cannot add a word after a missing word, try going to the next word and adding a word before it.", timeout: 5000);
            return;
        }

        OcrPage page = edition.LoadedPages[existingWordReference.PageNumber].Page;
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        var content = new EditWordDialog.EditWordDialogContent(State.Editions[existingWordReference.BookInfo], existingWordReference, page.ImageWidth, page.ImageHeight, true);
        var dialog = await DialogService.ShowDialogAsync<EditWordDialog, EditWordDialog.EditWordDialogContent>(content, dialogParameters);

        FeatureState newFeatureState = State;

        newFeatureState = newFeatureState with {
            LastEditedColumnIndex = columnIndex,
            LastEditedEdition = existingWordReference.BookInfo
        };

        DialogResult result = await dialog.Result;
        if (result.Cancelled) return;
        var dialogResult = (EditWordDialog.EditWordDialogResult)result.Data!;
        newFeatureState = FeatureState.AddWord(newFeatureState, existingWordReference, dialogResult.Word!, dialogResult.After);
        SetNewStateWithUndo("Add word", newFeatureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged.InvokeAsync();
    }

    public async Task AlignSelectedWordsAsync()
    {
        FeatureState newFeatureState = State;
        newFeatureState = FeatureState.AlignSelectedWords(newFeatureState);
        SetNewStateWithUndo("Align editions", newFeatureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged.InvokeAsync();
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
        await StateHasChanged.InvokeAsync();
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

        while (true)
        {
            OcrPage page = State.Editions[wordReference.BookInfo].LoadedPages[wordReference.PageNumber].Page;
            DialogParameters dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
            EditWordDialog.EditWordDialogContent content = new EditWordDialog.EditWordDialogContent(State.Editions[wordReference.BookInfo], wordReference, page.ImageWidth, page.ImageHeight, false);
            IDialogReference dialog = await DialogService.ShowDialogAsync<EditWordDialog, EditWordDialog.EditWordDialogContent>(content, dialogParameters);

            State = State with {
                LastEditedColumnIndex = columnIndex,
                LastEditedEdition = wordReference.BookInfo
            };

            DialogResult result = await dialog.Result;
            EditWordDialog.EditWordDialogResult? dialogResult = result.Data as EditWordDialog.EditWordDialogResult;

            if (dialogResult?.Word != null)
            {
                FeatureState newFeatureState = State;
                newFeatureState = FeatureState.ReplaceWord(newFeatureState, wordReference, [dialogResult.Word]);
                SetNewStateWithUndo("Edit word", newFeatureState);
                await LoadRowDataAsync(SectionIndex);
            }

            EditWordDialog.NavigateDirection navigate = dialogResult?.Navigate ?? EditWordDialog.NavigateDirection.None;
            if (navigate == EditWordDialog.NavigateDirection.None) break;

            WordReference? nextRef = await FindAdjacentWordAsync(wordReference, navigate == EditWordDialog.NavigateDirection.Next);
            if (nextRef == null)
            {
                ToastService.ShowInfo(navigate == EditWordDialog.NavigateDirection.Next ? "No next word." : "No previous word.");
                break;
            }
            wordReference = nextRef;
        }
        await StateHasChanged.InvokeAsync();
    }

    private async Task<WordReference?> FindAdjacentWordAsync(WordReference current, bool forward)
    {
        EditionState edition = State.Editions[current.BookInfo];
        int absoluteIndex = edition.GetFirstWordIndexForPage(current.PageNumber) + current.WordIndex;
        int step = forward ? 1 : -1;
        int wordCount = edition.GetWordCount();

        while (true)
        {
            absoluteIndex += step;
            if (absoluteIndex < 0 || absoluteIndex >= wordCount) return null;

            int pageNumber = edition.GetPageNumberForWord(absoluteIndex);
            if (pageNumber < 1) return null;

            edition = await EditionState.EnsurePageLoadedAsync(edition, pageNumber);
            State = State with {
                Editions = State.Editions.SetItem(current.BookInfo, edition)
            };

            int relativeIndex = absoluteIndex - edition.GetFirstWordIndexForPage(pageNumber);
            OcrWord? word = edition.LoadedPages[pageNumber].Page.Words[relativeIndex];
            if (word != null)
                return new WordReference(current.BookInfo, pageNumber, relativeIndex);
        }
    }

    public IEnumerable<OcrBookInfoAndPageNumber> GetDirtyPages()
    {
        foreach (var edition in State.Editions.Values)
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
        return WordsAnalysis.AppLayer.Features.SyncDocuments.ColumnData.GetColumnWords(State.Editions, RowData, columnIndex);
    }

    private async Task<WordReferenceAndColumnIndex?> GetWordReferenceUnderMouseAsync()
    {
        WordGridLocation location = await HtmlService.GetWordGridLocationAsync();
        if (location == WordGridLocation.None) return null;
        WordReference wordReference = RowData[location.RowIndex].Words[location.ColumnIndex];
        int columnIndex = location.ColumnIndex;
        return new WordReferenceAndColumnIndex(wordReference, columnIndex);
    }

    public async Task InsertNullWordAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;

        WordReference wordReference = wordInfo.Value.WordReference;
        int columnIndex = wordInfo.Value.ColumnIndex;

        FeatureState newFeatureState = State;

        newFeatureState = FeatureState.AddWord(newFeatureState, wordReference, ocrWord: null, after: false);
        SetNewStateWithUndo("Add word", newFeatureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged.InvokeAsync();
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
        await StateHasChanged.InvokeAsync();
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
        await StateHasChanged.InvokeAsync();
    }

    public async Task KillTheRestOfThePageAsync()
    {
        FeatureState featureState = State;

        KeyValuePair<WordReference, string?>[] remainingText = featureState.GetFollowingTextOnPage();
        if (remainingText == null) return;

        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        EditionState editionState = featureState.Editions[remainingText[0].Key.BookInfo];
        var content = new DeleteWordsDialog.DeleteWordsDialogContent(editionState, remainingText);
        var dialog = await DialogService.ShowDialogAsync<DeleteWordsDialog, DeleteWordsDialog.DeleteWordsDialogContent>(content, dialogParameters);

        DialogResult result = await dialog.Result;
        if (result.Cancelled) return;
        var dialogResult = (DeleteWordsDialog.DeleteWordsDialogResult)result.Data!;

        featureState = FeatureState.DeleteWords(featureState, dialogResult.DeletedWords);
        SetNewStateWithUndo("Nuke rest of page", featureState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged.InvokeAsync();
    }

    public async Task RedoAsync()
    {
        if (RedoStack.TryPop(out var action))
        {
            UndoStack.Push((action.Description, State));
            State = action.State;
            await StateHasChanged.InvokeAsync();
        }
    }

    public async Task ReloadRowDataAsync() => await LoadRowDataAsync(SectionIndex);

    public async Task RescanAreaAsync()
    {
        WordReference selectedWordReference = SelectedWords.Single();
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        var content = new RescanAreaDialog.RescanAreaDialogContent(State.Editions[selectedWordReference.BookInfo], selectedWordReference);
        var dialog = await DialogService.ShowDialogAsync<RescanAreaDialog, RescanAreaDialog.RescanAreaDialogContent>(content, dialogParameters);
        DialogResult result = await dialog.Result;
        if (result.Cancelled) return;
        var dialogResult = (RescanAreaDialog.EditWordDialogResult)result.Data!;

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
        await StateHasChanged.InvokeAsync();
    }

    public async Task SaveChangesAsync()
    {
        var updatedPageVersions = new Dictionary<OcrBookInfoAndPageNumber, Guid>();

        var state = State;
        var tasks = GetDirtyPages().Select(async x =>
        {
            var edition = state.Editions[x.BookInfo];
            PageState pageState = edition.LoadedPages[x.PageNumber];
            OcrPage page = pageState.Page;

            await page.SaveAsync(AppLayer.Constants.Data.SourcesDirectoryPath, x.BookInfo);
            return (x, pageState);
        });
        await foreach (var task in Task.WhenEach(tasks))
        {
            (OcrBookInfoAndPageNumber key, PageState pageState) = task.Result;
            updatedPageVersions.Add(key, pageState.ContentsVersion);
        }
        // Replace all page versions
        foreach (var entry in updatedPageVersions)
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
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        var content = new ViewColumnImagesDialog.ViewColumnImagesDialogContent(Editions, wordReferences);
        await DialogService.ShowDialogAsync<ViewColumnImagesDialog, ViewColumnImagesDialog.ViewColumnImagesDialogContent>(content, dialogParameters);
    }

    public async Task SuggestSplitWordsAsync()
    {
        WordReferenceAndColumnIndex? wordInfo = await GetWordReferenceUnderMouseAsync();
        if (wordInfo == null) return;

        WordReference wordReference = wordInfo.Value.WordReference;
        int columnIndex = wordInfo.Value.ColumnIndex;

        EditionState edition = State.Editions[wordReference.BookInfo];
        OcrWord ocrWord = wordReference.GetWord(edition)!;
        if (ocrWord == null) return;
        if (ocrWord.IsComposite()) return;

        string displayText = ocrWord.GetDisplayText(showBenefitOfDoubt: false);

        string[][] suggestions = DictionaryService.SplitTextIntoWords(displayText).ToArray();
        if (!suggestions.Any()) return;

        OcrRect actualBounds = ocrWord.Elements[0].Bounds;

        OcrPage page = State.Editions[wordReference.BookInfo].LoadedPages[wordReference.PageNumber].Page;
        var splitWordSuggestions = new List<SplitWordsDialog.SplitWordSuggestion>();
        foreach (string[] words in suggestions)
        {
            int[] estimatedWordWidths = words.Select(x => OcrProcessor.EstimateWordSize(actualBounds.Height, x)).Select(x => x.Width).ToArray();
            int totalEstimatedCombinedTextWidth = estimatedWordWidths.Sum();
            double widthFactor = actualBounds.Width / (double)totalEstimatedCombinedTextWidth;
            int x = actualBounds.X;
            var splitWords = new List<SplitWordsDialog.SplitWord>(words.Length);
            for (int i = 0; i < words.Length; i++)
            {
                int adjustedWidth = (int)Math.Ceiling(estimatedWordWidths[i] * widthFactor);
                string text = words[i];
                var bounds = new OcrRect { X = x, Y = actualBounds.Y, Width = adjustedWidth, Height = actualBounds.Height };
                x += adjustedWidth;
                var splitWord = new SplitWordsDialog.SplitWord(text, bounds);
                splitWords.Add(splitWord);
            }
            splitWordSuggestions.Add(new SplitWordsDialog.SplitWordSuggestion(splitWords.ToArray()));
        }
        var content = new SplitWordsDialog.SplitWordsDialogContent(State.Editions[wordReference.BookInfo], wordReference, splitWordSuggestions.ToArray(), page.ImageWidth, page.ImageHeight);
        SplitWordsDialog.SplitWordSuggestion chosenSuggestion;
        if (splitWordSuggestions.Count == 1)
        {
            chosenSuggestion = splitWordSuggestions[0];
        }
        else
        {
            var dialogParameters = new DialogParameters();
            var dialog = await DialogService.ShowDialogAsync<SplitWordsDialog, SplitWordsDialog.SplitWordsDialogContent>(content, dialogParameters);

            DialogResult result = await dialog.Result;
            if (result.Cancelled) return;
            var dialogResult = (SplitWordsDialog.SplitWordsDialogResult)result.Data!;
            chosenSuggestion = dialogResult.Suggestion!;
        }

        var newOcrWords = chosenSuggestion.Words.Select(x => new OcrWord {
            Elements = [
                new OcrElement { Bounds = x.Bounds, Text = x.Text }
            ]
        }).ToArray();

        FeatureState newState = State;
        newState = FeatureState.ReplaceWord(newState, wordReference, newOcrWords);

        string description = "Split text into: " + string.Join(' ', chosenSuggestion.Words.Select(x => x.Text));
        SetNewStateWithUndo(description, newState);
        await LoadRowDataAsync(SectionIndex);
        await StateHasChanged.InvokeAsync();
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
        if (UndoStack.TryPop(out var action))
        {
            RedoStack.Push((action.Description, State));
            State = action.State;
            await StateHasChanged.InvokeAsync();
        }
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
                // Only add to saved page hashes if we don't already have it
                SavedPageVersions.TryAdd(key, pageState.ContentsVersion);
            }
        }
    }

}

