using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.Investments.Commands;

public record BuyAssetCommand(
    Guid AccountId,
    Guid AssetId,
    decimal Quantity,
    decimal UnitPrice,
    decimal FeesAndTaxes,
    DateTime TradeDate,
    string IdempotencyKey
) : IRequest<Guid>;

public class BuyAssetCommandValidator : AbstractValidator<BuyAssetCommand>
{
    public BuyAssetCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero.");
        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Preço unitário deve ser maior que zero.");
        RuleFor(x => x.FeesAndTaxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TradeDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.UtcNow)
            .WithMessage("Não é possível registrar trades no futuro.");
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}

public class BuyAssetCommandHandler : IRequestHandler<BuyAssetCommand, Guid>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BuyAssetCommandHandler(
        IPortfolioRepository portfolioRepository,
        ITradeRepository tradeRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork
    )
    {
        _portfolioRepository = portfolioRepository;
        _tradeRepository = tradeRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(BuyAssetCommand request, CancellationToken cancellationToken)
    {
        if (
            await _transactionRepository.ExistsByIdempotencyKeyAsync(
                request.IdempotencyKey,
                cancellationToken
            )
        )
            throw new InvalidOperationException("Esta transação já foi processada (Idempotência).");

        var totalAssetCost = request.Quantity * request.UnitPrice;
        var totalCashOutflow = totalAssetCost + request.FeesAndTaxes;
        
        var position = await _portfolioRepository.GetByAccountAndAssetAsync(
            request.AccountId,
            request.AssetId,
            cancellationToken
        );

        if (position == null)
        {
            position = new PortfolioPosition(request.AccountId, request.AssetId);
            await _portfolioRepository.AddAsync(position, cancellationToken);
        }

        position.RecordBuy(request.Quantity, request.UnitPrice);

        var trade = new Trade(
            position.Id,
            TradeType.Buy,
            request.TradeDate,
            request.Quantity,
            request.UnitPrice,
            request.FeesAndTaxes
        );

        await _tradeRepository.AddAsync(trade, cancellationToken);


        var cashTransaction = new Transaction(
            request.TradeDate,
            $"Compra de Ativo via Trade {trade.Id}",
            null,
            null,
            request.IdempotencyKey
        );

        var totalMoney = new Money(totalCashOutflow, "BRL");
        cashTransaction.AddEntry(request.AccountId, totalMoney, EntryType.Credit);

        await _transactionRepository.AddAsync(cashTransaction, cancellationToken);

        await _unitOfWork.CommitAsync(cancellationToken);

        return trade.Id;
    }
}
