using ConvertImagesToText;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace WordsAnalysis.Components;

public partial class EditionProcessor : IDisposable
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool HideControlButtons { get; set; }

    [Parameter]
    public EventCallback OnStarted { get; set; }

    [Parameter]
    public EventCallback OnFinished { get; set; }

    [EditorRequired, Parameter]
    public required EditionsProcessorBase Processor { get; set; }

    public bool StartProcessingEnabled => !Processor.IsProcessing;
    public bool StopProcessingEnabled => Processor.IsProcessing;

    ConcurrentDictionary<string, string>? EditionProgress = [];

    public void StartProcessing()
    {
        if (!StartProcessingEnabled) throw new InvalidOperationException("Cannot start processing");
        EditionProgress = new();
        Processor.Start(StartedProcessingFile, ProcessingFinished);
        _ = InvokeAsync(OnStarted.InvokeAsync);
    }

    public void StopProcessing()
    {
        Processor?.Stop();
        ProcessingFinished();
    }


    void IDisposable.Dispose()
    {
        Processor?.Stop();
    }


    void ProcessingFinished()
    {
        InvokeAsync(async () =>
        {
            StateHasChanged();
            await OnFinished.InvokeAsync();
        });
    }

    void StartedProcessingFile(KeyValuePair<string, string> info)
    {
        InvokeAsync(() =>
        {
            EditionProgress?.AddOrUpdate(info.Key, info.Value, (_, _) => info.Value);
            StateHasChanged();
        });
    }

}