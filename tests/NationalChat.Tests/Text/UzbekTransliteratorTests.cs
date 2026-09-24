using System.Text.Json;
using Domain.Text;

namespace NationalChat.Tests.Text;

public sealed class UzbekTransliteratorTests
{
    public static IEnumerable<object[]> ToLatinCases() => LoadCases("toLatin");
    public static IEnumerable<object[]> ToCyrillicCases() => LoadCases("toCyrillic");

    [Theory]
    [MemberData(nameof(ToLatinCases))]
    public void ToLatin_MatchesSharedCases(string cyrillic, string latin)
    {
        Assert.Equal(latin, UzbekTransliterator.ToLatin(cyrillic));
    }

    [Theory]
    [MemberData(nameof(ToCyrillicCases))]
    public void ToCyrillic_MatchesSharedCases(string cyrillic, string latin)
    {
        Assert.Equal(cyrillic, UzbekTransliterator.ToCyrillic(latin));
    }

    [Fact]
    public void SharedCases_HasAtLeastSixtyEntries()
    {
        Assert.True(ReadCases().Count >= 60);
    }

    [Theory]
    [InlineData("Ўзбекистон")]
    [InlineData("O‘zbekiston")]
    [InlineData("Oʻzbekiston")]
    [InlineData("O'zbekiston")]
    [InlineData("o`zbekiston")]
    [InlineData("  ЎЗБЕКИСТОН ")]
    public void NormalizeForSearch_IgnoresScriptCaseAndApostropheForm(string text)
    {
        Assert.Equal("o'zbekiston", UzbekTransliterator.NormalizeForSearch(text));
    }

    [Fact]
    public void NormalizeForSearch_CollapsesWhitespaceAndMixedScripts()
    {
        Assert.Equal("salom, qalaysiz? ok", UzbekTransliterator.NormalizeForSearch("Салом,\n  Қалайсиз?\tOK"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NormalizeForSearch_BlankText_ReturnsEmpty(string? text)
    {
        Assert.Equal(string.Empty, UzbekTransliterator.NormalizeForSearch(text));
    }

    [Fact]
    public void NormalizeForSearch_CyrillicMessageContainsLatinQuery()
    {
        var stored = UzbekTransliterator.NormalizeForSearch("Салом, қалайсиз?");
        var query = UzbekTransliterator.NormalizeForSearch("qalay");

        Assert.Contains(query, stored);
    }

    private static IEnumerable<object[]> LoadCases(string direction) =>
        ReadCases()
            .Where(x => x.Direction is null or "both" || x.Direction == direction)
            .Select(x => new object[] { x.Cyrillic, x.Latin });

    private static IReadOnlyList<TransliterationCase> ReadCases()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Text", "transliteration-cases.json");
        var file = JsonSerializer.Deserialize<TransliterationCaseFile>(
            System.IO.File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        return file.Cases;
    }

    private sealed record TransliterationCaseFile(IReadOnlyList<TransliterationCase> Cases);
    private sealed record TransliterationCase(string Cyrillic, string Latin, string? Direction);
}
