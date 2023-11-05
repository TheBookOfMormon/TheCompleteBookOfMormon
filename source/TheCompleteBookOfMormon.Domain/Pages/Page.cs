namespace TheCompleteBookOfMormon.Domain.Pages;

public class Page : Entity
{
    public Guid EditionId { get; init; }
    public int Number { get; init; }
    public string FileExtension { get; set; } = "";
}
