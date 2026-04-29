using HMS.Application.Features.NurseDashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HMS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/nurse")]
public class NurseController : ControllerBase
{
    private readonly IMediator _mediator;

    public NurseController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard-stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized("TenantId claim is missing or invalid in the token.");

        var result = await _mediator.Send(new GetNurseStatsQuery { TenantId = tenantId });
        return Ok(result);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue()
    {
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized("TenantId claim is missing or invalid in the token.");

        var result = await _mediator.Send(new GetNurseQueueQuery { TenantId = tenantId });
        return Ok(result);
    }

    [HttpGet("appointments/today")]
    public async Task<IActionResult> GetTodayAppointments()
    {
        if (!TryGetTenantId(out var tenantId))
            return Unauthorized("TenantId claim is missing or invalid in the token.");

        var result = await _mediator.Send(new GetTodayAppointmentsQuery { TenantId = tenantId });
        return Ok(result);
    }

    private bool TryGetTenantId(out Guid tenantId)
    {
        tenantId = Guid.Empty;

        var tenantRaw =
            User.FindFirst("orgId")?.Value ??
            User.FindFirst("tenantId")?.Value ??
            User.FindFirst("tenant_id")?.Value ??
            User.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(tenantRaw) || !Guid.TryParse(tenantRaw, out tenantId))
            return false;

        return true;
    }
}
