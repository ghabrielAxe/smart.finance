using System.Text.RegularExpressions;

namespace SmartFinance.Domain.Services;

public sealed class CategoryLearningService
{
    private static readonly Regex NoiseRegex = new(
        @"\d{2}/\d{2}|[*#-]|(?:\b\d{4,}\b)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string ExtractCleanKeyword(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return string.Empty;

        var cleaned = NoiseRegex.Replace(description, " ");

        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim().ToLowerInvariant();


        var words = cleaned.Split(' ');
        return words.Length > 1 ? $"{words[0]} {words[1]}" : words[0];
    }
}
