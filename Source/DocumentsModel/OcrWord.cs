using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace DocumentsModel;

[DebuggerDisplay("{GetDisplayText(false)}")]
public record OcrWord
{
    public required ImmutableList<OcrElement> Elements { get; init; }

    public string GetCombinedText() => string.Join("", Elements.Where((x, index) => ShowDashes || index == 0 || x.Text != "-").Select(x => x.Text));
    public string GetDisplayText(bool showBenefitOfDoubt) =>
        Corrected && showBenefitOfDoubt
        ? ""
        : showBenefitOfDoubt && BenefitOfDoubt != BenefitOfDoubt.None
        ? BenefitOfDoubtText ?? ""
        : string.Join("", Elements.Where((x, index) => ShowDashes || index == 0 || x.Text != "-").Select(x => ShowDashes ? x.Text : x.GetDisplayText()));

    public bool IsComposite() => Elements.Count > 1;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public BenefitOfDoubt BenefitOfDoubt { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? BenefitOfDoubtText { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Corrected { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Correction { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool Inserted { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Notes { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool ShowDashes { get; init;  }

    public OcrElement LastElementOnSamePage() => Elements.Last(x => !x.IsOnNextPage);
}
