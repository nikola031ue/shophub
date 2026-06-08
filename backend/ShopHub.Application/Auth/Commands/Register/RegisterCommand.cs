using MediatR;
using ShopHub.Application.Auth.Dtos;

namespace ShopHub.Application.Auth.Commands.Register;

public record RegisterCommand(string Email, string Password) : IRequest<TokenDto?>;
