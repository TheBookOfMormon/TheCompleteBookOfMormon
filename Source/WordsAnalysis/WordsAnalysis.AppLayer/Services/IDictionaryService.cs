namespace WordsAnalysis.AppLayer.Services;

public interface IDictionaryService
{
    bool WordExists(string word);
    IEnumerable<string[]> SplitTextIntoWords(string text);
}
