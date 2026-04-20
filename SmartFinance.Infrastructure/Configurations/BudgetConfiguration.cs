using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.ToTable("Budgets");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserId).IsRequired();

        builder.Property(b => b.CategoryId).IsRequired();

        builder
            .Property(b => b.RolloverType)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.OwnsOne(
            b => b.MonthlyLimit,
            limit =>
            {
                limit
                    .Property(m => m.Amount)
                    .HasColumnName("MonthlyLimit_Amount")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                limit
                    .Property(m => m.Currency)
                    .HasColumnName("MonthlyLimit_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            b => b.WarningThreshold,
            warning =>
            {
                warning
                    .Property(p => p.Value)
                    .HasColumnName("WarningThreshold_Value")
                    .HasColumnType("decimal(5,4)")
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            b => b.CriticalThreshold,
            critical =>
            {
                critical
                    .Property(p => p.Value)
                    .HasColumnName("CriticalThreshold_Value")
                    .HasColumnType("decimal(5,4)")
                    .IsRequired();
            }
        );

        builder
            .HasOne(b => b.Category)
            .WithMany()
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
