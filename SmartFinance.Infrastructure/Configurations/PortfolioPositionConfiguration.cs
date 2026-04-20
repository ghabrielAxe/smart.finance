using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Data.Configurations;

public class PortfolioPositionConfiguration : IEntityTypeConfiguration<PortfolioPosition>
{
    public void Configure(EntityTypeBuilder<PortfolioPosition> builder)
    {
        builder.ToTable("PortfolioPositions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.AccountId).IsRequired();
        builder.Property(p => p.AssetId).IsRequired();

        builder
            .Property(p => p.Quantity)
            .HasColumnType("decimal(18,8)")
            .IsRequired();

        builder
            .Property(p => p.AverageCost)
            .HasColumnType("decimal(18,8)")
            .IsRequired();
        
        builder.UseXminAsConcurrencyToken();

        builder.HasIndex(p => new { p.AccountId, p.AssetId }).IsUnique();
    }
}
