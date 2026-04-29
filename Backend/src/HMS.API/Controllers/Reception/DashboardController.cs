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
        // FIX: JWT emits tenantId under claim key "orgId" (set by JwtService).
        // Previously used "tenantId" which was always null → NullReferenceException.
        // Try all possible claim keys in the same priority order as TenantProvider.
        var tenantRaw =
            User.FindFirst("orgId")?.Value ??
            User.FindFirst("tenantId")?.Value ??
            User.FindFirst("tenant_id")?.Value ??
            User.FindFirst("TenantId")?.Value;

        if (string.IsNullOrEmpty(tenantRaw) || !Guid.TryParse(tenantRaw, out var tenantId))
            return Unauthorized("TenantId claim is missing or invalid in the token.");

        query.TenantId = tenantId;

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}