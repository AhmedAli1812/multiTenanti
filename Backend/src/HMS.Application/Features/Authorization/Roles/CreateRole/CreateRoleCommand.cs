using MediatR;
using HMS.Shared.Results;

namespace HMS.Application.Features.Authorization.Roles.CreateRole
{
    public class CreateRoleCommand : IRequest<Result<Guid>>
    {
        public string Name { get; set; } = default!;
        public List<Guid> PermissionIds { get; set; } = new();
    }
}