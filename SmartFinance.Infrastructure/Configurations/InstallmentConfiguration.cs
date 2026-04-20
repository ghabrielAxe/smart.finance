using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class InstallmentConfiguration : IEntityTypeConfiguration<Installment>
{
    public void Configure(EntityTypeBuilder<Installment> builder)
    {
        builder.ToTable("Installments");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.DueDate).HasColumnType("date").IsRequired();
        builder.Property(i => i.IsPaid).IsRequired();

        builder.OwnsOne(
            i => i.Amount,
            money =>
            {
                money
                    .Property(m => m.Amount)
                    .HasColumnName("Amount_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(m => m.Currency)
                    .HasColumnName("Amount_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );
        
        builder.HasIndex(i => new
        {
            i.InstallmentPlanId,
            i.IsPaid,
            i.DueDate,
        });
    }
}
