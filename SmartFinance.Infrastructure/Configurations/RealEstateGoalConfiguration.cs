using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class RealEstateGoalConfiguration : IEntityTypeConfiguration<RealEstateGoal>
{
    public void Configure(EntityTypeBuilder<RealEstateGoal> builder)
    {
        builder.ToTable("RealEstateGoals");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.UserId).IsRequired();

        builder.Property(r => r.Title).IsRequired().HasMaxLength(150);
        builder.Property(r => r.TotalValue).HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.DownPaymentTarget).HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.FinancingStartDate).HasColumnType("date").IsRequired();
    }
}
