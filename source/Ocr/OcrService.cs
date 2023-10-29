using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ocr.Config;
using System.Collections.Immutable;
using System.Text.Json;

namespace Ocr;

internal interface IOcrService
{
    ValueTask<ImmutableArray<Word>> GetOcrAsync(
        string imagePath,
        CancellationToken cancellationToken);
}

internal class OcrService : IOcrService
{
    private readonly HttpClient HttpClient;
    private readonly IOptions<OcrApiSettings> ApiSettings;
    private readonly ILogger<OcrService> Logger;

    public OcrService(
        HttpClient httpClient,
        IOptions<OcrApiSettings> apiSettings,
        ILogger<OcrService> logger)
    {
        HttpClient = httpClient;
        ApiSettings = apiSettings ?? throw new ArgumentNullException(nameof(apiSettings));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async ValueTask<ImmutableArray<Word>> GetOcrAsync(
        string imagePath,
        CancellationToken cancellationToken)
    {
        decimal maxFileSize = ApiSettings.Value.MaxFileSizeInMB * 1024m * 1024m;

        var fileInfo = new FileInfo(imagePath);
        if (fileInfo.Length > maxFileSize)
        {
            Logger.LogError(
                $"Image is larger than {ApiSettings.Value.MaxFileSizeInMB} MB: {{imagePath}}",
                imagePath);
            return ImmutableArray<Word>.Empty;
        }

        using var form = new MultipartFormDataContent();
        form.Headers.Add("ApiKey", ApiSettings.Value.Key);
        form.Add(new StringContent("true"), "isOverlayRequired");
        form.Add(new StringContent("2"), "ocrEngine");
        form.Add(
            new ByteArrayContent(File.ReadAllBytes(imagePath)),
            name: "file",
            fileName: Path.GetFileName(imagePath));

        Logger.LogInformation("Getting OCR for {imagePath}", imagePath);
        HttpResponseMessage response =
            await HttpClient.PostAsync(
                "https://api.ocr.space/parse/image",
                form,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                throw new UnauthorizedAccessException();
            
            Logger.LogError(
                "Error performing OCR on {imagePath}: {errorCode}",
                imagePath,
                response.StatusCode);
            return ImmutableArray<Word>.Empty;
        }

        if (cancellationToken.IsCancellationRequested)
            return ImmutableArray<Word>.Empty;

        string json = await response.Content.ReadAsStringAsync(cancellationToken);
        var doc = JsonSerializer.Deserialize<RootObject>(json)!;
        if (doc.IsErroredOnProcessing)
        {
            Logger.LogError(
                "Error processing {imagePath}: {errorMessage}",
                imagePath,
                doc.OCRExitCode);
            return ImmutableArray<Word>.Empty;
        }

        ImmutableArray<Word> result = doc
            .ParsedResults
            .SelectMany(x => x.TextOverlay!.Lines!)
            .SelectMany(x => x.Words!)
            .ToImmutableArray();
        return result;
    }
}


public class RootObject
{
    public ParsedResult[] ParsedResults { get; set; } = Array.Empty<ParsedResult>();
    public int OCRExitCode { get; set; }
    public bool IsErroredOnProcessing { get; set; }
    public string ProcessingTimeInMilliseconds { get; set; } = "";
    public string SearchablePDFURL { get; set; } = "";
}

public class ParsedResult
{
    public TextOverlay? TextOverlay { get; set; }
    public string TextOrientation { get; set; } = "";
    public int FileParseExitCode { get; set; }
    public string ParsedText { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public string ErrorDetails { get; set; } = "";
}

public class TextOverlay
{
    public Line[] Lines { get; set; } = Array.Empty<Line>();
    public bool HasOverlay { get; set; }
}

public class Line
{
    public string LineText { get; set; } = "";
    public Word[] Words { get; set; } = Array.Empty<Word>();
    public float MaxHeight { get; set; }
    public float MinTop { get; set; }
}

public class Word
{
    public string WordText { get; set; } = "";
    public float Left { get; set; }
    public float Top { get; set; }
    public float Height { get; set; }
    public float Width { get; set; }
}

