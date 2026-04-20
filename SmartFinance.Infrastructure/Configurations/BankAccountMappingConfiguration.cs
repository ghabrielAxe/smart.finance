using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class BankAccountMappingConfiguration : IEntityTypeConfiguration<BankAccountMapping>
{
    public void Configure(EntityTypeBuilder<BankAccountMapping> builder)
    {
        builder.ToTable("BankAccountMappings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserId).IsRequired();

        builder.Property(b => b.BankName).IsRequired().HasMaxLength(100);
        builder.Property(b => b.CardLastDigits).HasMaxLength(10);

        builder
            .HasOne(b => b.Account)
            .WithMany()
            .HasForeignKey(b => b.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasIndex(b => new
            {
                b.UserId,
                b.BankName,
                b.CardLastDigits,
            })
            .IsUnique();
    }
}
