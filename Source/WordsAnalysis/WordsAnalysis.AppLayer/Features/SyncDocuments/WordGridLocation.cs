namespace WordsAnalysis.AppLayer.Features.SyncDocuments;

public readonly record struct WordGridLocation(int RowIndex, int ColumnIndex)
{
    public static readonly WordGridLocation None = new(-1, -1);
}
