using System.Text.RegularExpressions;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Application.Ingestion.Engines;

public sealed class CategorizationEngine
{
    public Guid? MatchCategory(string merchantDescription, IEnumerable<CategoryRule> activeRules)
    {
        var description = merchantDescription.ToLowerInvariant();

        foreach (var rule in activeRules.OrderByDescending(r => r.Priority))
        {
            var pattern = $@"\b{Regex.Escape(rule.Keyword)}\b";

            if (
                Regex.IsMatch(
                    description,
                    pattern,
                    RegexOptions.IgnoreCase | RegexOptions.CultureInvariant
                )
            )
            {
                return rule.CategoryId;
            }
        }

        return null;
    }
}
