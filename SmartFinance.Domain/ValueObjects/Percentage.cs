namespace SmartFinance.Domain.ValueObjects;

public record Percentage
{
    public decimal Value { get; init; }

    public Percentage(decimal value)
    {
        if (value is < 0 or > 1)
            throw new ArgumentException("Percentage value must be between 0 and 1");
        Value = value;
    }
}
