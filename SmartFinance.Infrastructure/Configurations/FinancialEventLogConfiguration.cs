using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class FinancialEventLogConfiguration : IEntityTypeConfiguration<FinancialEventLog>
{
    public void Configure(EntityTypeBuilder<FinancialEventLog> builder)
    {
        builder.ToTable("FinancialEventLogs");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired();

        builder.Property(e => e.Source).IsRequired().HasMaxLength(100);

        builder.Property(e => e.Type).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(e => e.RawPayload).HasColumnType("jsonb").IsRequired();
        builder.Property(e => e.ErrorMessage).HasMaxLength(2000);

        builder.HasIndex(e => new { e.UserId, e.Status });
    }
}
