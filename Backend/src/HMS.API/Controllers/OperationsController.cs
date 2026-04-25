using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/operations")]
public class OperationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public OperationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("rooms")]
    public async Task<IActionResult> GetRooms()
    {
        var result = await _mediator.Send(new GetRoomsDashboardQuery());
        return Ok(result);
    }
}