using System.Text;
using System.Text.RegularExpressions;

namespace Domain.Text;

/// <summary>
/// Converts Uzbek text between the Cyrillic and the 1995 Latin alphabet.
/// The TypeScript client keeps an identical copy (uzbek-transliterator.ts); both are tested
/// against the same transliteration-cases.json, so any rule change must be made in both places.
/// URLs, e-mail addresses, @usernames, #tags and `code` spans are never converted.
/// </summary>
public static partial class UzbekTransliterator
{
    /// <summary>Letter part of o‘ and g‘ in the output of <see cref="ToLatin"/>.</summary>
    public const char OkinaMark = '‘';

    /// <summary>Tutuq belgisi (ъ) in the output of <see cref="ToLatin"/>.</summary>
    public const char TutuqMark = '’';

    private const string Apostrophes = "'‘’`ʻʼ";
    private const string CyrillicVowels = "аеёиоуўэюяы";
    private const string LatinVowels = "aeiou";

    // Loanwords where Latin "ts" stands for Cyrillic "ц"; everywhere else "ts" is "тс".
    private static readonly string[] TsExceptionStems =
    [
        "tsiy", "tsion", "litsey", "patsient", "ofitsiant", "ofitser", "spetsial", "sotsial", "abzats", "kvarts"
    ];

