using DocumentsModel;
using NSubstitute;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.AppLayer.Services;
using WordsAnalysis.AppLayer.Tests.Helpers;

namespace WordsAnalysis.AppLayer.Tests.Features.SyncDocuments;

public class SyncDocumentsViewModelTests
{
    private readonly OcrBookInfo _book1830 = TestDataBuilder.CreateBookInfo(1830, "Edition1830", "E1");
    private readonly OcrBookInfo _book1837 = TestDataBuilder.CreateBookInfo(1837, "Edition1837", "E2");

    private readonly ISyncDocumentsDialogService _dialogService = Substitute.For<ISyncDocumentsDialogService>();
    private readonly IDictionaryService _dictionaryService = Substitute.For<IDictionaryService>();
    private readonly IWordGridService _wordGridService = Substitute.For<IWordGridService>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly IDataPaths _dataPaths = Substitute.For<IDataPaths>();

    private SyncDocumentsViewModel CreateViewModel(FeatureState state)
    {
        return new SyncDocumentsViewModel(
            state,
            _dialogService,
            _dictionaryService,
            _wordGridService,
            _notificationService,
            _dataPaths,
            () => Task.CompletedTask);
    }

    private FeatureState CreateTwoEditionState()
    {
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "the", "book", "of", "mormon");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "the", "Book", "of", "Mormon");
        return TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
    }

    // --- GetWordStyle ---

    [Fact]
    public void GetWordStyle_NullWord_ReturnsNull()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        string? result = vm.GetWordStyle(null);

        Assert.Null(result);
    }

    [Fact]
    public void GetWordStyle_WordNotCorrected_ReturnsNull()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        OcrWord word = TestDataBuilder.CreateWord("hello");

        string? result = vm.GetWordStyle(word);

        Assert.Null(result);
    }

    [Fact]
    public void GetWordStyle_CorrectedWord_ReturnsLineThrough()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        OcrWord word = TestDataBuilder.CreateWord("hello") with { Corrected = true };

        string? result = vm.GetWordStyle(word);

        Assert.Equal("text-decoration: line-through", result);
    }

    // --- GetWordHint ---

    [Fact]
    public void GetWordHint_ReturnsHintContainingPageAndWordInfo()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 5, wordIndex: 3);

        string result = vm.GetWordHint(wordRef);

        Assert.Contains("Page 5 Word 3", result);
    }

    [Fact]
    public void GetWordHint_ContainsKeyboardShortcutInfo()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830);

        string result = vm.GetWordHint(wordRef);

        Assert.Contains("Edit word (ALT E)", result);
        Assert.Contains("Add word (ALT A)", result);
        Assert.Contains("Delete word (ALT D)", result);
    }

    // --- GetZeroPaddedSectionNumber ---

    [Fact]
    public void GetZeroPaddedSectionNumber_PadsToCorrectWidth()
    {
        // With 4 words and WordsInSection=100, SectionCount=1 => 1 digit
        OcrPage page = TestDataBuilder.CreatePage(1, "a", "b", "c", "d");
        FeatureState state = TestDataBuilder.CreateFeatureState((_book1830, [page]));
        var vm = CreateViewModel(state);

        string result = vm.GetZeroPaddedSectionNumber(0);

        Assert.Equal("0", result);
    }

    [Fact]
    public void GetZeroPaddedSectionNumber_MultipleDigits_PadsCorrectly()
    {
        // Create enough words to have SectionCount > 9 (need > 900 words for 10 sections)
        // With WordsInSection=100, we need many words to get multi-digit section count
        // Create pages with enough words
        var words = Enumerable.Range(0, 50).Select(i => $"word{i}").ToArray();
        var pages = Enumerable.Range(1, 25).Select(i => TestDataBuilder.CreatePage(i, words)).ToArray();
        FeatureState state = TestDataBuilder.CreateFeatureState((_book1830, pages));
        var vm = CreateViewModel(state);

        // With 25 pages * 50 words = 1250 words, SectionCount = ceil(1250/100) = 13 => 2 digits
        string result = vm.GetZeroPaddedSectionNumber(3);

        Assert.Equal("03", result);
    }

    // --- GetEditionClass ---

    [Fact]
    public void GetEditionClass_NoSelectionNoLastEdited_ReturnsNotSelected()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        string result = vm.GetEditionClass(_book1830);

        Assert.Contains("--not-selected", result);
        Assert.DoesNotContain("--last-edited-row", result);
    }

    [Fact]
    public void GetEditionClass_EditionSelected_ReturnsSelectedClass()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        vm.ToggleEditionSelected(_book1830);

        string result = vm.GetEditionClass(_book1830);

        Assert.Contains("--selected", result);
        Assert.DoesNotContain("--not-selected", result);
    }

    [Fact]
    public void GetEditionClass_OtherEditionSelected_ReturnsNotSelected()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        vm.ToggleEditionSelected(_book1837);

        string result = vm.GetEditionClass(_book1830);

        Assert.Contains("--not-selected", result);
    }

    // --- ToggleEditionSelected ---

    [Fact]
    public void ToggleEditionSelected_AddsEditionToSet()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        vm.ToggleEditionSelected(_book1830);

        Assert.Contains(_book1830, vm.SelectedEditions);
    }

    [Fact]
    public void ToggleEditionSelected_RemovesAlreadySelectedEdition()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        vm.ToggleEditionSelected(_book1830);

        vm.ToggleEditionSelected(_book1830);

        Assert.DoesNotContain(_book1830, vm.SelectedEditions);
    }

    // --- ToggleWordSelected ---

    [Fact]
    public void ToggleWordSelected_SelectsUnselectedWord_ReturnsTrue()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);

        bool result = vm.ToggleWordSelected(wordRef);

        Assert.True(result);
        Assert.True(vm.IsWordSelected(wordRef));
    }

    [Fact]
    public void ToggleWordSelected_DeselectsSelectedWord_ReturnsFalse()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        vm.ToggleWordSelected(wordRef);

        bool result = vm.ToggleWordSelected(wordRef);

        Assert.False(result);
        Assert.False(vm.IsWordSelected(wordRef));
    }

    // --- DeselectAll ---

    [Fact]
    public void DeselectAll_ClearsAllSelectedWords()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef1 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var wordRef2 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 1);
        vm.ToggleWordSelected(wordRef1);
        vm.ToggleWordSelected(wordRef2);

        vm.DeselectAll();

        Assert.False(vm.HasSelectedWords);
    }

    // --- IsDirty ---

    [Fact]
    public void IsDirty_InitiallyFalse()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.False(vm.IsDirty);
    }

    // --- CanUndo / CanRedo ---

    [Fact]
    public void CanUndo_InitiallyFalse()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.False(vm.CanUndo);
    }

    [Fact]
    public void CanRedo_InitiallyFalse()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.False(vm.CanRedo);
    }

    [Fact]
    public void UndoActionDescription_WhenNoUndo_ReturnsNull()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.Null(vm.UndoActionDescription);
    }

    [Fact]
    public void RedoActionDescription_WhenNoRedo_ReturnsNull()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.Null(vm.RedoActionDescription);
    }

    // --- HandleWordClicked ---

    [Fact]
    public void HandleWordClicked_NoModifiers_TogglesWordSelection()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);

        vm.HandleWordClicked(shiftKey: false, altKey: false, wordRef);

        Assert.True(vm.IsWordSelected(wordRef));
    }

    [Fact]
    public void HandleWordClicked_NoModifiers_SecondClickDeselects()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);

        vm.HandleWordClicked(shiftKey: false, altKey: false, wordRef);
        vm.HandleWordClicked(shiftKey: false, altKey: false, wordRef);

        Assert.False(vm.IsWordSelected(wordRef));
    }

    [Fact]
    public void HandleWordClicked_AltClick_DeselectsOthersKeepsNewSelection()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef1 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var wordRef2 = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 1);

        // Select first word
        vm.HandleWordClicked(shiftKey: false, altKey: false, wordRef1);
        // Alt-click second word - should deselect first, keep second
        vm.HandleWordClicked(shiftKey: false, altKey: true, wordRef2);

        Assert.False(vm.IsWordSelected(wordRef1));
        Assert.True(vm.IsWordSelected(wordRef2));
    }

    [Fact]
    public void HandleWordClicked_ShiftWithNoPreviousSelection_DoesNothing()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);

        vm.HandleWordClicked(shiftKey: true, altKey: false, wordRef);

        Assert.False(vm.IsWordSelected(wordRef));
    }

    // --- Properties after construction ---

    [Fact]
    public void FirstWordIndex_AtSection0_Returns1()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.Equal(1, vm.FirstWordIndex);
    }

    [Fact]
    public void SectionIndex_MatchesState()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.Equal(0, vm.SectionIndex);
    }

    [Fact]
    public void Editions_MatchesState()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.Equal(2, vm.Editions.Count);
        Assert.True(vm.Editions.ContainsKey(_book1830));
        Assert.True(vm.Editions.ContainsKey(_book1837));
    }

    // --- GetWordIndexClass with loaded RowData ---

    [Fact]
    public async Task GetWordIndexClass_AfterLoadRowData_ReturnsExpectedClass()
    {
        // All editions have identical words so ErrorLevel should be None
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "the", "book");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "the", "book");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
        var vm = CreateViewModel(state);

        await vm.LoadRowDataAsync(0);

        // Identical words across editions => None error level => empty string
        string result = vm.GetWordIndexClass(0);
        Assert.Equal("", result);
    }

    [Fact]
    public async Task GetWordIndexClass_DifferentCaseWords_ReturnsWarning()
    {
        // Words differ in case only (e.g., "Book" vs "book") => Warning
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "book");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "Book");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
        var vm = CreateViewModel(state);

        await vm.LoadRowDataAsync(0);

        string result = vm.GetWordIndexClass(0);
        Assert.Equal("--warning", result);
    }

    [Fact]
    public async Task GetWordIndexClass_CompletelyDifferentWords_ReturnsError()
    {
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "hello");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "world");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
        var vm = CreateViewModel(state);

        await vm.LoadRowDataAsync(0);

        string result = vm.GetWordIndexClass(0);
        Assert.Equal("--error", result);
    }

    [Fact]
    public async Task GetWordIndexClass_OneEditionHasExtraWord_ReturnsWordAddedOrRemoved()
    {
        // One edition has 2 words, the other has 1. The second column has a word for one
        // edition and null for the other => WordAddedOrRemoved
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "the", "book");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "the");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
        var vm = CreateViewModel(state);

        await vm.LoadRowDataAsync(0);

        // Column 1 has "book" in one edition and null (missing) in the other
        string result = vm.GetWordIndexClass(1);
        Assert.Equal("--word-added-or-removed", result);
    }

    // --- GetWordClass with loaded RowData ---

    [Fact]
    public async Task GetWordClass_SelectedWord_ContainsSelectedClass()
    {
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "the", "book");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "the", "book");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
        var vm = CreateViewModel(state);
        await vm.LoadRowDataAsync(0);

        var wordRef = vm.RowData[0].Words[0];
        vm.ToggleWordSelected(wordRef);

        string result = vm.GetWordClass(wordRef, "the", 0);
        Assert.Contains("--selected", result);
    }

    [Fact]
    public async Task GetWordClass_EmptyDisplayText_ContainsSpacerClass()
    {
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "the");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "the");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
        var vm = CreateViewModel(state);
        await vm.LoadRowDataAsync(0);

        var wordRef = vm.RowData[0].Words[0];
        string result = vm.GetWordClass(wordRef, null, 0);

        Assert.Contains("--spacer", result);
    }

    [Fact]
    public async Task GetWordClass_FirstWordOnPage_ContainsFirstWordOnPageClass()
    {
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "the", "book");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "the", "book");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
        var vm = CreateViewModel(state);
        await vm.LoadRowDataAsync(0);

        var wordRef = vm.RowData[0].Words[0];
        string result = vm.GetWordClass(wordRef, "the", 0);

        Assert.Contains("first-word-on-page", result);
    }

    // --- ShowBenefitOfDoubt ---

    [Fact]
    public void ShowBenefitOfDoubt_DefaultsFalse()
    {
        FeatureState state = CreateTwoEditionState();
        var vm = CreateViewModel(state);

        Assert.False(vm.ShowBenefitOfDoubt);
    }

    // --- SectionCount ---

    [Fact]
    public void SectionCount_ReturnsCorrectValue()
    {
        OcrPage page = TestDataBuilder.CreatePage(1, "a", "b", "c", "d");
        FeatureState state = TestDataBuilder.CreateFeatureState((_book1830, [page]));
        var vm = CreateViewModel(state);

        // 4 words / 100 per section = ceil(0.04) = 1
        Assert.Equal(1, vm.SectionCount);
    }

    // --- GetEditionClass with LastEditedEdition ---

    [Fact]
    public void GetEditionClass_LastEditedEdition_ContainsLastEditedRowClass()
    {
        OcrPage page1830 = TestDataBuilder.CreatePage(1, "the");
        OcrPage page1837 = TestDataBuilder.CreatePage(1, "the");
        FeatureState state = TestDataBuilder.CreateFeatureState(
            (_book1830, [page1830]),
            (_book1837, [page1837]));
        state = state with { LastEditedEdition = _book1830 };
        var vm = CreateViewModel(state);

        string result = vm.GetEditionClass(_book1830);

        Assert.Contains("--last-edited-row", result);
    }
}
