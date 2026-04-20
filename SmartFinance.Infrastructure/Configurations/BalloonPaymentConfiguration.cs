using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public class BalloonPaymentConfiguration : IEntityTypeConfiguration<BalloonPayment>
{
    public void Configure(EntityTypeBuilder<BalloonPayment> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.Description).IsRequired().HasMaxLength(100);
        builder.Property(b => b.DueDate).IsRequired();
        builder.Property(b => b.IsPaid).IsRequired();

        builder.OwnsOne(
            b => b.BaseAmount,
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
            b => b.AdjustedAmount,
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
    }
}
