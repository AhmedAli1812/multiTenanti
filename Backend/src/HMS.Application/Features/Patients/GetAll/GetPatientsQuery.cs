using MediatR;
using HMS.Application.Features.Patients.Common;

namespace HMS.Application.Features.Patients.GetAll;
public class GetPatientsQuery : IRequest<PaginatedResult<PatientDto>>
{
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}