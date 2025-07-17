using DocumentsModel.Helpers;
using DocumentsModel;
using System.Collections.Concurrent;

namespace ConvertImagesToText;

public abstract class EditionsProcessorBase : IDisposable
{
    protected abstract void ProcessFile(OcrBookInfo bookInfo, string scansDirectoryPath, string scansDeskewedDirectoryPath, string ocrDirectoryPath, string imageFileName, bool multiColumn);

    public bool IsProcessing { get; protected set; }

    private Thread? ProcessingThread;
    private readonly Lock SyncRoot = new();

    protected readonly string SourcesDirectoryPath;
    private Action<KeyValuePair<string, string>>? FileProcessingStarted;
    private Action? Stopped;

    protected EditionsProcessorBase(string sourcesDirectoryPath)
    {
        ArgumentNullException.ThrowIfNull(sourcesDirectoryPath);
        if (!Directory.Exists(sourcesDirectoryPath))
            throw new DirectoryNotFoundException(sourcesDirectoryPath);

        SourcesDirectoryPath = sourcesDirectoryPath;
    }

    public void Start(Action<KeyValuePair<string, string>> fileProcessingStarted, Action stopped)
    {
        ArgumentNullException.ThrowIfNull(fileProcessingStarted);
        ArgumentNullException.ThrowIfNull(stopped);

        lock (SyncRoot)
        {
            if (IsProcessing) return;
            IsProcessing = true;
            FileProcessingStarted = fileProcessingStarted;
            Stopped = stopped;
        }
        ProcessingThread = new Thread(Process);
        ProcessingThread.Start();
    }

    public void Stop()
    {
        lock (SyncRoot)
        {
            if (!IsProcessing) return;
            IsProcessing = false;
        }
        Stopped?.Invoke();
    }

    void IDisposable.Dispose() => Stop();

    private void Process()
    {
        var editionDirs = Directory.GetDirectories(SourcesDirectoryPath).Where(x => !x.Contains("JosephSmithPapers")).Where(x => File.Exists(Path.Combine(x, DocumentsModel.Constants.OcrBookInfoFileName))).Order();

        var editionPagesList = new List<EditionPages>();
        foreach (var editionDir in editionDirs)
        {
            if (!IsProcessing) return;
            string editionCode = Path.GetFileName(editionDir)!;
            OcrBookInfo bookInfo = OcrBookInfo.LoadAsync(SourcesDirectoryPath, editionCode).Result!;
            string editionDirectoryPath = FilePathHelper.GetEditionDirectoryPath(SourcesDirectoryPath, editionCode);
            string scansDir = Path.Combine(editionDirectoryPath, Constants.ScansDirectoryName);
            Directory.CreateDirectory(scansDir);
            string deskewedDir = Path.Combine(editionDirectoryPath, Constants.ScansDeskewedDirectoryName);
            Directory.CreateDirectory(deskewedDir);
            string ocrDir = FilePathHelper.GetOcrDirectoryPath(SourcesDirectoryPath, bookInfo);
            Directory.CreateDirectory(ocrDir);

            string[] imagePaths = Directory.GetFiles(scansDir);
            editionPagesList.Add(new EditionPages {
                EditionCode = editionCode,
                BookInfo = bookInfo,
                ScansDirectoryPath = scansDir,
                ScansDeskewedDirectoryPath = deskewedDir,
                OcrDirectoryPath = ocrDir,
                ImageFilePaths = imagePaths,
                TotalPageCount = imagePaths.Length
            });

            FileProcessingStarted?.Invoke(new KeyValuePair<string, string>(editionCode, $"000 of {imagePaths.Length}"));
        }

        var pageJobs = new ConcurrentQueue<PageJob>();
        int index = 0;
        bool pageAdded;
        do
        {
            pageAdded = false;
            foreach (var edition in editionPagesList)
            {
                if (index < edition.ImageFilePaths.Length)
                {
                    pageJobs.Enqueue(new PageJob {
                        Edition = edition,
                        ImageFilePath = edition.ImageFilePaths[index]
                    });
                    pageAdded = true;
                }
            }
            index++;
        } while (pageAdded);

        var parallelOptions = new ParallelOptions {
            MaxDegreeOfParallelism = Math.Max(1, Environment.ProcessorCount - 1)
            //MaxDegreeOfParallelism = 1
        };

        Parallel.ForEach(pageJobs, parallelOptions, (_, state) =>
        {
            if (!IsProcessing)
            {
                pageJobs.Clear();
                state.Stop();
                return;
            }

            if (!pageJobs.TryDequeue(out var pageJob)) return;

            var fileName = Path.GetFileName(pageJob.ImageFilePath);
            FileProcessingStarted?.Invoke(new(pageJob.Edition.EditionCode, $"{(pageJob.Edition.CompletedPageCount + 1):000} of {pageJob.Edition.TotalPageCount}"));

            ProcessFile(
                pageJob.Edition.BookInfo,
                pageJob.Edition.ScansDirectoryPath,
                pageJob.Edition.ScansDeskewedDirectoryPath,
                pageJob.Edition.OcrDirectoryPath,
                fileName,
                pageJob.Edition.BookInfo.MultiColumn
            );

            int count = Interlocked.Increment(ref pageJob.Edition.CompletedPageCount);
            if (count == pageJob.Edition.TotalPageCount)
            {
                FileProcessingStarted?.Invoke(new(pageJob.Edition.EditionCode, "Completed"));
            }
        });

        if (IsProcessing) Stop();
    }

}
