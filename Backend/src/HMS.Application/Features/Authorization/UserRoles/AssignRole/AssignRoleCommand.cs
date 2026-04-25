using MediatR;
using HMS.Shared.Results;

namespace HMS.Application.Features.Authorization.UserRoles.AssignRole
{
    public class AssignRoleCommand : IRequest<Result>
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
    }
}