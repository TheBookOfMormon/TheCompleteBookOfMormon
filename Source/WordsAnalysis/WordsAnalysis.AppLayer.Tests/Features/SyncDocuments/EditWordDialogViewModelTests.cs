using DocumentsModel;
using NSubstitute;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.AppLayer.Services;
using WordsAnalysis.AppLayer.Tests.Helpers;

namespace WordsAnalysis.AppLayer.Tests.Features.SyncDocuments;

public class EditWordDialogViewModelTests
{
    private readonly OcrBookInfo _book1830 = TestDataBuilder.CreateBookInfo(1830, "Edition1830", "E1");

    private EditWordDialogViewModel CreateViewModel()
    {
        return new EditWordDialogViewModel();
    }

    private (EditWordDialogViewModel vm, EditWordDialogContent content) CreateInitializedViewModel(bool isAdd = false)
    {
        OcrWord word = TestDataBuilder.CreateWord("hello", x: 100, y: 50, width: 80, height: 25);
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState edition = TestDataBuilder.CreateEditionState(_book1830, page);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var content = new EditWordDialogContent(edition, wordRef, 1000, 800, isAdd);

        var vm = CreateViewModel();
        vm.SetContent(content);
        vm.Initialize(content, hasSampleImages: false);
        return (vm, content);
    }

    // --- GetActionName ---

