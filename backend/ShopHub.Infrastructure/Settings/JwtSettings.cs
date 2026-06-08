namespace ShopHub.Infrastructure.Settings;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; init; } = string.Empty;
    public string Issuer { get; init; } = "ShopHubApi";
    public string Audience { get; init; } = "ShopHubClient";
    public int AccessTokenExpirationMinutes { get; init; } = 60;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
