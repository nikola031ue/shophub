using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShopHub.Infrastructure.Persistence;
using Testcontainers.PostgreSql;

namespace ShopHub.Tests.Integration.Infrastructure;

public class ShopHubApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:16")
        .WithDatabase("shophub_test")
        .WithUsername("shophub")
        .WithPassword("shophub123")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                ["Jwt:SecretKey"] = "test-secret-key-min-32-characters-long!!"
            });
        });
    }

    public async Task<string> GetUserTokenAsync(string email = "test@shophub.com", string password = "Password1!")
    {
        using var client = CreateClient();
        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new { email, password });
        if (!registerResponse.IsSuccessStatusCode)
        {
            var loginResponse = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
            var loginBody = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
            return loginBody.GetProperty("accessToken").GetString()!;
        }
        var body = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("accessToken").GetString()!;
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ShopHubDbContext>();
        await db.Users.ExecuteDeleteAsync();
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();
    public new async Task DisposeAsync() => await _postgres.DisposeAsync();
}
