using System.ComponentModel.DataAnnotations;

namespace TheCompleteBookOfMormon.Domain.Editions;

public class Edition : Entity
{
    [Required]
    public string Code { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string FolderName { get; set; } = "";

    public int Year { get; set; }

    public bool ExcludeFromUI { get; set; }
}
