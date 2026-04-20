using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using SmartFinance.Application.Ingestion.Interfaces;
using SmartFinance.Application.Ingestion.Models;

namespace SmartFinance.Application.Ingestion.Extractors;

public sealed class NubankEmailExtractor : IEmailExtractor
{
    private static readonly Regex AmountRegex = new(
        @"R\$\s*([\d\.,]+)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );
    private static readonly Regex MerchantRegex = new(
        @"em\s+(.+?)(?:\n|$|\r)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled
    );

    public bool CanHandle(string fromEmail, string subject, string body)
    {
        return fromEmail.EndsWith("@nubank.com.br", StringComparison.OrdinalIgnoreCase)
            && (
                subject.Contains("Compra aprovada", StringComparison.OrdinalIgnoreCase)
                || body.Contains("Compra aprovada", StringComparison.OrdinalIgnoreCase)
            );
    }

    public ExtractedTransaction? Extract(
        string eventId,
        string subject,
        string body,
        string fromEmail,
        DateTime date
    )
    {
        var fullText = $"{subject}\n{body}";

        var amountMatch = AmountRegex.Match(fullText);
        var merchantMatch = MerchantRegex.Match(fullText);

        if (!amountMatch.Success || !merchantMatch.Success)
            return null;

        var amountString = amountMatch.Groups[1].Value.Replace(".", "").Replace(",", ".");
        if (!decimal.TryParse(amountString, out decimal amount))
            return null;

        var merchant = merchantMatch.Groups[1].Value.Trim();

        var idempotencyKey = GenerateSha256($"Nubank-{eventId}");

        return new ExtractedTransaction("Nubank", amount, merchant, date, idempotencyKey);
    }

    private static string GenerateSha256(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