    private static readonly Dictionary<char, string> CyrillicToLatinMap = new()
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['ғ'] = "g‘", ['д'] = "d", ['ё'] = "yo",
        ['ж'] = "j", ['з'] = "z", ['и'] = "i", ['й'] = "y", ['к'] = "k", ['қ'] = "q", ['л'] = "l",
        ['м'] = "m", ['н'] = "n", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t",
        ['у'] = "u", ['ў'] = "o‘", ['ф'] = "f", ['х'] = "x", ['ҳ'] = "h", ['ч'] = "ch", ['ш'] = "sh",
        ['щ'] = "sh", ['ъ'] = "’", ['ы'] = "i", ['ь'] = "", ['э'] = "e", ['ю'] = "yu", ['я'] = "ya"
    };

    private static readonly Dictionary<char, char> LatinToCyrillicMap = new()
    {
        ['a'] = 'а', ['b'] = 'б', ['c'] = 'с', ['d'] = 'д', ['f'] = 'ф', ['g'] = 'г', ['h'] = 'ҳ',
        ['i'] = 'и', ['j'] = 'ж', ['k'] = 'к', ['l'] = 'л', ['m'] = 'м', ['n'] = 'н', ['o'] = 'о',
        ['p'] = 'п', ['q'] = 'қ', ['r'] = 'р', ['s'] = 'с', ['t'] = 'т', ['u'] = 'у', ['v'] = 'в',
        ['w'] = 'в', ['x'] = 'х', ['y'] = 'й', ['z'] = 'з'
    };

    // Order matters: e-mail must win over @username, which would otherwise match its domain part.
    [GeneratedRegex(
        @"`[^`]*`|(?:https?://|www\.)\S+|[\p{L}\p{N}._%+-]+@[\p{L}\p{N}-]+(?:\.[\p{L}\p{N}-]+)+|[@#][\p{L}\p{N}_]+",
        RegexOptions.IgnoreCase)]
    private static partial Regex ProtectedSpanRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    public static string ToLatin(string? text) => ConvertOutsideProtectedSpans(text, ToLatinSegment);

    public static string ToCyrillic(string? text) => ConvertOutsideProtectedSpans(text, ToCyrillicSegment);

    /// <summary>
    /// Script-independent search key: Latin, lower case, one apostrophe form, single spaces.
    /// "Ўзбек", "o‘zbek", "oʻzbek" and "O'zbek" all become "o'zbek".
    /// </summary>
    public static string NormalizeForSearch(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;

        var latin = ToLatin(text).ToLowerInvariant();
        var builder = new StringBuilder(latin.Length);
        foreach (var ch in latin) builder.Append(IsApostrophe(ch) ? '\'' : ch);
        return WhitespaceRegex().Replace(builder.ToString(), " ").Trim();
    }

    public static bool IsApostrophe(char ch) => Apostrophes.Contains(ch);

    private static string ConvertOutsideProtectedSpans(string? text, Func<string, string> convert)
    {
        if (string.IsNullOrEmpty(text)) return text ?? string.Empty;

        var builder = new StringBuilder(text.Length + 8);
        var position = 0;
        foreach (Match match in ProtectedSpanRegex().Matches(text))
        {
            builder.Append(convert(text[position..match.Index]));
            builder.Append(match.Value);
            position = match.Index + match.Length;
        }

        builder.Append(convert(text[position..]));
        return builder.ToString();
    }

    private static string ToLatinSegment(string text)
    {
        var builder = new StringBuilder(text.Length + 8);
        for (var i = 0; i < text.Length; i++)
        {
            var ch = text[i];
            var lower = char.ToLowerInvariant(ch);
            var previous = i > 0 ? char.ToLowerInvariant(text[i - 1]) : '\0';

            string latin;
            if (lower == 'е')
            {
                var soundsYe = !IsWordChar(text, i - 1) || CyrillicVowels.Contains(previous) || previous is 'ъ' or 'ь';
                latin = soundsYe ? "ye" : "e";
            }
            else if (lower == 'ц')
            {
                latin = CyrillicVowels.Contains(previous) ? "ts" : "s";
            }
            else if (lower == 'ҳ' && previous == 'с')
            {
                // "сҳ" (Исҳоқ) is written with a tutuq in Latin so that it is not read as "ш".
                latin = TutuqMark + "h";
            }
            else if (!CyrillicToLatinMap.TryGetValue(lower, out latin!))
            {
                builder.Append(ch);
                continue;
            }

            builder.Append(ch == lower ? latin : ApplyUpperCase(latin, IsUpperCaseWord(text, i)));
        }

        return builder.ToString();
    }

    private static string ToCyrillicSegment(string text)
    {
        var lower = text.ToLowerInvariant();
        var tsPositions = FindExceptionTsPositions(lower);
        var builder = new StringBuilder(text.Length);
        var i = 0;
        while (i < text.Length)
        {
            var c = lower[i];
            var next = i + 1 < text.Length ? lower[i + 1] : '\0';
            var afterNext = i + 2 < text.Length ? lower[i + 2] : '\0';
            var isUpper = text[i] != c;

            string? cyrillic = null;
            var length = 1;
            if (c == 's' && next == 'h') (cyrillic, length) = ("ш", 2);
            else if (c == 's' && IsApostrophe(next) && afterNext == 'h') (cyrillic, length) = ("с", 2);
            else if (c == 'c' && next == 'h') (cyrillic, length) = ("ч", 2);
            else if (c == 'o' && IsApostrophe(next)) (cyrillic, length) = ("ў", 2);
            else if (c == 'g' && IsApostrophe(next)) (cyrillic, length) = ("ғ", 2);
            else if (c == 'y' && next == 'o' && !IsApostrophe(afterNext)) (cyrillic, length) = ("ё", 2);
            else if (c == 'y' && next == 'u') (cyrillic, length) = ("ю", 2);
            else if (c == 'y' && next == 'a') (cyrillic, length) = ("я", 2);
            else if (c == 'y' && next == 'e') (cyrillic, length) = ("е", 2);
            else if (c == 't' && next == 's' && tsPositions.Contains(i)) (cyrillic, length) = ("ц", 2);
            else if (c == 'e')
            {
                // Cyrillic "е" after a vowel or at a word start is written "ye", so a bare "e" there was "э".
                var previous = i > 0 ? lower[i - 1] : '\0';
                cyrillic = !IsWordChar(text, i - 1) || LatinVowels.Contains(previous) ? "э" : "е";
            }
            else if (IsApostrophe(c) && IsWordChar(text, i - 1) && IsLetter(text, i + 1))
            {
                cyrillic = IsUpperCaseWord(text, i) ? "Ъ" : "ъ";
            }
            else if (LatinToCyrillicMap.TryGetValue(c, out var single))
            {
                cyrillic = single.ToString();
            }

            if (cyrillic is null)
            {
                builder.Append(text[i]);
            }
            else
            {
                builder.Append(isUpper ? cyrillic.ToUpperInvariant() : cyrillic);
            }

            i += length;
        }

        return builder.ToString();
    }

    private static HashSet<int> FindExceptionTsPositions(string lowerText)
    {
        var positions = new HashSet<int>();
        foreach (var stem in TsExceptionStems)
        {
            for (var start = lowerText.IndexOf(stem, StringComparison.Ordinal); start >= 0; start = lowerText.IndexOf(stem, start + 1, StringComparison.Ordinal))
            {
                for (var ts = stem.IndexOf("ts", StringComparison.Ordinal); ts >= 0; ts = stem.IndexOf("ts", ts + 2, StringComparison.Ordinal))
                {
                    positions.Add(start + ts);
                }
            }
        }

        return positions;
    }

    private static string ApplyUpperCase(string latin, bool wholeWordUpper)
    {
        if (latin.Length == 0) return latin;
        return wholeWordUpper ? latin.ToUpperInvariant() : char.ToUpperInvariant(latin[0]) + latin[1..];
    }

    private static bool IsLetter(string text, int index) =>
        index >= 0 && index < text.Length && char.IsLetter(text[index]) && !IsApostrophe(text[index]);

    private static bool IsWordChar(string text, int index) =>
        index >= 0 && index < text.Length && (char.IsLetter(text[index]) || IsApostrophe(text[index]));

    /// <summary>True when the word around <paramref name="index"/> has at least two cased letters, all upper case.</summary>
    private static bool IsUpperCaseWord(string text, int index)
    {
        var start = index;
        while (IsWordChar(text, start - 1)) start--;
        var end = index;
        while (IsWordChar(text, end + 1)) end++;

        var casedLetters = 0;
        for (var i = start; i <= end; i++)
        {
            var ch = text[i];
            if (char.ToUpperInvariant(ch) == char.ToLowerInvariant(ch)) continue;
            if (!char.IsUpper(ch)) return false;
            casedLetters++;
        }

        return casedLetters >= 2;
    }
}
