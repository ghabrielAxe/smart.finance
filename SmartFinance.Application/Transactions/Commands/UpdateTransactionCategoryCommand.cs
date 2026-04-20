using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.Services;

namespace SmartFinance.Application.Transactions.Commands;

public record UpdateTransactionCategoryCommand(
    Guid TransactionId,
    Guid NewCategoryId,
    bool ApplyToFuture = true
) : IRequest<bool>;

public sealed class UpdateTransactionCategoryCommandHandler(
    ITransactionRepository transactionRepository,
    ICategoryRuleRepository categoryRuleRepository,
    IUnitOfWork unitOfWork,
    CategoryLearningService learningService
) : IRequestHandler<UpdateTransactionCategoryCommand, bool>
{
    public async Task<bool> Handle(
        UpdateTransactionCategoryCommand request,
        CancellationToken cancellationToken
    )
    {
        var transaction = await transactionRepository.GetByIdAsync(
            request.TransactionId,
            cancellationToken
        );
        if (transaction == null)
            return false;

        transaction.UpdateBasicInfo(
            transaction.Date,
            transaction.Description,
            request.NewCategoryId
        );
        await transactionRepository.UpdateAsync(transaction, cancellationToken);

        if (request.ApplyToFuture)
        {
            var keyword = learningService.ExtractCleanKeyword(transaction.Description);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var existingRule = await categoryRuleRepository.GetByKeywordAsync(
                    keyword,
                    cancellationToken
                );

                if (existingRule != null)
                {
                    existingRule.UpdateCategory(request.NewCategoryId);
                    await categoryRuleRepository.UpdateAsync(existingRule, cancellationToken);
                }
                else
                {
                    var newRule = new CategoryRule(request.NewCategoryId, keyword, priority: 1);
                    await categoryRuleRepository.AddAsync(newRule, cancellationToken);
                }
            }
        }

        return await unitOfWork.CommitAsync(cancellationToken);
    }
}
