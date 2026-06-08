using Microsoft.EntityFrameworkCore;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Domain.Entities;

namespace ShopHub.Infrastructure.Persistence;

public class ShopHubDbContext(DbContextOptions<ShopHubDbContext> options) : DbContext(options), IShopHubDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Store> Stores => Set<Store>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShopHubDbContext).Assembly);
    }
}
