using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class AccountReconciliationConfiguration
    : IEntityTypeConfiguration<AccountReconciliation>
{
    public void Configure(EntityTypeBuilder<AccountReconciliation> builder)
    {
        builder.ToTable("AccountReconciliations");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).IsRequired();
        builder.Property(r => r.AccountId).IsRequired();

        builder.Property(r => r.ClosingDate).HasColumnType("date").IsRequired();

        builder.Property(r => r.ExpectedLedgerBalance).HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(r => r.ActualBankBalance).HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(r => r.Difference).HasColumnType("decimal(18,2)").IsRequired();

        builder.Property(r => r.AdjustmentTransactionId).IsRequired(false);

        builder
            .HasOne(r => r.Account)
            .WithMany()
            .HasForeignKey(r => r.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => new { r.AccountId, r.ClosingDate }).IsUnique();
    }
}
