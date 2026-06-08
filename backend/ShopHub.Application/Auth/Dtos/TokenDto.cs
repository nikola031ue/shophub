namespace ShopHub.Application.Auth.Dtos;

public record TokenDto(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry);
