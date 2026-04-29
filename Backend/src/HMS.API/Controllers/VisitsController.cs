using HMS.Application.Features.Visits.GetVisits;
using HMS.Application.Features.Visits.Commands.DeleteVisit;
using HMS.Application.Features.Visits.Commands.FinishVisit;
using HMS.Application.Features.Visits.UpdateVisitStatus;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/visits")]
public class VisitsController : ControllerBase
{
    private readonly IMediator _mediator;

    public VisitsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ Get Visits (Pagination + Search + Status)
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetVisitsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateVisitStatusCommand command)
    {
        if (id != command.VisitId)
            return BadRequest("Id mismatch");

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPatch("{id}/finish")]
    public async Task<IActionResult> Finish(Guid id)
    {
        await _mediator.Send(new FinishVisitCommand { VisitId = id });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteVisitCommand { VisitId = id });
        return NoContent();
    }
}