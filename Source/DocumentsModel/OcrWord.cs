using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace DocumentsModel;

[DebuggerDisplay("{GetDisplayText()}")]
public record OcrWord
{
    public required ImmutableList<OcrElement> Elements { get; init; }

    public string GetCombinedText() => string.Join("", Elements.Where((x, index) => ShowDashes || index == 0 || x.Text != "-").Select(x => x.Text));
    public string GetDisplayText() => string.Join("", Elements.Where((x, index) => ShowDashes || index == 0 || x.Text != "-" || ShowDashes).Select(x => ShowDashes ? x.Text : x.GetDisplayText()));

    public bool IsComposite() => Elements.Count > 1;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ShowDashes { get; init;  }
}
