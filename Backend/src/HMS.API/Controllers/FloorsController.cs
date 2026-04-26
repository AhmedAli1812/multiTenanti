using HMS.Application.Features.Floors.CreateFloor;
using HMS.Application.Features.Floors.GetFloorsByBranch;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers;

[ApiController]
[Route("api/floors")]
[Authorize]
public class FloorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public FloorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFloorCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }

    [HttpGet("by-branch")]
    public async Task<IActionResult> GetByBranch([FromQuery] Guid branchId)
    {
        var result = await _mediator.Send(new GetFloorsByBranchQuery
        {
            BranchId = branchId
        });

        return Ok(result);
    }
}