using HMS.Application.Features.Rooms.CreateRoom;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers;

[ApiController]
[Route("api/rooms")]
[Authorize]
public class RoomsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoomsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ Create Room
    [HttpPost]
    public async Task<IActionResult> Create(CreateRoomCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }

    // ✅ Get Rooms
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetRoomsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ✅ Assign Room
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(AssignRoomCommand command)
    {
        await _mediator.Send(command);
        return Ok();
    }
}