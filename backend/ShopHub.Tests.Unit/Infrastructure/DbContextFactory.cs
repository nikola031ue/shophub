using Microsoft.EntityFrameworkCore;
using ShopHub.Infrastructure.Persistence;

namespace ShopHub.Tests.Unit.Infrastructure;

public static class DbContextFactory
{
    public static ShopHubDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ShopHubDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ShopHubDbContext(options);
    }
}
