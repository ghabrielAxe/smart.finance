using Microsoft.EntityFrameworkCore;
using SmartFinance.Application.Interfaces;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Data;

public class SmartFinanceDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public SmartFinanceDbContext(
        DbContextOptions<SmartFinanceDbContext> options,
        ICurrentUserService currentUserService
    )
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<AccountReconciliation> AccountReconciliations { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<LedgerEntry> LedgerEntries { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Budget> Budgets { get; set; }
    public DbSet<InstallmentPlan> InstallmentPlans { get; set; }
    public DbSet<Installment> Installments { get; set; }
    public DbSet<Insight> Insights { get; set; }
    public DbSet<RealEstateContract> RealEstateContracts { get; set; }
    public DbSet<ConstructionInstallment> ConstructionInstallments { get; set; }
    public DbSet<BalloonPayment> BalloonPayments { get; set; }
    public DbSet<Mortgage> Mortgages { get; set; }
    public DbSet<MortgageInstallment> MortgageInstallments { get; set; }
    public DbSet<IndexRate> IndexRates { get; set; }
    public DbSet<Asset> Assets { get; set; }
    public DbSet<PortfolioPosition> PortfolioPositions { get; set; }
    public DbSet<Trade> Trades { get; set; }
    public DbSet<FinancialEventLog> FinancialEventLogs { get; set; }
    public DbSet<CategoryRule> CategoryRules { get; set; }
    public DbSet<BankAccountMapping> BankAccountMappings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartFinanceDbContext).Assembly);

        modelBuilder.Entity<Account>().HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder
            .Entity<AccountReconciliation>()
            .HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder
            .Entity<Transaction>()
            .HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder.Entity<Category>().HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder.Entity<Budget>().HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder
            .Entity<InstallmentPlan>()
            .HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder.Entity<Insight>().HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder
            .Entity<RealEstateContract>()
            .HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder.Entity<Asset>().HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder
            .Entity<PortfolioPosition>()
            .HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder
            .Entity<CategoryRule>()
            .HasQueryFilter(e => e.UserId == _currentUserService.UserId);
        modelBuilder
            .Entity<BankAccountMapping>()
            .HasQueryFilter(e => e.UserId == _currentUserService.UserId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var userId = _currentUserService.UserId;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (userId != Guid.Empty)
                    entry.Entity.SetUser(userId);
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.SetUpdatedAt();
                entry.Property(nameof(BaseEntity.UserId)).IsModified = false;
                entry.Property(nameof(BaseEntity.CreatedAt)).IsModified = false;
            }
        }

        var transactionEntries = ChangeTracker
            .Entries<Transaction>()
            .Where(e =>
                e.State == EntityState.Added
                || e.State == EntityState.Modified
                || e.State == EntityState.Deleted
            )
            .ToList();

        if (transactionEntries.Any())
        {
            var affectedAccountIds = transactionEntries
                .SelectMany(e => e.Entity.Entries)
                .Select(le => le.AccountId)
                .Distinct()
                .ToList();

            if (affectedAccountIds.Any())
            {
                var lockedAccounts = await Accounts
                    .Where(a => affectedAccountIds.Contains(a.Id) && a.LockedUntil != null)
                    .AsNoTracking()
                    .ToDictionaryAsync(a => a.Id, a => a.LockedUntil, cancellationToken);

                foreach (var entry in transactionEntries)
                {
                    var txDate = entry.Entity.Date;

                    foreach (var ledgerEntry in entry.Entity.Entries)
                    {
                        if (
                            lockedAccounts.TryGetValue(ledgerEntry.AccountId, out var lockedUntil)
                            && txDate <= lockedUntil
                        )
                        {
                            throw new InvalidOperationException(
                                $"Violação Contábil Crítica: Não é possível inserir, alterar ou deletar transações anteriores a {lockedUntil:dd/MM/yyyy} na conta {ledgerEntry.AccountId}, pois o período já encontra-se conciliado e auditado."
                            );
                        }
                    }
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
