using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditController(IMediator mediator)
    {
        _mediator = mediator;
    }
    [Authorize(Policy = "ViewAuditLogs")]
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] GetAuditLogsQuery query)
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}