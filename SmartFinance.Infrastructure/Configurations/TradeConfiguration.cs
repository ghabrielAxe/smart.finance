using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public class TradeConfiguration : IEntityTypeConfiguration<Trade>
{
    public void Configure(EntityTypeBuilder<Trade> builder)
    {
        builder.ToTable("Trades");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.PortfolioPositionId).IsRequired();
        builder.Property(t => t.Type).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(t => t.Date).IsRequired();

        builder.Property(t => t.Quantity).HasColumnType("decimal(18,8)").IsRequired();
        builder.Property(t => t.UnitPrice).HasColumnType("decimal(18,8)").IsRequired();
        builder.Property(t => t.FeesAndTaxes).HasColumnType("decimal(18,2)").IsRequired();
        builder.Property(t => t.RealizedPnL).HasColumnType("decimal(18,2)");
    }
}
