using MediatR;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Application.Auth.Commands.Login;
using ShopHub.Application.Auth.Commands.RefreshToken;
using ShopHub.Application.Auth.Commands.Register;

namespace ShopHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var token = await mediator.Send(command, cancellationToken);
        return token is null ? Conflict("Email je već registrovan.") : Ok(token);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var token = await mediator.Send(command, cancellationToken);
        return token is null ? Unauthorized("Pogrešan email ili lozinka.") : Ok(token);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var token = await mediator.Send(command, cancellationToken);
        return token is null ? Unauthorized("Nevažeći ili istekli refresh token.") : Ok(token);
    }
}
