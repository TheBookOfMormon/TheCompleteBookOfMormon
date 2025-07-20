namespace DocumentsModel;

public enum BenefitOfDoubt
{
    None = 0,
    InkError = 1,
    PrinterError = 2,
    EditorialFormatting = 3
}

public static class BenefitOfDoubtExtensions
{
    public static IEnumerable<KeyValuePair<BenefitOfDoubt, string>> GetOptions() =>
        [
            new (BenefitOfDoubt.None, "None"),
            new (BenefitOfDoubt.InkError, "Ink error"),
            new (BenefitOfDoubt.PrinterError, "Printer error"),
            new (BenefitOfDoubt.EditorialFormatting, "Editorial formatting")
        ];
}
