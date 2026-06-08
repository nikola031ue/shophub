using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopHub.Application.Auth.Interfaces;
using ShopHub.Application.Common.Interfaces;
using ShopHub.Infrastructure.Persistence;
using ShopHub.Infrastructure.Services;
using ShopHub.Infrastructure.Settings;

namespace ShopHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ShopHubDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IShopHubDbContext>(sp => sp.GetRequiredService<ShopHubDbContext>());

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.AddScoped<IJwtService, JwtService>();
        services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}
