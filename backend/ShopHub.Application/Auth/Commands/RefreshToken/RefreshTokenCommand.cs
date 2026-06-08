using MediatR;
using ShopHub.Application.Auth.Dtos;

namespace ShopHub.Application.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<TokenDto?>;
