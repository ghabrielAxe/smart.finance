using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Domain.Entities;

public class IndexRate : BaseEntity
{
    public string IndexName { get; private set; } // Ex: "INCC-M", "IPCA"
    public DateTime ReferenceMonth { get; private set; }
    public Percentage Rate { get; private set; } // A variação do mês (Ex: 0.5%)

    protected IndexRate() { }

    public IndexRate(string indexName, DateTime referenceMonth, Percentage rate)
    {
        IndexName = indexName;
        ReferenceMonth = new DateTime(
            referenceMonth.Year,
            referenceMonth.Month,
            1
        ).ToUniversalTime();
        Rate = rate;
    }
}
