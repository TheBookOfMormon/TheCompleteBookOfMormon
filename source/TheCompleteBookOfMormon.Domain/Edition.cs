namespace TheCompleteBookOfMormon.Domain;

public class Edition : Entity
{
    public int Year { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
}
