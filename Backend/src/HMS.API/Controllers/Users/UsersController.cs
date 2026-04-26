using HMS.API.Filters;
using HMS.Application.Features.Authorization.UserRoles.AssignRole;
using HMS.Application.Features.Users.Create;
using HMS.Application.Features.Users.CreateUser;
using HMS.Application.Features.Users.DeleteUser;
using HMS.Application.Features.Users.GetUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers.Users
{
    [ApiController]
    [Route("api/users")]
    [Authorize] // 🔐 كل الاندبوينتس محتاجة توكن
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // =====================================================
        // 🔥 Create User
        // =====================================================
        [HttpPost]
        [AuthorizePermission("users.create")]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        {
            var id = await _mediator.Send(command);

            return Ok(new
            {
                message = "User created successfully",
                userId = id
            });
        }

        // =====================================================
        // 🔥 Soft Delete User
        // =====================================================
        [HttpDelete("{id}")]
        [AuthorizePermission("users.delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteUserCommand(id));

            if (!result.IsSuccess)
                return BadRequest(result);

            return Ok(new
            {
                message = "User deleted successfully"
            });
        }
        // 🔥 Get Users with Pagination
        [HttpGet]
        [AuthorizePermission("users.view")]
        public async Task<IActionResult> Get([FromQuery] GetUsersQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // =====================================================
        // 🔥 Assign Multiple Roles
        // =====================================================
        [HttpPost("{id}/roles")]
        [AuthorizePermission("users.assign-role")]
        public async Task<IActionResult> AssignRoles(Guid id, [FromBody] AssignRolesRequest request)
        {
            if (request.RoleIds == null || !request.RoleIds.Any())
                return BadRequest(new { message = "At least one role is required" });

            foreach (var roleId in request.RoleIds)
            {
                var result = await _mediator.Send(new AssignRoleCommand
                {
                    UserId = id,
                    RoleId = roleId
                });

                if (!result.IsSuccess)
                    return BadRequest(result);
            }

            return Ok(new
            {
                message = "Roles assigned successfully"
            });
        }
    }


    // =====================================================
    // 🔥 Request DTO
    // =====================================================
    public class AssignRolesRequest
    {
        public List<Guid> RoleIds { get; set; } = new();
    }
}