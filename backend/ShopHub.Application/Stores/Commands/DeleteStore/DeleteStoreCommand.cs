using MediatR;

namespace ShopHub.Application.Stores.Commands.DeleteStore;

public record DeleteStoreCommand(Guid Id, Guid UserId) : IRequest<bool>;
