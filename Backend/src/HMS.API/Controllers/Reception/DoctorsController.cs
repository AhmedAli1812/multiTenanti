using MediatR;
using Microsoft.AspNetCore.Mvc;
using HMS.Application.Features.Reception.Doctors;

namespace HMS.API.Controllers.Reception;

[ApiController]
[Route("api/doctors")]
public class DoctorsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DoctorsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetDoctors([FromQuery] GetDoctorsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}