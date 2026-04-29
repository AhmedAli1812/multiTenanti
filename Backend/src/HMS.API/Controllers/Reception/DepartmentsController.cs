using MediatR;
using Microsoft.AspNetCore.Mvc;
using HMS.Application.Features.Reception.Departments;

namespace HMS.API.Controllers.Reception;

[ApiController]
[Route("api/departments")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // 🔥 branchId مهم هنا
    [HttpGet]
    public async Task<IActionResult> GetDepartments([FromQuery] GetDepartmentsQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(HMS.Application.Features.Reception.Departments.CreateDepartmentCommand command)
    {
        var id = await _mediator.Send(command);
        return Ok(id);
    }
}