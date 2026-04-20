using FluentValidation;
using MediatR;
using SmartFinance.Domain.Entities;
using SmartFinance.Domain.Repositories;
using SmartFinance.Domain.ValueObjects;

namespace SmartFinance.Application.Investments.Commands;

public record SellAssetCommand(
    Guid AccountId,
    Guid AssetId,
    decimal Quantity,
    decimal UnitPrice,
    decimal FeesAndTaxes,
    DateTime TradeDate,
    string IdempotencyKey
) : IRequest<Guid>;

public class SellAssetCommandValidator : AbstractValidator<SellAssetCommand>
{
    public SellAssetCommandValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantidade vendida deve ser maior que zero.");
        RuleFor(x => x.UnitPrice)
            .GreaterThan(0)
            .WithMessage("Preço de venda deve ser maior que zero.");
        RuleFor(x => x.FeesAndTaxes).GreaterThanOrEqualTo(0);
        RuleFor(x => x.TradeDate).NotEmpty().LessThanOrEqualTo(DateTime.UtcNow);
        RuleFor(x => x.IdempotencyKey).NotEmpty();
    }
}

public class SellAssetCommandHandler : IRequestHandler<SellAssetCommand, Guid>
{
    private readonly IPortfolioRepository _portfolioRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SellAssetCommandHandler(
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

    public async Task<Guid> Handle(SellAssetCommand request, CancellationToken cancellationToken)
    {
        if (
            await _transactionRepository.ExistsByIdempotencyKeyAsync(
                request.IdempotencyKey,
                cancellationToken
            )
        )
            throw new InvalidOperationException("Esta venda já foi processada (Idempotência).");

        var position = await _portfolioRepository.GetByAccountAndAssetAsync(
            request.AccountId,
            request.AssetId,
            cancellationToken
        );

        if (position == null || position.Quantity < request.Quantity)
            throw new InvalidOperationException(
                "Saldo insuficiente em custódia para realizar esta venda."
            );
        
        var gainResult = position.RecordSell(request.Quantity, request.UnitPrice);

        var netRealizedPnL = gainResult.PnL - request.FeesAndTaxes;

        var trade = new Trade(
            position.Id,
            TradeType.Sell,
            request.TradeDate,
            request.Quantity,
            request.UnitPrice,
            request.FeesAndTaxes,
            netRealizedPnL
        );

        await _tradeRepository.AddAsync(trade, cancellationToken);
        
        var totalSaleGross = request.Quantity * request.UnitPrice;
        var netCashInflow = totalSaleGross - request.FeesAndTaxes; 

        var cashTransaction = new Transaction(
            request.TradeDate,
            $"Venda de Ativo via Trade {trade.Id} (Lucro: {netRealizedPnL:C2})",
            null,
            null,
            request.IdempotencyKey
        );

        var moneyIn = new Money(netCashInflow, "BRL");
        cashTransaction.AddEntry(request.AccountId, moneyIn, EntryType.Debit);

        await _transactionRepository.AddAsync(cashTransaction, cancellationToken);
        
        await _unitOfWork.CommitAsync(cancellationToken);

        return trade.Id;
    }
}
