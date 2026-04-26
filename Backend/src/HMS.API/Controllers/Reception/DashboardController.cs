using HMS.Application.Features.ReceptionDashboard.Queries;
using HMS.Domain.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers;

[ApiController]
[Authorize(Policy = "DashboardReceptionView")]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("reception")]
    public async Task<IActionResult> GetReceptionDashboard(
        [FromQuery] GetReceptionDashboardQuery query)
    {
        query.TenantId = Guid.Parse(User.FindFirst("tenantId")!.Value);

        var result = await _mediator.Send(query);

        return Ok(result);
    }
}