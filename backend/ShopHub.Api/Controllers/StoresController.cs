using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShopHub.Application.Stores.Commands.CreateStore;
using ShopHub.Application.Stores.Commands.DeleteStore;
using ShopHub.Application.Stores.Commands.UpdateStore;
using ShopHub.Application.Stores.Queries.GetStoreById;
using ShopHub.Application.Stores.Queries.GetStores;
using ShopHub.Domain.Enums;

namespace ShopHub.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StoresController(IMediator mediator) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var stores = await mediator.Send(new GetStoresQuery(UserId), cancellationToken);
        return Ok(stores);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var store = await mediator.Send(new GetStoreByIdQuery(id, UserId), cancellationToken);
        return store is null ? NotFound() : Ok(store);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStoreRequest request, CancellationToken cancellationToken)
    {
        var id = await mediator.Send(new CreateStoreCommand(request.Name, request.Availability, request.WalletAddress, request.DatabaseType, UserId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStoreRequest request, CancellationToken cancellationToken)
    {
        var updated = await mediator.Send(new UpdateStoreCommand(id, request.Availability, request.WalletAddress, UserId), cancellationToken);
        return updated ? NoContent() : NotFound();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteStoreCommand(id, UserId), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateStoreRequest(string Name, StoreAvailability Availability, string WalletAddress, DatabaseType DatabaseType);
public record UpdateStoreRequest(StoreAvailability Availability, string WalletAddress);
