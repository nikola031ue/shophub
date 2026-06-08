using MediatR;
using ShopHub.Application.Auth.Dtos;

namespace ShopHub.Application.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<TokenDto?>;
