namespace TheCompleteBookOfMormon.Domain.Editions;

public class Edition : Entity
{
    public int Year { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}
