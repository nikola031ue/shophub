using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ShopHub.Tests.Integration.Infrastructure;

namespace ShopHub.Tests.Integration.Stores;

public class StoresApiTests(ShopHubApiFactory factory) : IClassFixture<ShopHubApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        await factory.ResetDatabaseAsync();
        var token = await factory.GetUserTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAll_ShouldReturn200_WithEmptyList()
    {
        var response = await _client.GetAsync("/api/stores");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldReturn401_WhenNotAuthenticated()
    {
        var client = factory.CreateClient();
        var response = await client.GetAsync("/api/stores");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Post_ShouldReturn201_AndCreateStore()
    {
        var response = await _client.PostAsJsonAsync("/api/stores",
            new { name = "My Shop", availability = 2, walletAddress = "0xABC123", databaseType = 0 });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("id").GetGuid().Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetById_ShouldReturn200_WhenStoreExists()
    {
        var id = await CreateStoreAsync("Test Store");

        var response = await _client.GetAsync($"/api/stores/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Test Store");
        body.GetProperty("status").GetString().Should().Be("Pending");
    }

    [Fact]
    public async Task GetById_ShouldReturn404_WhenNotFound()
    {
        var response = await _client.GetAsync($"/api/stores/{Guid.NewGuid()}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Put_ShouldReturn204_AndUpdateStore()
    {
        var id = await CreateStoreAsync("Old Name");

        var response = await _client.PutAsJsonAsync($"/api/stores/{id}",
            new { availability = 3, walletAddress = "0xNEW" });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var body = await _client.GetFromJsonAsync<JsonElement>($"/api/stores/{id}");
        body.GetProperty("availability").GetString().Should().Be("High");
        body.GetProperty("walletAddress").GetString().Should().Be("0xNEW");
    }

    [Fact]
    public async Task Put_ShouldReturn404_WhenNotFound()
    {
        var response = await _client.PutAsJsonAsync($"/api/stores/{Guid.NewGuid()}",
            new { availability = 2, walletAddress = "0xABC" });
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturn204_AndRemoveStore()
    {
        var id = await CreateStoreAsync("To Delete");

        var response = await _client.DeleteAsync($"/api/stores/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await _client.GetAsync($"/api/stores/{id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_ShouldNotReturnStoresOfOtherUsers()
    {
        await CreateStoreAsync("My Store");

        var otherToken = await factory.GetUserTokenAsync("other@shophub.com", "Password1!");
        var otherClient = factory.CreateClient();
        otherClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", otherToken);
        await otherClient.PostAsJsonAsync("/api/stores",
            new { name = "Other Store", availability = 2, walletAddress = "0xOTHER", databaseType = 0 });

        var response = await _client.GetAsync("/api/stores");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetArrayLength().Should().Be(1);
        body[0].GetProperty("name").GetString().Should().Be("My Store");
    }

    private async Task<Guid> CreateStoreAsync(string name)
    {
        var response = await _client.PostAsJsonAsync("/api/stores",
            new { name, availability = 2, walletAddress = "0xABC123", databaseType = 0 });
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }
}
