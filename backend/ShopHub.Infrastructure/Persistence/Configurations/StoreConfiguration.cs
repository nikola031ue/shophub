using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ShopHub.Domain.Entities;

namespace ShopHub.Infrastructure.Persistence.Configurations;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Name).IsRequired().HasMaxLength(100);
        builder.Property(s => s.WalletAddress).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Url).HasMaxLength(500);
        builder.Property(s => s.Availability).HasConversion<string>();
        builder.Property(s => s.DatabaseType).HasConversion<string>();
        builder.Property(s => s.Status).HasConversion<string>();
        builder.HasOne<User>().WithMany().HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
