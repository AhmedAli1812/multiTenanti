using MediatR;

namespace HMS.Application.Features.Users.GetUsers;

public class GetUsersQuery : IRequest<PaginatedResult<UserDto>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public string? Role { get; set; }
}