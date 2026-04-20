using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class MortgageInstallmentConfiguration : IEntityTypeConfiguration<MortgageInstallment>
{
    public void Configure(EntityTypeBuilder<MortgageInstallment> builder)
    {
        builder.ToTable("MortgageInstallments");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.InstallmentNumber).IsRequired();
        builder.Property(m => m.DueDate).HasColumnType("date").IsRequired();
        builder.Property(m => m.IsPaid).IsRequired();

        builder.OwnsOne(
            m => m.PrincipalAmortization,
            money =>
            {
                money
                    .Property(x => x.Amount)
                    .HasColumnName("Amortization_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(x => x.Currency)
                    .HasColumnName("Amortization_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            m => m.InterestAmount,
            money =>
            {
                money
                    .Property(x => x.Amount)
                    .HasColumnName("Interest_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(x => x.Currency)
                    .HasColumnName("Interest_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            m => m.TotalAmount,
            money =>
            {
                money
                    .Property(x => x.Amount)
                    .HasColumnName("Total_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(x => x.Currency)
                    .HasColumnName("Total_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            m => m.RemainingBalance,
            money =>
            {
                money
                    .Property(x => x.Amount)
                    .HasColumnName("RemainingBalance_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(x => x.Currency)
                    .HasColumnName("RemainingBalance_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );
        
        builder.HasIndex(m => new { m.MortgageId, m.InstallmentNumber }).IsUnique();
    }
}
