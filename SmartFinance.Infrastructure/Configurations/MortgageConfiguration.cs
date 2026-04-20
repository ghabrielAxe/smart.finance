using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public class MortgageConfiguration : IEntityTypeConfiguration<Mortgage>
{
    public void Configure(EntityTypeBuilder<Mortgage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.BankName).IsRequired().HasMaxLength(100);
        builder.Property(m => m.TermMonths).IsRequired();
        builder.Property(m => m.StartDate).IsRequired();

        builder.OwnsOne(
            m => m.PrincipalAmount,
            money =>
            {
                money
                    .Property(x => x.Amount)
                    .HasColumnName("PrincipalAmount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(x => x.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder.OwnsOne(
            m => m.AnnualInterestRate,
            percentage =>
            {
                percentage
                    .Property(p => p.Value)
                    .HasColumnName("AnnualInterestRate")
                    .HasPrecision(5, 4)
                    .IsRequired();
            }
        );

        builder
            .HasMany(m => m.Installments)
            .WithOne(i => i.Mortgage)
            .HasForeignKey(i => i.MortgageId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
