namespace SmartFinance.Domain.Services;

public enum CashflowEventType
{
    Income,
    FixedExpense,
    DebtInstallment,
    InvestmentAport,
}

public record CashflowEvent(
    DateTime Date,
    decimal Amount,
    CashflowEventType Type,
    string Description
);

public record ProjectionResult(
    DateTime TargetDate,
    decimal ProjectedBalance,
    bool IsAlertState,
    int DaysOfCashLeft
);

public interface ICashflowProjectionService
{
    ProjectionResult ProjectBalanceToDate(
        decimal currentBalance,
        DateTime targetDate,
        IEnumerable<CashflowEvent> events,
        decimal averageDailyVariableExpense
    );
}

public class CashflowProjectionService : ICashflowProjectionService
{
    public ProjectionResult ProjectBalanceToDate(
        decimal currentBalance,
        DateTime targetDate,
        IEnumerable<CashflowEvent> events,
        decimal averageDailyVariableExpense
    )
    {
        var today = DateTime.UtcNow.Date;
        var target = targetDate.Date;

        if (target < today)
            throw new ArgumentException("A data alvo deve ser no futuro.");

        var projectedBalance = currentBalance;
        var orderedEvents = events
            .Where(e => e.Date.Date >= today && e.Date.Date <= target)
            .OrderBy(e => e.Date)
            .ToList();

        var currentDate = today;

        while (currentDate <= target)
        {
            projectedBalance -= averageDailyVariableExpense;

            var dailyEvents = orderedEvents.Where(e => e.Date.Date == currentDate);
            foreach (var ev in dailyEvents)
            {
                projectedBalance += ev.Type == CashflowEventType.Income ? ev.Amount : -ev.Amount;
            }

            currentDate = currentDate.AddDays(1);
        }

        var daysOfCashLeft =
            averageDailyVariableExpense > 0
                ? (int)Math.Max(0, projectedBalance / averageDailyVariableExpense)
                : 999;

        var isAlertState = daysOfCashLeft < 7;

        return new ProjectionResult(target, projectedBalance, isAlertState, daysOfCashLeft);
    }
}
