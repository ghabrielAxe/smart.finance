using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public class IndexRateConfiguration : IEntityTypeConfiguration<IndexRate>
{
    public void Configure(EntityTypeBuilder<IndexRate> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.IndexName).IsRequired().HasMaxLength(20); // Ex: "INCC", "IPCA"
        builder.Property(i => i.ReferenceMonth).IsRequired();

        builder.OwnsOne(
            i => i.Rate,
            percentage =>
            {
                percentage
                    .Property(p => p.Value)
                    .HasColumnName("RateValue")
                    .HasPrecision(7, 6)
                    .IsRequired(); // Permite índices muito quebrados
            }
        );
    }
}
