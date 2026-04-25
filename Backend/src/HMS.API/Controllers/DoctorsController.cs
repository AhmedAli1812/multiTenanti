using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("my-queue")]
    public async Task<IActionResult> GetMyQueue()
    {
        var result = await _mediator.Send(new GetMyQueueQuery());
        return Ok(result);
    }
}