using ConvertImagesToText;
using Microsoft.AspNetCore.Components;
using System.Collections.Concurrent;

namespace WordsAnalysis.Components;

public partial class EditionProcessor : IDisposable
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [EditorRequired, Parameter]
    public required EditionsProcessorBase Processor { get; set; }

    public bool StartProcessingEnabled => !Processor.IsProcessing;
    public bool StopProcessingEnabled => Processor.IsProcessing;

    ConcurrentDictionary<string, string>? EditionProgress = [];

    void IDisposable.Dispose()
    {
        Processor?.Stop();
    }

    void StartProcessing()
    {
        EditionProgress = new();
        Processor.Start(StartedProcessingFile, ProcessingFinished);
    }

    void ProcessingFinished()
    {
        InvokeAsync(() =>
        {
            StateHasChanged();
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

    void StopProcessing()
    {
        Processor?.Stop();
        ProcessingFinished();
    }

}