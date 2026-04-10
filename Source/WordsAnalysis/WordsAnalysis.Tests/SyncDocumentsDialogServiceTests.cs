using Microsoft.FluentUI.AspNetCore.Components;
using NSubstitute;
using WordsAnalysis.AppLayer.Features.SyncDocuments;
using WordsAnalysis.Components;
using WordsAnalysis.Services;

namespace WordsAnalysis.Tests;

public class SyncDocumentsDialogServiceTests
{
    private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
    private readonly SyncDocumentsDialogService _sut;

    public SyncDocumentsDialogServiceTests()
    {
        _sut = new SyncDocumentsDialogService(_dialogService);
    }

    // EditWordDialog tests

    [Fact]
    public async Task ShowEditWordDialogAsync_Cancelled_ReturnsNull()
    {
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Cancel());

        _dialogService
            .ShowDialogAsync<EditWordDialog, EditWordDialogContent>(
                Arg.Any<EditWordDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        EditWordDialogContent content = CreateEditWordDialogContent();
        EditWordDialogResult? result = await _sut.ShowEditWordDialogAsync(content);

        Assert.Null(result);
    }

    [Fact]
    public async Task ShowEditWordDialogAsync_Confirmed_ReturnsTypedResult()
    {
        var expectedResult = new EditWordDialogResult(null, false);
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Ok<object>(expectedResult));

        _dialogService
            .ShowDialogAsync<EditWordDialog, EditWordDialogContent>(
                Arg.Any<EditWordDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        EditWordDialogContent content = CreateEditWordDialogContent();
        EditWordDialogResult? result = await _sut.ShowEditWordDialogAsync(content);

        Assert.NotNull(result);
        Assert.Same(expectedResult, result);
    }

    // DeleteWordsDialog tests

    [Fact]
    public async Task ShowDeleteWordsDialogAsync_Cancelled_ReturnsNull()
    {
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Cancel());

        _dialogService
            .ShowDialogAsync<DeleteWordsDialog, DeleteWordsDialogContent>(
                Arg.Any<DeleteWordsDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        DeleteWordsDialogContent content = CreateDeleteWordsDialogContent();
        DeleteWordsDialogResult? result = await _sut.ShowDeleteWordsDialogAsync(content);

        Assert.Null(result);
    }

    [Fact]
    public async Task ShowDeleteWordsDialogAsync_Confirmed_ReturnsTypedResult()
    {
        var expectedResult = new DeleteWordsDialogResult([]);
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Ok<object>(expectedResult));

        _dialogService
            .ShowDialogAsync<DeleteWordsDialog, DeleteWordsDialogContent>(
                Arg.Any<DeleteWordsDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        DeleteWordsDialogContent content = CreateDeleteWordsDialogContent();
        DeleteWordsDialogResult? result = await _sut.ShowDeleteWordsDialogAsync(content);

        Assert.NotNull(result);
        Assert.Same(expectedResult, result);
    }

    // RescanAreaDialog tests

    [Fact]
    public async Task ShowRescanAreaDialogAsync_Cancelled_ReturnsNull()
    {
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Cancel());

        _dialogService
            .ShowDialogAsync<RescanAreaDialog, RescanAreaDialogContent>(
                Arg.Any<RescanAreaDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        RescanAreaDialogContent content = CreateRescanAreaDialogContent();
        RescanAreaDialogResult? result = await _sut.ShowRescanAreaDialogAsync(content);

        Assert.Null(result);
    }

    [Fact]
    public async Task ShowRescanAreaDialogAsync_Confirmed_ReturnsTypedResult()
    {
        var expectedResult = new RescanAreaDialogResult([]);
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Ok<object>(expectedResult));

        _dialogService
            .ShowDialogAsync<RescanAreaDialog, RescanAreaDialogContent>(
                Arg.Any<RescanAreaDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        RescanAreaDialogContent content = CreateRescanAreaDialogContent();
        RescanAreaDialogResult? result = await _sut.ShowRescanAreaDialogAsync(content);

        Assert.NotNull(result);
        Assert.Same(expectedResult, result);
    }

    // SplitWordsDialog tests

    [Fact]
    public async Task ShowSplitWordsDialogAsync_Cancelled_ReturnsNull()
    {
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Cancel());

        _dialogService
            .ShowDialogAsync<SplitWordsDialog, SplitWordsDialogContent>(
                Arg.Any<SplitWordsDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        SplitWordsDialogContent content = CreateSplitWordsDialogContent();
        SplitWordsDialogResult? result = await _sut.ShowSplitWordsDialogAsync(content);

        Assert.Null(result);
    }

    [Fact]
    public async Task ShowSplitWordsDialogAsync_Confirmed_ReturnsTypedResult()
    {
        var expectedResult = new SplitWordsDialogResult(null);
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Ok<object>(expectedResult));

        _dialogService
            .ShowDialogAsync<SplitWordsDialog, SplitWordsDialogContent>(
                Arg.Any<SplitWordsDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        SplitWordsDialogContent content = CreateSplitWordsDialogContent();
        SplitWordsDialogResult? result = await _sut.ShowSplitWordsDialogAsync(content);

        Assert.NotNull(result);
        Assert.Same(expectedResult, result);
    }

    // ViewColumnImagesDialog tests

    [Fact]
    public async Task ShowViewColumnImagesDialogAsync_Completes()
    {
        _dialogService
            .ShowDialogAsync<ViewColumnImagesDialog, ViewColumnImagesDialogContent>(
                Arg.Any<ViewColumnImagesDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(Substitute.For<IDialogReference>());

        ViewColumnImagesDialogContent content = CreateViewColumnImagesDialogContent();
        await _sut.ShowViewColumnImagesDialogAsync(content);

        await _dialogService.Received(1)
            .ShowDialogAsync<ViewColumnImagesDialog, ViewColumnImagesDialogContent>(
                content,
                Arg.Any<DialogParameters>());
    }

    // Helpers

    private static EditWordDialogContent CreateEditWordDialogContent()
    {
        return new EditWordDialogContent(null!, null!, 100, 100, false);
    }

    private static DeleteWordsDialogContent CreateDeleteWordsDialogContent()
    {
        return new DeleteWordsDialogContent(null!, []);
    }

    private static RescanAreaDialogContent CreateRescanAreaDialogContent()
    {
        return new RescanAreaDialogContent(null!, null!);
    }

    private static SplitWordsDialogContent CreateSplitWordsDialogContent()
    {
        return new SplitWordsDialogContent(null!, null!, [], 100, 100);
    }

    private static ViewColumnImagesDialogContent CreateViewColumnImagesDialogContent()
    {
        return new ViewColumnImagesDialogContent(
            System.Collections.Immutable.ImmutableDictionary<DocumentsModel.OcrBookInfo, EditionState>.Empty,
            []);
    }
}
