using DocumentsModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Services;

namespace WordsAnalysis.Components.Pages.SyncDocuments;

public partial class Index : IDisposable
{
    private const string LastEditedCellClass = "--last-edited-cell";
    private const string LastEditedRowClass = "--last-edited-row";
    private bool IsSearchingForNextError;
    private int LoadingCount;
    private bool Loading => LoadingCount > 0;
    private ElementReference SectionNumberElement;
    private HashSet<OcrBookInfo> SelectedEditions = [];
    private bool ShowLoadingIndicator;
    private ViewModel ViewModel = null!;
    private WordReference? WordPreviouslySelected;

    [Inject]
    private IHtmlService HtmlService { get; set; } = null!;

    [Inject]
    private IToastService ToastService { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        ShowLoadingIndicator = true;
        LoadingCount++;
        await base.OnInitializedAsync();
        FeatureState state = await Task.Run(() => FeatureState.LoadAsync());
        var stateHasChangedCallback = EventCallback.Factory.Create(this, RefreshGrid);
        ViewModel = new ViewModel(state, DialogService, DictionaryService, HtmlService, ToastService, stateHasChangedCallback);
        await ViewModel.LoadRowDataAsync(0);
        LoadingCount--;
        ShowLoadingIndicator = false;
    }

    void IDisposable.Dispose()
    {
        StopSearchingForNextError();
    }

    private void StopSearchingForNextError()
    {
        IsSearchingForNextError = false;
    }

    private string GetEditionClass(OcrBookInfo bookInfo)
    {
        string result = "";
        if (ViewModel.LastEditedEdition == bookInfo)
            result = $"{result} {LastEditedRowClass}";
        if (SelectedEditions.Contains(bookInfo))
            result = $"{result} --selected";
        else
            result = $"{result} --not-selected";

        return result;
    }

    private string? GetWordStyle(OcrWord? word) =>
        (!ViewModel.ShowBenefitOfDoubt && word?.IsStrikethrough == true) ? "text-decoration: line-through" : null;

    private string GetZeroPaddedSectionNumber(int sectionNumber)
    {
        int length = ViewModel.SectionCount.ToString().Length;
        return string.Format("{0:D" + length + "}", sectionNumber);
    }

    private string GetWordClass(WordReference wordReference, string? displayText, int columnIndex)
    {
        ColumnData columnData = ViewModel.ColumnData[columnIndex];
        bool isEmpty = string.IsNullOrEmpty(displayText);
        string spacer = isEmpty ? "--spacer" : "";
        string selected = ViewModel.IsWordSelected(wordReference!) ? "--selected" : "";
        string lastEditedRow = ViewModel.LastEditedEdition == wordReference.BookInfo ? LastEditedRowClass : "";
        string outlier = "";
        string errorLevel = "";
        bool isFlagWord = displayText != null && (displayText == "{min}" || displayText.ToUpper().Contains("CHAPTER"));
        string firstWordOnPage = wordReference.WordIndex == 0 ? "first-word-on-page" : "";
        if (!isEmpty && (isFlagWord || columnData.MostCommonDisplayText != displayText))
        {
            outlier = "--outlier";
            if (isFlagWord || !string.Equals(columnData.MostCommonDisplayText, displayText, StringComparison.OrdinalIgnoreCase))
                errorLevel = "--error";
            else
                errorLevel = "--warning";
        }

        string lastEditedCell = wordReference.BookInfo == ViewModel.LastEditedEdition && columnIndex == ViewModel.LastEditedColumnIndex ? LastEditedCellClass : "";
        return $"{selected} {spacer} {lastEditedRow} {lastEditedCell} {errorLevel} {outlier} {firstWordOnPage}";
    }

    private string GetWordHint(WordReference wordReference)
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

    private string GetWordIndexClass(int index)
    {
        return ViewModel.ColumnData[index].ErrorLevel switch {
            ColumnDataErrorLevel.Error => "--error",
            ColumnDataErrorLevel.Warning => "--warning",
            ColumnDataErrorLevel.WordAddedOrRemoved => "--word-added-or-removed",
            ColumnDataErrorLevel.None => "",
            _ => throw new NotImplementedException()
        };
    }

    private async Task RefreshGrid()
    {
        await Task.Yield();
        StateHasChanged();
    }

    private async Task SaveChangesAsync()
    {
        await ViewModel.SaveChangesAsync();
        await SectionNumberElement.FocusAsync();
    }

    private async Task SelectedSectionIndexChanged(ChangeEventArgs e)
    {
        int newIndex = Convert.ToInt32(e.Value);
        LoadingCount++;
        StateHasChanged();
        await Task.Yield();
        await ViewModel.LoadRowDataAsync(newIndex);
        await HtmlService.ScrollBodyToTopLeftAsync();
        StateHasChanged();
        await Task.Yield();
        LoadingCount--;
    }

    private async Task ScrollToNextWarningOrErrorAsync()
    {
        IsSearchingForNextError = true;
        LoadingCount++;
        try
        {
            while (IsSearchingForNextError)
            {
                bool hasWarningOrError = await HtmlService.ScrollToNextErrorAsync();
                if (hasWarningOrError)
                    break;

                if (ViewModel.SectionIndex < ViewModel.SectionCount - 1)
                {
                    if (IsSearchingForNextError)
                    {
                        await SelectedSectionIndexChanged(new ChangeEventArgs { Value = ViewModel.SectionIndex + 1 });
                        StateHasChanged();
                        await Task.Yield();
                    }
                    if (IsSearchingForNextError)
                    {
                        bool firstColumnHasErrorOrWarning = await HtmlService.FirstColumnHasErrorAsync();
                        if (firstColumnHasErrorOrWarning)
                            break;
                    }
                }
                else
                {
                    if (IsSearchingForNextError)
                    {
                        ToastService.ClearAll();
                        ToastService.ShowWarning("No more warnings or errors.", timeout: 3000);
                        break;
                    }
                }
            }
            if (IsSearchingForNextError)
            {
                await SectionNumberElement.FocusAsync();
            }
        }
        finally
        {
            LoadingCount--;
            IsSearchingForNextError = false;
        }
        await Task.Delay(100); // Ignore accidental double-tap
    }

    private void ToggleEditionSelected(OcrBookInfo edition)
    {
        if (SelectedEditions.Contains(edition))
            SelectedEditions.Remove(edition);
        else
            SelectedEditions.Add(edition);
    }

    private void WordClicked(MouseEventArgs e, WordReference wordReference)
    {
        if (e.ShiftKey && WordPreviouslySelected == null) return;
        if (e.ShiftKey && WordPreviouslySelected != null)
        {
            ViewModel.SelectWordsInRange(WordPreviouslySelected!, wordReference);
            WordPreviouslySelected = null;
        }
        else if (!e.ShiftKey)
        {
            bool newlySelected = ViewModel.ToggleWordSelected(wordReference);
            if (newlySelected)
                WordPreviouslySelected = wordReference;
            else
                WordPreviouslySelected = null;
            if (e.AltKey && newlySelected && ViewModel.SelectedWords.Count > 1)
            {
                ViewModel.DeselectAll();
                ViewModel.ToggleWordSelected(wordReference);
            }
        }
    }
}