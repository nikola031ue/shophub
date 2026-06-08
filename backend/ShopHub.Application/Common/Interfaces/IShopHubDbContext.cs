using Microsoft.EntityFrameworkCore;
using ShopHub.Domain.Entities;

namespace ShopHub.Application.Common.Interfaces;

public interface IShopHubDbContext
{
    DbSet<User> Users { get; }
    DbSet<Store> Stores { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
