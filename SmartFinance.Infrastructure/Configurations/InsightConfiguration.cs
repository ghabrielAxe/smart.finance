using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class InsightConfiguration : IEntityTypeConfiguration<Insight>
{
    public void Configure(EntityTypeBuilder<Insight> builder)
    {
        builder.ToTable("Insights");

        builder.HasKey(i => i.Id);


        builder.Property(i => i.UserId).IsRequired();


        builder.Property(i => i.Type).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(i => i.Severity).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(i => i.Title).HasMaxLength(255).IsRequired();

 
        builder.Property(i => i.Message).HasColumnType("text").IsRequired();


        builder.Property(i => i.ActionableDataJson).HasColumnType("jsonb").IsRequired(false);

        builder.Property(i => i.ReferenceDate).HasColumnType("date").IsRequired();


        builder
            .HasIndex(i => new
            {
                i.UserId,
                i.Type,
                i.ReferenceDate,
            })
            .IsUnique();
    }
}
