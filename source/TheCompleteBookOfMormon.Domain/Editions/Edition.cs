using System.ComponentModel.DataAnnotations;

namespace TheCompleteBookOfMormon.Domain.Editions;

public class Edition : Entity
{
    public int Year { get; set; }

    [Required]
    public string? Code { get; set; }

    [Required]
    public string? Name { get; set; }

    public bool ExcludeFromUI { get; set; }
}
