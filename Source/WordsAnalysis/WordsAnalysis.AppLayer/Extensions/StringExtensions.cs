namespace WordsAnalysis.AppLayer.Extensions;

public static class StringExtensions
{
    public static bool HasCapitalAfterLower(this string input)
    {
        bool foundLower = false;
        foreach (char c in input)
        {
            if (char.IsLower(c))
                foundLower = true;
            else if (foundLower && char.IsUpper(c))
                return true;
        }
        return false;
    }
}
