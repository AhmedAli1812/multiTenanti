using HMS.API.Filters;
using HMS.Application.Features.Permissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/permissions")]
[Authorize]
public class PermissionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PermissionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetAllPermissionsQuery());
        return Ok(result);
    }

    [HttpPost("assign-to-role")]
    [AuthorizePermission("users.assignRole")]
    public async Task<IActionResult> AssignToRole([FromBody] AssignPermissionsToRoleCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}