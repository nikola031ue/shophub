using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Domain.Entities;

namespace ShopHub.Infrastructure.Persistence;

public class ShopHubDbContext(DbContextOptions<ShopHubDbContext> options) : DbContext(options), IShopHubDbContext
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopHubDbContext).Assembly);
    }
}
