using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartFinance.Domain.Entities;

namespace SmartFinance.Infrastructure.Configurations;

public sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.UserId).IsRequired();

        builder.Property(a => a.Name).HasMaxLength(150).IsRequired();

        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(a => a.LockedUntil).HasColumnType("date").IsRequired(false);
    }
}
