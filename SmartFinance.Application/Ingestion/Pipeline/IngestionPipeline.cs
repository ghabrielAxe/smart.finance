using System.Text.Json;
using SmartFinance.Application.Ingestion.Engines;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.Ingestion.Pipeline;

public interface IIngestionPipeline
{
    Task ProcessEventAsync(Guid eventLogId, CancellationToken cancellationToken);
}

public sealed class IngestionPipeline(
    IFinancialEventLogRepository eventLogRepository,
    ITransactionRepository transactionRepository,
    ICategoryRuleRepository categoryRuleRepository,
    IBankAccountMappingRepository bankAccountMappingRepository,
    IAccountRepository accountRepository,
    IUnitOfWork unitOfWork,
    EmailExtractionEngine extractionEngine,
    CategorizationEngine categorizationEngine
) : IIngestionPipeline
{
    public async Task ProcessEventAsync(Guid eventLogId, CancellationToken cancellationToken)
    {
        var eventLog = await eventLogRepository.GetByIdAsync(eventLogId, cancellationToken);
        if (eventLog == null || eventLog.Status != EventProcessingStatus.Pending)
            return;

        try
        {
            using var document = JsonDocument.Parse(eventLog.RawPayload);
            var root = document.RootElement;

            var emailId = root.TryGetProperty("id", out var idProp)
                ? idProp.GetString() ?? eventLog.Id.ToString()
                : eventLog.Id.ToString();
            var subject = root.TryGetProperty("subject", out var subProp)
                ? subProp.GetString() ?? ""
                : "";
            var body = root.TryGetProperty("body", out var bodyProp)
                ? bodyProp.GetString() ?? ""
                : "";
            var from = root.TryGetProperty("from", out var fromProp)
                ? fromProp.GetString() ?? ""
                : "";

            var extracted = extractionEngine.Extract(
                emailId,
                subject,
                body,
                from,
                eventLog.CreatedAt
            );

            if (extracted == null)
            {
                eventLog.MarkAsFailed(
                    "O email não corresponde a nenhum padrão de transação conhecido."
                );
                await unitOfWork.CommitAsync(cancellationToken);
                return;
            }

            if (
                await transactionRepository.ExistsByIdempotencyKeyAsync(
                    extracted.IdempotencyKey,
                    cancellationToken
                )
            )
            {
                eventLog.MarkAsProcessed();
                await unitOfWork.CommitAsync(cancellationToken);
                return;
            }

            var rules = await categoryRuleRepository.GetAllAsync(cancellationToken);
            var categoryId = categorizationEngine.MatchCategory(extracted.Merchant, rules);

            var accountMapping = await bankAccountMappingRepository.GetByBankNameAsync(
                extracted.SourceBank,
                cancellationToken
            );
            if (accountMapping == null)
                throw new InvalidOperationException($"Banco desconhecido: {extracted.SourceBank}");

            var expenseAccountId = await accountRepository.GetSystemExpenseAccountIdAsync(
                cancellationToken
            );

            var money = new Money(extracted.Amount, extracted.Currency);
            var transaction = Transaction.CreateExpense(
                extracted.TransactionDate,
                extracted.Merchant,
                accountMapping.AccountId,
                expenseAccountId,
                money,
                categoryId,
                extracted.IdempotencyKey
            );

            await transactionRepository.AddAsync(transaction, cancellationToken);
            eventLog.MarkAsProcessed();

            await unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            eventLog.MarkAsFailed(ex.Message);
            await unitOfWork.CommitAsync(cancellationToken);
        }
    }
}
