using System.Text.RegularExpressions;
using WordsAnalysis.AppLayer.Constants;

namespace WordsAnalysis.Services;

public interface IDictionaryService
{
    bool WordExists(string word);
    IEnumerable<string[]> SplitTextIntoWords(string text);
}

sealed class DictionaryService : IDictionaryService
{
    private readonly HashSet<string> Dictionary;
    private readonly int LongestWordLength;
    private readonly Dictionary<int, HashSet<string>> WordsByLength;

    public DictionaryService()
    {
        string dictionaryFilePath = Path.Combine(Data.SourcesDirectoryPath, "..", "dictionary.txt");
        var entries = new List<string>(10 * 1000 * 1000);
        IEnumerable<string> lines = File.ReadLines(dictionaryFilePath);

        foreach (string line in lines)
        {
            string entry = line.Trim();
            bool isName = entry.StartsWith("name:");
            if (isName) entry = entry[5..];
            entries.Add(entry);
            AddDerivations(entries, entry, isName);
        }
        Dictionary = entries.Distinct().ToHashSet(StringComparer.OrdinalIgnoreCase);
        WordsByLength = Dictionary.GroupBy(x => x.Length).ToDictionary(x => x.Key, x => x.ToHashSet(StringComparer.OrdinalIgnoreCase));
        LongestWordLength = WordsByLength.Keys.Max();
    }

    public IEnumerable<string[]> SplitTextIntoWords(string text)
    {
        if (string.IsNullOrEmpty(text))
            yield break;

        if (Dictionary.Contains(text))
        {
            yield return [text];
        }

        int length = Math.Min(LongestWordLength, text.Length - 1);
        for (int i = length; i >= 1; i--)
        {
            string left = text[..i];
            if (!Dictionary.Contains(left)) continue;
            foreach (string[] right in SplitTextIntoWords(text[i..]))
            {
                yield return [left, .. right];
            }
        }
    }


    public bool WordExists(string word)
    {
        return Dictionary.Contains(word);
    }

    public static void AddDerivations(List<string> dictionary, string entry, bool isName)
    {
        if (entry.Length < 2 || entry.Contains('\'')) return;

        string upper = entry.ToUpper();

        if (isName)
        {
            if (upper.EndsWith("S"))
                dictionary.Add(entry + "'");
            dictionary.Add(entry + "'s");
        }
        else
        {
            if (upper.EndsWith("E"))
            {
                dictionary.Add(entry + "th");
                dictionary.Add(entry[..^1] + "ing");
                dictionary.Add(entry[..^1] + "ings");
                dictionary.Add(entry + "d");
                dictionary.Add(entry + "n");
                dictionary.Add(entry + "st");
            }

            if (upper.EndsWith("ING"))
            {
                dictionary.Add(entry + "ly");
            }

            if (upper.EndsWith("LL"))
            {
                dictionary.Add(entry + "eth");
            }
            else if (upper.EndsWith("L"))
            {
                dictionary.Add(entry + "leth");
                dictionary.Add(entry + "ling");
            }

            if (upper.EndsWith("N"))
            {
                dictionary.Add(entry + "neth");
                dictionary.Add(entry + "ning");
            }

            if (upper.EndsWith("SS"))
            {
                dictionary.Add(entry + "es");
            }

            if (upper.EndsWith("Y"))
            {
                dictionary.Add(entry + "eth");
                dictionary.Add(entry[..^1] + "ies");
                dictionary.Add(entry[..^1] + "ieth");
                dictionary.Add(entry[..^1] + "ied");
            }

            if (EndsInCvc(upper))
            {
                dictionary.Add(entry + upper[^1] + "ing");
            }

            if (Regex.IsMatch(upper, @"[^AEIOU]L$"))
            {
                dictionary.Add(entry + "led");
            }

            if (!upper.EndsWith("E"))
            {
                dictionary.Add(entry + "eth");
                dictionary.Add(entry + "ing");
                dictionary.Add(entry + "ings");
                dictionary.Add(entry + "est");
                dictionary.Add(entry + "es");
                if (!upper.EndsWith("ed"))
                {
                    dictionary.Add(entry + "ed");
                }
            }

            if (!upper.EndsWith("S"))
            {
                dictionary.Add(entry + "s");
            }
        }
    }

    private static bool EndsInCvc(string word)
    {
        if (word.Length < 3)
            return false;

        string lastThree = word[^3..];
        return Regex.IsMatch("^[^AEIOU][AEIOU][^AEIOU]$", lastThree);
    }

}
