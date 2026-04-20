using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class ConstructionInstallmentConfiguration
    : IEntityTypeConfiguration<ConstructionInstallment>
{
    public void Configure(EntityTypeBuilder<ConstructionInstallment> builder)
    {
        builder.ToTable("ConstructionInstallments");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.DueDate).HasColumnType("date").IsRequired();
        builder.Property(c => c.IsPaid).IsRequired();

        builder.OwnsOne(
            c => c.BaseAmount,
            money =>
            {
                money
                    .Property(m => m.Amount)
                    .HasColumnName("BaseAmount_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(m => m.Currency)
                    .HasColumnName("BaseAmount_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            c => c.AdjustedAmount,
            money =>
            {
                money
                    .Property(m => m.Amount)
                    .HasColumnName("AdjustedAmount_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(m => m.Currency)
                    .HasColumnName("AdjustedAmount_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );
        
        builder.HasIndex(c => new
        {
            c.ContractId,
            c.IsPaid,
            c.DueDate,
        });
    }
}
