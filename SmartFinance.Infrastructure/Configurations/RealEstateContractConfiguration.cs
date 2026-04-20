using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class RealEstateContractConfiguration : IEntityTypeConfiguration<RealEstateContract>
{
    public void Configure(EntityTypeBuilder<RealEstateContract> builder)
    {
        builder.ToTable("RealEstateContracts");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).IsRequired();

        builder.Property(r => r.PropertyName).IsRequired().HasMaxLength(150);

        builder.Property(r => r.ContractDate).HasColumnType("date").IsRequired();
        builder.Property(r => r.ExpectedDeliveryDate).HasColumnType("date").IsRequired();

        builder.OwnsOne(
            r => r.PropertyValue,
            money =>
            {
                money
                    .Property(m => m.Amount)
                    .HasColumnName("PropertyValue_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(m => m.Currency)
                    .HasColumnName("PropertyValue_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder
            .HasMany(r => r.ConstructionInstallments)
            .WithOne(c => c.Contract)
            .HasForeignKey(c => c.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(r => r.BalloonPayments)
            .WithOne(b => b.Contract)
            .HasForeignKey(b => b.ContractId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(r => r.Mortgage)
            .WithOne(m => m.Contract)
            .HasForeignKey<Mortgage>(m => m.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
