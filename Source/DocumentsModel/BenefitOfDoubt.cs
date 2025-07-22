namespace DocumentsModel;

public enum BenefitOfDoubt
{
    None = 0,
    ScanError = 1,
    InkError = 2,
    PrinterError = 3,
    EditorialFormatting = 4
}

public static class BenefitOfDoubtExtensions
{
    public static IEnumerable<KeyValuePair<BenefitOfDoubt, string>> GetOptions() =>
        [
            new (BenefitOfDoubt.None, "None"),
            new (BenefitOfDoubt.PrinterError, "Printer error"),
            new (BenefitOfDoubt.EditorialFormatting, "Editorial formatting"),
            new (BenefitOfDoubt.InkError, "Ink error"),
            new (BenefitOfDoubt.InkError, "Scan error"),
        ];
}
