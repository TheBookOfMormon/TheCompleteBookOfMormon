namespace TheCompleteBookOfMormon.Domain.Pages;

public class PageScan : Entity
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
}
