using WordsAnalysis.Services;

namespace WordsAnalysis.Tests;

public class DictionaryServiceTests
{
    [Fact]
    public void AddDerivations_EntryLessThan2Chars_NoDerivationsAdded()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "a", false);

        Assert.Empty(dictionary);
    }

    [Fact]
    public void AddDerivations_EntryContainsApostrophe_NoDerivationsAdded()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "it's", false);

        Assert.Empty(dictionary);
    }

    [Fact]
    public void AddDerivations_IsNameEndingInS_AddsApostrophe()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "James", true);

        Assert.Contains("James'", dictionary);
        Assert.Contains("James's", dictionary);
    }

    [Fact]
    public void AddDerivations_IsNameNotEndingInS_AddsPossessive()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "Nephi", true);

        Assert.Contains("Nephi's", dictionary);
        Assert.DoesNotContain("Nephi'", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInE_AddsExpectedSuffixes()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "make", false);

        Assert.Contains("maketh", dictionary);
        Assert.Contains("making", dictionary);
        Assert.Contains("makings", dictionary);
        Assert.Contains("maked", dictionary);
        Assert.Contains("maken", dictionary);
        Assert.Contains("makest", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInING_AddsLy()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "making", false);

        Assert.Contains("makingly", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInLL_AddsEth()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "fall", false);

        Assert.Contains("falleth", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInLNotLL_AddsLethAndLing()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "heal", false);

        Assert.Contains("healleth", dictionary);
        Assert.Contains("healing", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInN_AddsNethAndNning()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "open", false);

        Assert.Contains("openneth", dictionary);
        Assert.Contains("openning", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInSS_AddsEs()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "bless", false);

        Assert.Contains("blesses", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInEY_AddsDerivations()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "obey", false);

        Assert.Contains("obeyeth", dictionary);
        Assert.Contains("obies", dictionary);
        Assert.Contains("obieth", dictionary);
        Assert.Contains("obied", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInYNotEY_AddsDerivations()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "carry", false);

        Assert.Contains("carryeth", dictionary);
        Assert.Contains("carries", dictionary);
        Assert.Contains("carrieth", dictionary);
        Assert.Contains("carried", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInED_AddsEthAndEst()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "blessed", false);

        Assert.Contains("blesseth", dictionary);
        Assert.Contains("blessest", dictionary);
    }

    [Fact]
    public void AddDerivations_NotEndingInE_AddsCommonSuffixes()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "work", false);

        Assert.Contains("worketh", dictionary);
        Assert.Contains("working", dictionary);
        Assert.Contains("workings", dictionary);
        Assert.Contains("workest", dictionary);
        Assert.Contains("workes", dictionary);
        Assert.Contains("worked", dictionary);
    }

    [Fact]
    public void AddDerivations_NotEndingInS_AddsS()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "work", false);

        Assert.Contains("works", dictionary);
    }

    [Fact]
    public void AddDerivations_EndsInS_DoesNotAddExtraS()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "works", false);

        Assert.DoesNotContain("workss", dictionary);
    }

    [Fact]
    public void AddDerivations_ContainsOur_AddsOrVariant()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "colour", false);

        Assert.Contains("color", dictionary);
    }

    [Fact]
    public void AddDerivations_ContainsIse_DoesNotAddIzeVariant_BecauseUpperCaseCheckMismatch()
    {
        // Note: The code checks upper.Contains("ise") but upper is all uppercase,
        // so this branch is never reached. This test documents the actual behavior.
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "realise", false);

        Assert.DoesNotContain("realize", dictionary);
    }

    [Fact]
    public void AddDerivations_CvcPattern_DoublesLastConsonantPlusIng()
    {
        // "run" -> R(consonant) U(vowel) N(consonant) -> CVC pattern
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "run", false);

        Assert.Contains("runNing", dictionary);
    }

    [Fact]
    public void AddDerivations_ConsonantL_AddsLed()
    {
        // "heal" ends in consonant+L pattern ('a' is vowel before 'l', doesn't match)
        // "abil" - 'i' is vowel, 'l' consonant but regex is [^AEIOU]L - so 'b' before 'i' before 'l' won't match
        // We need something like "curl" - c-u-r-l -> [^AEIOU]L means the char before L is non-vowel
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "curl", false);

        Assert.Contains("curlled", dictionary);
    }

    [Fact]
    public void AddDerivations_IsName_DoesNotAddNonNameDerivations()
    {
        List<string> dictionary = new();

        DictionaryService.AddDerivations(dictionary, "Nephi", true);

        // Names should only get possessive forms
        Assert.DoesNotContain("Nephieth", dictionary);
        Assert.DoesNotContain("Nephiing", dictionary);
    }
}
