using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.UserId).IsRequired();

        builder.Property(t => t.Description).IsRequired().HasMaxLength(255);

        builder
            .Property(t => t.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(t => t.IdempotencyKey).HasMaxLength(100);

        builder
            .HasIndex(t => new { t.UserId, t.IdempotencyKey })
            .IsUnique()
            .HasFilter("\"IdempotencyKey\" IS NOT NULL");

        builder.HasIndex(t => new { t.UserId, t.Date });

        builder
            .HasOne(t => t.Category)
            .WithMany()
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasOne(t => t.Installment)
            .WithMany()
            .HasForeignKey(t => t.InstallmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder
            .HasMany(t => t.Entries)
            .WithOne(e => e.Transaction)
            .HasForeignKey(e => e.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
