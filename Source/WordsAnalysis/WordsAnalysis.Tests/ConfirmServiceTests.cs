using Microsoft.FluentUI.AspNetCore.Components;
using NSubstitute;
using WordsAnalysis.Components;
using WordsAnalysis.Services;

namespace WordsAnalysis.Tests;

public class ConfirmServiceTests
{
    private readonly IDialogService _dialogService = Substitute.For<IDialogService>();
    private readonly ConfirmService _sut;

    public ConfirmServiceTests()
    {
        _sut = new ConfirmService(_dialogService);
    }

    [Fact]
    public async Task ConfirmAsync_DialogCancelled_ReturnsFalse()
    {
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Cancel());

        _dialogService
            .ShowDialogAsync<ConfirmDialog, ConfirmDialogContent>(
                Arg.Any<ConfirmDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        bool result = await _sut.ConfirmAsync("Are you sure?");

        Assert.False(result);
    }

    [Fact]
    public async Task ConfirmAsync_ConfirmedWithTrue_ReturnsTrue()
    {
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Ok((object)true));

        _dialogService
            .ShowDialogAsync<ConfirmDialog, ConfirmDialogContent>(
                Arg.Any<ConfirmDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        bool result = await _sut.ConfirmAsync("Are you sure?");

        Assert.True(result);
    }

    [Fact]
    public async Task ConfirmAsync_ConfirmedWithFalse_ReturnsFalse()
    {
        IDialogReference dialogRef = Substitute.For<IDialogReference>();
        dialogRef.Result.Returns(DialogResult.Ok((object)false));

        _dialogService
            .ShowDialogAsync<ConfirmDialog, ConfirmDialogContent>(
                Arg.Any<ConfirmDialogContent>(),
                Arg.Any<DialogParameters>())
            .Returns(dialogRef);

        bool result = await _sut.ConfirmAsync("Are you sure?");

        Assert.False(result);
    }
}
