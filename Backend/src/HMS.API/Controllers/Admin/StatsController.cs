using HMS.Application.Features.Admin.Dashboard;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers.Admin;

[ApiController]
[Route("api/admin/stats")]
[Authorize]
public class StatsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetStats([FromQuery] Guid? tenantId, [FromQuery] Guid? branchId)
    {
        var result = await _mediator.Send(new GetAdminDashboardStatsQuery
        {
            TenantId = tenantId,
            BranchId = branchId
        });
        return Ok(result);
    }
}
