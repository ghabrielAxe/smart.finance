namespace SmartFinance.Application.Ingestion.Models;

public record ExtractedTransaction(
    string SourceBank,
    decimal Amount,
    string Merchant,
    DateTime TransactionDate,
    string IdempotencyKey,
    string Currency = "BRL"
);
