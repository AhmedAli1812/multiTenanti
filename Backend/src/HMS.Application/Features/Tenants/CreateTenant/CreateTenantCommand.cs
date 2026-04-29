using MediatR;
using HMS.Shared.Results;

namespace HMS.Application.Features.Tenants.CreateTenant;

public record CreateTenantCommand(string Name, string Code) : IRequest<Result<Guid>>;
