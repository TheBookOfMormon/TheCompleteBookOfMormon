using ConvertImagesToText;
using DocumentsModel;
using DocumentsModel.Helpers;
using ImageMagick;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.FluentUI.AspNetCore.Components;
using System.Collections.Immutable;
using WordsAnalysis.AppLayer.Extensions;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Extensions;
using WordsAnalysis.Services;

namespace WordsAnalysis.Components;

public partial class EditWordDialog : IAsyncDisposable
{
    public record EditWordDialogContent(
        EditionState Edition,
        WordReference WordReference,
        int PageWidth,
        int PageHeight,
        bool IsAdd,
        Func<OcrWord?, NavigateDirection, Task<(WordReference Reference, EditionState Edition)?>>? NavigateAsync = null,
        Func<OcrWord, Task<EditionState>>? SaveAsync = null,
        Func<OcrWord, bool, Task<(WordReference Reference, EditionState Edition)>>? InsertAsync = null,
        Func<Task<(WordReference Reference, EditionState Edition)?>>? DeleteAsync = null,
        Func<WordReference, string?>? GetMostCommonDisplayText = null);
    public record EditWordDialogResult(OcrWord? Word, bool After, bool IsInsert = false, bool IsDelete = false);

    public enum NavigateDirection { None, Previous, Next }

    [Parameter]
    public EditWordDialogContent Content { get; set; } = null!;

    [CascadingParameter]
    public FluentDialog Dialog { get; set; } = default!;

    private bool AddWordAfter = true;
    private bool ApplyThreshold;
    private KeyValuePair<BenefitOfDoubt, string> BenefitOfDoubtSelectedOption;
    private string? BenefitOfDoubtText;
    private bool Corrected;
    private bool Correction;
    private EditionState CurrentEdition = null!;
    private WordReference CurrentWordReference = null!;
    private EditForm EditForm = null!;
    private MagickImage FilteredPageImage = null!;
    private bool HasEstimatedSize;
    private bool HasSampleImages;
    private bool Inserted;
    private bool IsAddMode;
    private bool EnteredAddModeViaInsert;
    private EditionState? PreviousEdition;
    private WordReference? PreviousWordReference;
    private int LineHeight;
    private int LineHeightAdjustment;
    private bool LineHeightLarger;
    private string? Notes = "";
    private OcrRect OriginalBounds = OcrRect.Empty;
    private MagickImage PageImage = null!;
    private MagickImage PageDisplayImage => ShowHighContrast ? FilteredPageImage : PageImage;
    private string? PageImageData;
    private string PageImageFilePath => FilePathHelper.GetScansDeskewedImageFilePath(AppLayer.Constants.Data.SourcesDirectoryPath, CurrentWordReference.BookInfo, CurrentWordReference.PageNumber);
    private bool ShowDashes;
    private bool ShowHighContrast;
    private int ThresholdLower;
    private int ThresholdUpper;
    private TextData[] Texts = [];
    private OcrWord Word = null!;
    private string? WordImageData;

    private static readonly IEnumerable<BenefitOfDoubt> BenefitOfDoubtOptions;

    static EditWordDialog()
    {
        BenefitOfDoubtOptions = Enum.GetValues<BenefitOfDoubt>();
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        PageImage?.Dispose();
        FilteredPageImage?.Dispose();
        return ValueTask.CompletedTask;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
            await CenterImagePointAsync();
    }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        HasSampleImages = TextSamplesDialog.GetImageFilePaths(Content.Edition.BookInfo).Any();
        ReadAppSettings();

        CurrentWordReference = Content.WordReference;
        CurrentEdition = Content.Edition;

