using HMS.Application.Features.PatientIntake.Commands.SubmitIntake;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using HMS.Application.Abstractions.Services;

[ApiController]
[Route("api/intake")]
public class PatientIntakeController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPdfService _pdfService;

    public PatientIntakeController(IMediator mediator, IPdfService pdfService)
    {
        _mediator = mediator;
        _pdfService = pdfService;
    }

    // 🟢 Create Intake (Draft)
    [HttpPost]
    public async Task<IActionResult> Create(CreateIntakeCommand cmd)
    {
        var id = await _mediator.Send(cmd);
        return Ok(id);
    }

    // 🟡 Update Intake
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateIntakeCommand cmd)
    {
        cmd.IntakeId = id;
        await _mediator.Send(cmd);
        return NoContent();
    }

    // 🔵 Submit Intake (يرجع Wristband DTO)
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] SubmitIntakeCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // 🖨️ Submit + Print Wristband PDF
    [HttpPost("submit-and-print")]
    public async Task<IActionResult> SubmitAndPrint([FromBody] SubmitIntakeCommand command)
    {
        var result = await _mediator.Send(command);

        var pdf = _pdfService.GenerateWristbandPdf(result);

        return File(pdf, "application/pdf", "wristband.pdf");
    }
}