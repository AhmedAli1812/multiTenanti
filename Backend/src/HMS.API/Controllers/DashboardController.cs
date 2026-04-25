using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview(
        [FromQuery] int month,
        [FromQuery] int year)
    {
        var result = await _mediator.Send(
            new GetDashboardOverviewQuery
            {
                Month = month,
                Year = year
            });

        return Ok(result);
    }
}