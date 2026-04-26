using MediatR;
using HMS.Application.Dtos;

public record GetAllRolesQuery : IRequest<List<RoleDto>>;