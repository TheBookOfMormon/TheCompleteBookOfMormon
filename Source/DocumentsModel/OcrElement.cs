using System.Diagnostics;
using System.Text.Json.Serialization;

namespace DocumentsModel;

[DebuggerDisplay("{Text}")]
public record OcrElement
{
    public required OcrRect Bounds { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsOnNextPage { get; init; }

    public required string Text { get; init; }

    public OcrElement Append(OcrElement nextElement)
    {
        return this with {
            Text = Text + nextElement.Text,
            Bounds = Bounds.Union(nextElement.Bounds)
        };

    }

    public string GetDisplayText() =>
        Text switch {
            "'" => "{apos}",
            "\"" => "{quot}",
            "!" => "{excl}",
            "-" => "{min}",
            ":" => "{col}",
            ";" => "{semi}",
            "," => "{com}",
            "&" => "{amp}",
            "." => "{dot}",
            "—" => "{hyph}",
            "(" => "{open}",
            ")" => "{close}",
            _ => Text
        };
}
