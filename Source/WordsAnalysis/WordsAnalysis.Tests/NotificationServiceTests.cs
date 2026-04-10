using Microsoft.FluentUI.AspNetCore.Components;
using NSubstitute;
using WordsAnalysis.Services;

namespace WordsAnalysis.Tests;

public class NotificationServiceTests
{
    private readonly IToastService _toastService = Substitute.For<IToastService>();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(_toastService);
    }

    [Fact]
    public void ShowError_DefaultTimeout_CallsToastServiceWith3000()
    {
        _sut.ShowError("error message");

        _toastService.Received(1).ShowError(
            "error message",
            timeout: 3000);
    }

    [Fact]
    public void ShowError_CustomTimeout_PassesThrough()
    {
        _sut.ShowError("error message", 5000);

        _toastService.Received(1).ShowError(
            "error message",
            timeout: 5000);
    }

    [Fact]
    public void ShowWarning_DefaultTimeout_CallsToastServiceWith3000()
    {
        _sut.ShowWarning("warning message");

        _toastService.Received(1).ShowWarning(
            "warning message",
            timeout: 3000);
    }

    [Fact]
    public void ShowWarning_CustomTimeout_PassesThrough()
    {
        _sut.ShowWarning("warning message", 7000);

        _toastService.Received(1).ShowWarning(
            "warning message",
            timeout: 7000);
    }

    [Fact]
    public void ClearAll_CallsToastServiceClearAll()
    {
        _sut.ClearAll();

        _toastService.Received(1).ClearAll();
    }
}