    [Fact]
    public void GetActionName_WhenEdit_ReturnsEdit()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);

        string result = vm.GetActionName();

        Assert.Equal("Edit", result);
    }

    [Fact]
    public void GetActionName_WhenAdd_ReturnsAdd()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: true);

        string result = vm.GetActionName();

        Assert.Equal("Add", result);
    }

    // --- Initialize ---

    [Fact]
    public void Initialize_EditMode_SetsTextsFromWord()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);

        Assert.Single(vm.Texts);
        Assert.Equal("hello", vm.Texts[0].Text);
    }

    [Fact]
    public void Initialize_EditMode_SetsCorrectedFromWord()
    {
        OcrWord word = TestDataBuilder.CreateWord("test") with { Corrected = true };
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState edition = TestDataBuilder.CreateEditionState(_book1830, page);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var content = new EditWordDialogContent(edition, wordRef, 1000, 800, false);

        var vm = CreateViewModel();
        vm.SetContent(content);
        vm.Initialize(content, hasSampleImages: false);

        Assert.True(vm.Corrected);
    }

    [Fact]
    public void Initialize_AddMode_SetsEmptyText()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: true);

        Assert.Single(vm.Texts);
        Assert.Equal("", vm.Texts[0].Text);
    }

    [Fact]
    public void Initialize_SetsHasSampleImages()
    {
        OcrWord word = TestDataBuilder.CreateWord("hello");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState edition = TestDataBuilder.CreateEditionState(_book1830, page);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var content = new EditWordDialogContent(edition, wordRef, 1000, 800, false);

        var vm = CreateViewModel();
        vm.SetContent(content);
        vm.Initialize(content, hasSampleImages: true);

        Assert.True(vm.HasSampleImages);
    }

    // --- CreateWord ---

    [Fact]
    public void CreateWord_AssemblesWordFromCurrentState()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.Texts[0].Text = "modified";
        vm.Notes = "some note";
        vm.Corrected = true;

        OcrWord result = vm.CreateWord();

        Assert.Equal("modified", result.Elements[0].Text);
        Assert.Equal("some note", result.Notes);
        Assert.True(result.Corrected);
    }

    [Fact]
    public void CreateWord_EmptyNotes_SetsNotesToNull()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.Notes = "  ";

        OcrWord result = vm.CreateWord();

        Assert.Null(result.Notes);
    }

    [Fact]
    public void CreateWord_BenefitOfDoubtNone_ClearsBenefitOfDoubtText()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.BenefitOfDoubtSelectedOption = new KeyValuePair<BenefitOfDoubt, string>(BenefitOfDoubt.None, "None");
        vm.BenefitOfDoubtText = "some text";

        OcrWord result = vm.CreateWord();

        Assert.Null(result.BenefitOfDoubtText);
        Assert.Equal(BenefitOfDoubt.None, result.BenefitOfDoubt);
    }

    // --- ThresholdLowerChanged ---

    [Fact]
    public void ThresholdLowerChanged_WhenLowerExceedsUpper_AdjustsUpper()
    {
        var vm = CreateViewModel();
        vm.ThresholdLower = 200;
        vm.ThresholdUpper = 150;

        vm.ThresholdLowerChanged();

        Assert.Equal(201, vm.ThresholdUpper);
    }

    [Fact]
    public void ThresholdLowerChanged_WhenLowerEqualsUpper_AdjustsUpper()
    {
        var vm = CreateViewModel();
        vm.ThresholdLower = 150;
        vm.ThresholdUpper = 150;

        vm.ThresholdLowerChanged();

        Assert.Equal(151, vm.ThresholdUpper);
    }

    [Fact]
    public void ThresholdLowerChanged_WhenLowerBelowUpper_NoChange()
    {
        var vm = CreateViewModel();
        vm.ThresholdLower = 100;
        vm.ThresholdUpper = 200;

        vm.ThresholdLowerChanged();

        Assert.Equal(200, vm.ThresholdUpper);
    }

    // --- ThresholdUpperChanged ---

    [Fact]
    public void ThresholdUpperChanged_WhenUpperBelowLower_AdjustsLower()
    {
        var vm = CreateViewModel();
        vm.ThresholdUpper = 50;
        vm.ThresholdLower = 100;

        vm.ThresholdUpperChanged();

        Assert.Equal(49, vm.ThresholdLower);
    }

    [Fact]
    public void ThresholdUpperChanged_WhenUpperEqualsLower_AdjustsLower()
    {
        var vm = CreateViewModel();
        vm.ThresholdUpper = 100;
        vm.ThresholdLower = 100;

        vm.ThresholdUpperChanged();

        Assert.Equal(99, vm.ThresholdLower);
    }

    [Fact]
    public void ThresholdUpperChanged_WhenUpperAboveLower_NoChange()
    {
        var vm = CreateViewModel();
        vm.ThresholdUpper = 200;
        vm.ThresholdLower = 100;

        vm.ThresholdUpperChanged();

        Assert.Equal(100, vm.ThresholdLower);
    }

    // --- ResetLineHeightAdjustment ---

    [Fact]
    public void ResetLineHeightAdjustment_ResetsAllValues()
    {
        var vm = CreateViewModel();
        vm.LineHeightAdjustment = 10;
        vm.LineHeightLarger = true;
        vm.HasEstimatedSize = true;

        vm.ResetLineHeightAdjustment();

        Assert.Equal(0, vm.LineHeightAdjustment);
        Assert.False(vm.LineHeightLarger);
        Assert.False(vm.HasEstimatedSize);
    }

    // --- ConvertAmpersand ---

    [Fact]
    public void ConvertAmpersand_SetsBenefitOfDoubtToPrinterError()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.LineHeight = 25;

        vm.ConvertAmpersand();

        Assert.Equal(BenefitOfDoubt.PrinterError, vm.BenefitOfDoubtSelectedOption.Key);
    }

    [Fact]
    public void ConvertAmpersand_SetsTextToAmpersand()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.LineHeight = 25;

        vm.ConvertAmpersand();

        Assert.Equal("&", vm.Texts[0].Text);
    }

    [Fact]
    public void ConvertAmpersand_TogglesBenefitOfDoubtText()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.LineHeight = 25;
        vm.BenefitOfDoubtText = null;

        vm.ConvertAmpersand();
        // When BenefitOfDoubtText is null (not "and"), it toggles to "and"
        Assert.Equal("and", vm.BenefitOfDoubtText);

        vm.ConvertAmpersand();
        Assert.Equal("And", vm.BenefitOfDoubtText);

        vm.ConvertAmpersand();
        Assert.Equal("and", vm.BenefitOfDoubtText);
    }

    // --- DropFirstLetter ---

    [Fact]
    public void DropFirstLetter_RemovesFirstCharacter()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.Texts[0].Text = "hello";

        vm.DropFirstLetter(0);

        Assert.Equal("ello", vm.Texts[0].Text);
    }

    [Fact]
    public void DropFirstLetter_SingleCharacter_DoesNothing()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.Texts[0].Text = "h";

        vm.DropFirstLetter(0);

        Assert.Equal("h", vm.Texts[0].Text);
    }

    [Fact]
    public void DropFirstLetter_AdjustsBoundsXAndWidth()
    {
        var (vm, _) = CreateInitializedViewModel(isAdd: false);
        vm.Texts[0].Text = "hello";
        vm.LineHeight = 25;
        int originalX = vm.Texts[0].Bounds.X;
        int originalWidth = vm.Texts[0].Bounds.Width;

        vm.DropFirstLetter(0);

        Assert.True(vm.Texts[0].Bounds.X > originalX);
        Assert.True(vm.Texts[0].Bounds.Width < originalWidth);
    }

    // --- GetImageOptions ---

    [Fact]
    public void GetImageOptions_ShowHighContrastFalse_ReturnsNull()
    {
        var vm = CreateViewModel();
        vm.ShowHighContrast = false;

        var result = vm.GetImageOptions();

        Assert.Null(result);
    }

    [Fact]
    public void GetImageOptions_ShowHighContrastTrue_ReturnsOptions()
    {
        var vm = CreateViewModel();
        vm.ShowHighContrast = true;
        vm.ApplyThreshold = true;
        vm.ThresholdLower = 100;
        vm.ThresholdUpper = 200;

        var result = vm.GetImageOptions();

        Assert.NotNull(result);
        Assert.True(result.ShowHighContrast);
        Assert.True(result.ApplyThreshold);
        Assert.Equal(100, result.ThresholdLower);
        Assert.Equal(200, result.ThresholdUpper);
    }

    // --- ReadAppSettings / WriteAppSettings ---

    [Fact]
    public void ReadAppSettings_ReadsValuesFromPreferences()
    {
        var appPreferences = Substitute.For<IAppPreferences>();
        var editionPrefs = Substitute.For<IEditionPreferences>();
        var editWordDialogPrefs = Substitute.For<IEditWordDialogPreferences>();
        appPreferences.Editions.Returns(editionPrefs);
        appPreferences.EditWordDialog.Returns(editWordDialogPrefs);

        editionPrefs.GetLineHeight(_book1830).Returns(30);
        editWordDialogPrefs.ApplyThreshold.Returns(true);
        editWordDialogPrefs.ShowHighContrast.Returns(true);
        editWordDialogPrefs.ThresholdLower.Returns(80);
        editWordDialogPrefs.ThresholdUpper.Returns(200);

        OcrWord word = TestDataBuilder.CreateWord("test");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState edition = TestDataBuilder.CreateEditionState(_book1830, page);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var content = new EditWordDialogContent(edition, wordRef, 1000, 800, false);

        var vm = CreateViewModel();
        vm.ReadAppSettings(appPreferences, content);

        Assert.Equal(30, vm.LineHeight);
        Assert.True(vm.ApplyThreshold);
        Assert.True(vm.ShowHighContrast);
        Assert.Equal(80, vm.ThresholdLower);
        Assert.Equal(200, vm.ThresholdUpper);
    }

    [Fact]
    public void WriteAppSettings_WritesValuesToPreferences()
    {
        var appPreferences = Substitute.For<IAppPreferences>();
        var editionPrefs = Substitute.For<IEditionPreferences>();
        var editWordDialogPrefs = Substitute.For<IEditWordDialogPreferences>();
        appPreferences.Editions.Returns(editionPrefs);
        appPreferences.EditWordDialog.Returns(editWordDialogPrefs);

        OcrWord word = TestDataBuilder.CreateWord("test");
        OcrPage page = TestDataBuilder.CreatePageWithWords(1, word);
        EditionState edition = TestDataBuilder.CreateEditionState(_book1830, page);
        var wordRef = TestDataBuilder.CreateWordReference(_book1830, pageNumber: 1, wordIndex: 0);
        var content = new EditWordDialogContent(edition, wordRef, 1000, 800, false);

        var vm = CreateViewModel();
        vm.LineHeight = 42;
        vm.ApplyThreshold = true;
        vm.ShowHighContrast = true;
        vm.ThresholdLower = 90;
        vm.ThresholdUpper = 210;

        vm.WriteAppSettings(appPreferences, content);

        editionPrefs.Received(1).SetLineHeight(_book1830, 42);
        editWordDialogPrefs.Received(1).ApplyThreshold = true;
        editWordDialogPrefs.Received(1).ShowHighContrast = true;
        editWordDialogPrefs.Received(1).ThresholdLower = 90;
        editWordDialogPrefs.Received(1).ThresholdUpper = 210;
    }
}
