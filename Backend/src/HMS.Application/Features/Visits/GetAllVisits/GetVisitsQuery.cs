using MediatR;
using HMS.Application.Features.Patients.Common;
using HMS.Application.Dtos;

namespace HMS.Application.Features.Visits.GetVisits;

public class GetVisitsQuery : IRequest<PaginatedResult<VisitListDto>>
{
    public string? Search { get; set; }
    public string? Status { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}