using HMS.API.Filters;
using HMS.Application.Features.Patients.Create;
using HMS.Application.Features.Patients.Delete;
using HMS.Application.Features.Patients.GetAll;
using HMS.Application.Features.Patients.GetById;
using HMS.Application.Features.Patients.Update;
using HMS.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PatientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ✅ Create Patient
    [Authorize]
    [AuthorizePermission(Permissions.PatientsCreate)]
    [HttpPost]
    public async Task<IActionResult> Create([FromForm] CreatePatientCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    // ✅ Get All (Pagination + Search)
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetPatientsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    // ✅ Get By Id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetPatientByIdQuery(id));
        return Ok(result);
    }

    // ✅ Update Patient
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientCommand command)
    {
        if (id != command.Id)
            return BadRequest("Route id and body id mismatch");

        await _mediator.Send(command);
        return NoContent();
    }

    // ✅ Soft Delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeletePatientCommand(id));
        return NoContent();
    }
}