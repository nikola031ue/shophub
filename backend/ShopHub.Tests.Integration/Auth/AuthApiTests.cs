using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using ShopHub.Tests.Integration.Infrastructure;

namespace ShopHub.Tests.Integration.Auth;

public class AuthApiTests(ShopHubApiFactory factory) : IClassFixture<ShopHubApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client = factory.CreateClient();

    public Task InitializeAsync() => factory.ResetDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task Register_ShouldReturn200_WithTokens_WhenEmailIsNew()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { email = "new@shophub.com", password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_ShouldReturn409_WhenEmailAlreadyExists()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new { email = "dup@shophub.com", password = "Password1!" });

        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new { email = "dup@shophub.com", password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ShouldReturn200_WithTokens_WhenCredentialsAreValid()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new { email = "login@shophub.com", password = "Password1!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "login@shophub.com", password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenPasswordIsWrong()
    {
        await _client.PostAsJsonAsync("/api/auth/register",
            new { email = "wrong@shophub.com", password = "Password1!" });

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "wrong@shophub.com", password = "WrongPassword!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturn401_WhenUserNotFound()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new { email = "nonexistent@shophub.com", password = "Password1!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ShouldReturn200_WithNewTokens()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new { email = "refresh@shophub.com", password = "Password1!" });
        var registerBody = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = registerBody.GetProperty("accessToken").GetString()!;
        var refreshToken = registerBody.GetProperty("refreshToken").GetString()!;

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { accessToken, refreshToken });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Refresh_ShouldReturn401_WhenRefreshTokenIsInvalid()
    {
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register",
            new { email = "badrefresh@shophub.com", password = "Password1!" });
        var body = await registerResponse.Content.ReadFromJsonAsync<JsonElement>();
        var accessToken = body.GetProperty("accessToken").GetString()!;

        var response = await _client.PostAsJsonAsync("/api/auth/refresh",
            new { accessToken, refreshToken = "invalid-refresh-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
