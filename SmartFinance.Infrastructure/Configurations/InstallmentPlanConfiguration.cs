using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class InstallmentPlanConfiguration : IEntityTypeConfiguration<InstallmentPlan>
{
    public void Configure(EntityTypeBuilder<InstallmentPlan> builder)
    {
        builder.ToTable("InstallmentPlans");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.UserId).IsRequired();

        builder.Property(i => i.Description).IsRequired().HasMaxLength(200);

        builder.OwnsOne(
            i => i.TotalAmount,
            money =>
            {

                money
                    .Property(m => m.Amount)
                    .HasColumnName("TotalAmount_Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                money
                    .Property(m => m.Currency)
                    .HasColumnName("TotalAmount_Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            }
        );

        builder
            .HasMany(p => p.Installments)
            .WithOne(i => i.InstallmentPlan)
            .HasForeignKey(i => i.InstallmentPlanId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => new { p.UserId, p.CreatedAt });
    }
}
