namespace TheCompleteBookOfMormon.Domain.Pages;

public class Page : Entity
{
    public Guid EditionId { get; init; }
    public int Number { get; init; }
    public long FileSize { get; set; }
    public string FileExtension { get; set; } = "";
    public DateTime LastWrittenUtc { get; set; } 
    public virtual PageScan Scan { get; set; } = null!;

    public bool Matches(long fileSize, string fileExtension, DateTime lastWrittenUtc) =>
        fileSize == FileSize
        && lastWrittenUtc == LastWrittenUtc
        && string.Equals(fileExtension, FileExtension, StringComparison.OrdinalIgnoreCase);
}
