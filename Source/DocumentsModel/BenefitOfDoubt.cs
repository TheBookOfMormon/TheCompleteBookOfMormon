namespace DocumentsModel;

public enum BenefitOfDoubt
{
    None,
    InkError,
    PrinterError
}

public static class BenefitOfDoubtExtensions
{
    public static IEnumerable<KeyValuePair<BenefitOfDoubt, string>> GetOptions() =>
        [
            new (BenefitOfDoubt.None, "None"),
            new (BenefitOfDoubt.InkError, "Ink error"),
            new (BenefitOfDoubt.PrinterError, "Printer error")
        ];
}
