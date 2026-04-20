namespace SmartFinance.Domain.ValueObjects;

public record Money
{
    public decimal Amount { get; init; }
    public string Currency { get; init; }

    public Money(decimal amount, string currency = "BRL")
    {
        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(string currency = "BRL") => new(0, currency);

    public static Money operator +(Money a, Money b) =>
        a.Currency == b.Currency
            ? new Money(a.Amount + b.Amount, a.Currency)
            : throw new InvalidOperationException("Currency mismatch");

    public static Money operator -(Money a, Money b) =>
        a.Currency == b.Currency
            ? new Money(a.Amount - b.Amount, a.Currency)
            : throw new InvalidOperationException("Currency mismatch");
}