        if (Content.IsAdd)
        {
            LoadPageImage();
            LoadAddMode(AddWordAfter);
        }
        else
        {
            LoadWord(CurrentWordReference, CurrentEdition);
        }
    }

    private void LoadAddMode(bool after)
    {
        IsAddMode = true;
        AddWordAfter = after;
        ResetLineHeightAdjustment();

        OcrWord anchorWord = CurrentWordReference.GetWord(CurrentEdition)!;
        int gap = OcrProcessor.EstimateWordSize(LineHeight, "M").Width;
        OcrElement anchorElement;
        OcrRect newBounds;
        if (after)
        {
            anchorElement = anchorWord.LastElementOnSamePage();
            OcrRect bounds = anchorElement.Bounds;
            newBounds = bounds.Offset(bounds.Width + gap, 0) with { Width = LineHeight };
        }
        else
        {
            anchorElement = anchorWord.Elements[0];
            OcrRect bounds = anchorElement.Bounds;
            newBounds = bounds with { X = bounds.X - LineHeight - gap, Width = LineHeight };
        }
        Texts = [new TextData("", newBounds, false)];
        ShowDashes = false;
        Word = new OcrWord { Elements = [anchorElement with { Text = "" }] };
        OriginalBounds = Word.Elements[0].Bounds;
        Notes = null;
        Corrected = false;
        Correction = false;
        Inserted = false;
        BenefitOfDoubtSelectedOption = BenefitOfDoubtExtensions.GetOptions().First(x => x.Key == BenefitOfDoubt.None);
        BenefitOfDoubtText = null;

        UpdateWordImageData();
    }

    private void LoadWord(WordReference newRef, EditionState newEdition)
    {
        bool pageChanged = PageImage != null && CurrentWordReference.PageNumber != newRef.PageNumber;

        CurrentWordReference = newRef;
        CurrentEdition = newEdition;

        ResetLineHeightAdjustment();

        Word = CurrentWordReference.GetWord(CurrentEdition)!;
        Texts = Word.Elements.Select(x => new TextData(x.Text, x.Bounds, x.IsOnNextPage)).ToArray();
        ShowDashes = Word.ShowDashes;
        OriginalBounds = Word.Elements[0].Bounds;
        Notes = Word.Notes;
        Corrected = Word.Corrected;
        Correction = Word.Correction;
        Inserted = Word.Inserted;
        BenefitOfDoubtSelectedOption = BenefitOfDoubtExtensions.GetOptions().First(x => x.Key == Word.BenefitOfDoubt);
        BenefitOfDoubtText = Word.BenefitOfDoubtText;

        if (PageImage == null || pageChanged)
        {
            LoadPageImage();
        }
        else
        {
            UpdateWordImageData();
        }
    }

    private async Task CancelAsync()
    {
        if (EnteredAddModeViaInsert)
        {
            WordReference prev = PreviousWordReference!;
            EditionState prevEdition = PreviousEdition!;
            EnteredAddModeViaInsert = false;
            IsAddMode = false;
            PreviousWordReference = null;
            PreviousEdition = null;
            LoadWord(prev, prevEdition);
            StateHasChanged();
            await CenterImagePointAsync();
            return;
        }
        EditWordDialogResult result = new EditWordDialogResult(null, false);
        await Dialog.CancelAsync(result);
    }

    private async Task CenterImagePointAsync(OcrRect? rect = null)
    {
        rect ??= Texts.Last(x => !x.IsOnNextPage).Bounds;
        (int x, int y) = rect.GetCenter();
        await HtmlService.CenterImagePointInParent("page-image", x, y);
    }

    private async Task ConfirmAsync()
    {
        if (!EditForm.EditContext!.Validate()) return;
        WriteAppSettings();
        ImageRepository.SetFilteredPageImage(PageImageFilePath, FilteredPageImage);

        OcrWord newWord = CreateWord();

        EditWordDialogResult result = new EditWordDialogResult(newWord, AddWordAfter, IsInsert: IsAddMode);
        await Dialog.CloseAsync(result);
    }

    private bool IsDirty() => !Word.Equals(CreateWord());

    private async Task PreviousAsync() => await NavigateAsync(NavigateDirection.Previous);

    private async Task NextAsync() => await NavigateAsync(NavigateDirection.Next);

    private async Task InsertBeforeAsync() => await InsertAsync(after: false);

    private async Task InsertAfterAsync() => await InsertAsync(after: true);

    private async Task DeleteAsync()
    {
        if (IsAddMode) return;
        if (Content.DeleteAsync is null) return;

        ConfirmDialogContent confirmContent = new ConfirmDialogContent("Delete this word?");
        DialogParameters confirmParams = new DialogParameters();
        IDialogReference confirmDialog = await DialogService.ShowDialogAsync<ConfirmDialog, ConfirmDialogContent>(confirmContent, confirmParams);
        DialogResult confirmResult = await confirmDialog.Result;
        if (confirmResult.Cancelled) return;
        if (confirmResult.Data is not bool confirmed || !confirmed) return;

        WriteAppSettings();
        EditWordDialogResult deleteResult = new EditWordDialogResult(null, false, IsDelete: true);
        await Dialog.CloseAsync(deleteResult);
    }

    private async Task InsertAsync(bool after)
    {
        if (IsAddMode) return;
        if (Content.SaveAsync is null) return;

        if (IsDirty())
        {
            SaveChangesDialogResult choice = await PromptSaveChangesAsync();
            if (choice == SaveChangesDialogResult.Abort) return;
            if (choice == SaveChangesDialogResult.Yes)
            {
                if (!EditForm.EditContext!.Validate()) return;
                WriteAppSettings();
                ImageRepository.SetFilteredPageImage(PageImageFilePath, FilteredPageImage);
                OcrWord saved = CreateWord();
                CurrentEdition = await Content.SaveAsync(saved);
            }
        }

        PreviousWordReference = CurrentWordReference;
        PreviousEdition = CurrentEdition;
        EnteredAddModeViaInsert = true;
        LoadAddMode(after);
        StateHasChanged();
    }

    private async Task NavigateAsync(NavigateDirection direction)
    {
        if (Content.NavigateAsync is null) return;

        OcrWord? wordToSave = null;
        if (IsDirty())
        {
            SaveChangesDialogResult choice = await PromptSaveChangesAsync();
            if (choice == SaveChangesDialogResult.Abort) return;
            if (choice == SaveChangesDialogResult.Yes)
            {
                if (!EditForm.EditContext!.Validate()) return;
                WriteAppSettings();
                ImageRepository.SetFilteredPageImage(PageImageFilePath, FilteredPageImage);
                wordToSave = CreateWord();
            }
        }

        (WordReference Reference, EditionState Edition)? next = await Content.NavigateAsync(wordToSave, direction);

        if (next is null)
        {
            await CancelAsync();
            return;
        }

        LoadWord(next.Value.Reference, next.Value.Edition);
        StateHasChanged();
        await CenterImagePointAsync();
    }

    private async Task<SaveChangesDialogResult> PromptSaveChangesAsync()
    {
        SaveChangesDialogContent dialogContent = new SaveChangesDialogContent("Save changes to this word before navigating?");
        DialogParameters dialogParameters = new DialogParameters();
        IDialogReference dialog = await DialogService.ShowDialogAsync<SaveChangesDialog, SaveChangesDialogContent>(dialogContent, dialogParameters);
        DialogResult result = await dialog.Result;
        if (result.Data is SaveChangesDialogResult choice) return choice;
        return SaveChangesDialogResult.Abort;
    }

    private void ConvertAmpersand()
    {
        BenefitOfDoubtSelectedOption = BenefitOfDoubtExtensions.GetOptions().First(x => x.Key == BenefitOfDoubt.PrinterError);
        BenefitOfDoubtText = BenefitOfDoubtText == "and" ? "And" : "and";
        Texts[0].Text = "M";
        EstimateWordSize(0);
        Texts[0].Text = "&";
    }

    private MagickImage CreateFilteredPageImage()
    {
        var result = new MagickImage(PageImage.Clone());
        result.ApplyImageOptions(GetImageOptions());
        FilteredPageImage = result;
        return result;
    }

    private OcrWord CreateWord()
    {
        ImmutableList<OcrElement> newElements = Texts.Select(x => new OcrElement { Text = x.Text, Bounds = x.Bounds, IsOnNextPage = x.IsOnNextPage }).ToImmutableList();
        OcrWord result = Word with {
            Elements = newElements,
            Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
            Corrected = Corrected,
            Correction = Correction,
            Inserted = Inserted,
            ShowDashes = ShowDashes,
            BenefitOfDoubt = BenefitOfDoubtSelectedOption.Key,
            BenefitOfDoubtText = BenefitOfDoubtText
        };
        if (result.BenefitOfDoubt == BenefitOfDoubt.None)
        {
            result = result with { BenefitOfDoubtText = null };
        }
        return result;
    }

    private void DropFirstLetter(int elementIndex)
    {
        TextData item = Texts[elementIndex];
        string text = item.Text;
        if (text.Length < 2) return;

        char firstLetter = item.Text[0];
        int estimatedWidth = (int)(OcrProcessor.EstimateWordSize(LineHeight, firstLetter.ToString()).Height * 0.6d);

        item.Text = text[1..];
        item.Bounds = item.Bounds with {
            X = item.Bounds.X + estimatedWidth,
            Width = Math.Max(1, item.Bounds.Width - estimatedWidth)
        };
        UpdateWordImageData();
    }

    private void EstimateWordSize(int elementIndex)
    {
        TextData item = Texts[elementIndex];
        string text = item.Text;

        if (HasEstimatedSize)
        {
            if (LineHeightLarger)
            {
                LineHeightAdjustment -= 2;
                LineHeightLarger = false;
            }
            else
            {
                LineHeightLarger = true;
            }

            if (Math.Abs(LineHeightAdjustment) > LineHeight / 2)
            {
                LineHeightAdjustment = LineHeight / 2;
            }
        }
        int lineHeightAdjustmentFactor =
            LineHeightLarger ? 1 : -1;
        EstimatedWordSize estimatedSize = OcrProcessor.EstimateWordSize(LineHeight + (LineHeightAdjustment * lineHeightAdjustmentFactor), text, item.Bounds, CurrentWordReference.BookInfo);
        Texts[elementIndex].Bounds = estimatedSize.ExpandedRect;

        if (Texts.Length == 1)
        {
            double factor = Texts[0].Text switch {
                "I" => 2,
                "A" => 2,
                "a" => 2,
                _ => 1
            };
            Texts[0].Bounds = Texts[0].Bounds with { Width = (int)(Texts[0].Bounds.Width * factor) };
        }

        UpdateWordImageData();

        HasEstimatedSize = true;
    }

    private string GetActionName() => IsAddMode ? "Add" : "Edit";

    private string GetCurrentWordOutlierStyle()
    {
        if (Content.GetMostCommonDisplayText is null) return "";
        string? mostCommon = Content.GetMostCommonDisplayText(CurrentWordReference);
        if (mostCommon is null) return "";
        string currentText = CreateWord().GetDisplayText(showBenefitOfDoubt: true);
        if (string.IsNullOrEmpty(currentText)) return "";
        if (string.Equals(currentText, mostCommon, StringComparison.Ordinal)) return "";
        if (string.Equals(currentText, mostCommon, StringComparison.OrdinalIgnoreCase))
            return "background-color: var(--warning);";
        return "background-color: var(--error);";
    }

    private PageState.ImageOptions? GetImageOptions()
    {
        return !ShowHighContrast
            ? null
            : new PageState.ImageOptions {
                ApplyThreshold = ApplyThreshold,
                ShowHighContrast = ShowHighContrast,
                ThresholdLower = ThresholdLower,
                ThresholdUpper = ThresholdUpper
            };
    }

    private void LoadPageImage()
    {
        PageImage?.Dispose();
        FilteredPageImage?.Dispose();
        PageImage = ImageRepository.GetPageImage(PageImageFilePath);
        FilteredPageImage = ImageRepository.GetFilteredPageImage(PageImageFilePath, CreateFilteredPageImage);
        UpdatePageImageData();
    }

    private async Task MoveAsync(MouseEventArgs e, int elementIndex, int xFactor, int yFactor)
    {
        bool shouldCenter = false;
        ResetLineHeightAdjustment();
        bool wasAfter = elementIndex != 0 || isAfter();
        int changeSize = e.CtrlKey ? 1 : (LineHeight / 4);
        int xAdjustment = xFactor * changeSize;
        int yAdjustment = yFactor * changeSize;
        OcrRect bounds = Texts[elementIndex].Bounds;
        if (e.AltKey && yAdjustment == 0 && e.ShiftKey)
        {
            if (xAdjustment < 0)
            {
                var page = CurrentEdition.LoadedPages[CurrentWordReference.PageNumber].Page;
                var wordsBefore = page.Words.Where((x, index) => x != null && index < CurrentWordReference.WordIndex);
                var leftPositions = wordsBefore.SelectMany(x => x!.Elements).Select(x => x.Bounds.X);
                int leftMost = Math.Max(0, leftPositions.Any() ? leftPositions.Min() : 0);
                Texts[elementIndex].Bounds = Texts[elementIndex].Bounds = (bounds.Offset(0, bounds.Height) with { X = leftMost });
                shouldCenter = true;
            }
        }
        else
        {
            if (e.ShiftKey)
            {
                if (e.AltKey)
                    bounds = bounds with { Height = Math.Max(1, bounds.Height - yAdjustment) };
                Texts[elementIndex].Bounds = bounds.Offset(xAdjustment, yAdjustment);
            }
            else
                Texts[elementIndex].Bounds = bounds with {
                    Width = Math.Max(0, bounds.Width + xAdjustment),
                    Height = Math.Max(0, bounds.Height + yAdjustment)
                };
        }
        bool newIsAfter = elementIndex != 0 || isAfter();
        if (wasAfter != newIsAfter)
            AddWordAfter = newIsAfter;
        UpdateWordImageData();

        if (shouldCenter)
            await CenterImagePointAsync(Texts[elementIndex].Bounds);

        bool isAfter()
        {
            OcrRect bounds = Texts[0].Bounds;
            int middle = bounds.Y + (bounds.Height / 2);
            if (middle > OriginalBounds.GetBottom()) return true;
            if (middle < OriginalBounds.Y) return false;
            if (bounds.X < OriginalBounds.X) return false;
            return true;
        }
    }

    private async Task MoveDownAsync(MouseEventArgs e, int elementIndex)
    {
        await MoveAsync(e, elementIndex, 0, 1);
    }

    private async Task MoveLeftAsync(MouseEventArgs e, int elementIndex)
    {
        await MoveAsync(e, elementIndex, -1, 0);
    }

    private async Task MoveRightAsync(MouseEventArgs e, int elementIndex)
    {
        await MoveAsync(e, elementIndex, 1, 0);
    }

    private async Task MoveUpAsync(MouseEventArgs e, int elementIndex)
    {
        await MoveAsync(e, elementIndex, 0, -1);
    }

    private void PageFilterChanged()
    {
        FilteredPageImage?.Dispose();
        FilteredPageImage = CreateFilteredPageImage();
        UpdatePageImageData();
    }

    private void ResetLineHeightAdjustment()
    {
        LineHeightAdjustment = 0;
        LineHeightLarger = false;
        HasEstimatedSize = false;
    }

    private async Task ShowTextSamplesAsync()
    {
        var content = new TextSamplesDialog.TextSamplesDialogContent { BookInfo = CurrentWordReference.BookInfo };
        var dialogParameters = new DialogParameters { Height = "100vh", Width = "100vw" };
        await DialogService.ShowDialogAsync<TextSamplesDialog, TextSamplesDialog.TextSamplesDialogContent>(content, dialogParameters);
    }

    private void ThresholdLowerChanged()
    {
        if (ThresholdLower >= ThresholdUpper)
            ThresholdUpper = ThresholdLower + 1;
        PageFilterChanged();
    }

    private void ThresholdUpperChanged()
    {
        if (ThresholdUpper <= ThresholdLower)
            ThresholdLower = ThresholdUpper - 1;
        PageFilterChanged();
    }

    private void UpdatePageImageData()
    {
        PageImageData = PageDisplayImage.ToEmbeddedHtmlImage();
        UpdateWordImageData();
    }

    private void UpdateWordImageData()
    {
        if (Word is null) return;
        OcrWord tempWord = CreateWord();
        using MagickImage lineImage = PageState.GetWordImage(PageDisplayImage, tempWord);
        WordImageData = lineImage.ToEmbeddedHtmlImage();
    }

    private void ReadAppSettings()
    {
        // Edition
        LineHeight = AppPreferences.Editions.GetLineHeight(Content.Edition.BookInfo);
        // Image
        ApplyThreshold = AppPreferences.EditWordDialog.ApplyThreshold;
        ShowHighContrast = AppPreferences.EditWordDialog.ShowHighContrast;
        ThresholdLower = AppPreferences.EditWordDialog.ThresholdLower;
        ThresholdUpper = AppPreferences.EditWordDialog.ThresholdUpper;
    }

    private void WriteAppSettings()
    {
        // Edition
        AppPreferences.Editions.SetLineHeight(Content.Edition.BookInfo, LineHeight);
        // Image
        AppPreferences.EditWordDialog.ApplyThreshold = ApplyThreshold;
        AppPreferences.EditWordDialog.ShowHighContrast = ShowHighContrast;
        AppPreferences.EditWordDialog.ThresholdLower = ThresholdLower;
        AppPreferences.EditWordDialog.ThresholdUpper = ThresholdUpper;
    }

    private class TextData
    {
        public OcrRect Bounds { get; set; }

        public bool IsOnNextPage { get; set; }

        public string Text { get; set; } = null!;

        public TextData(string text, OcrRect bounds, bool isOnNextPage)
        {
            Text = text;
            Bounds = bounds;
            IsOnNextPage = isOnNextPage;
        }
    }
}
