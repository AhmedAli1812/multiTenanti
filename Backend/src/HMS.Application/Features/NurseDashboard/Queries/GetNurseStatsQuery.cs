using HMS.Application.Features.NurseDashboard.Dtos;
using MediatR;

namespace HMS.Application.Features.NurseDashboard.Queries;

public class GetNurseStatsQuery : IRequest<NurseStatsDto>
{
    public Guid TenantId { get; set; }
}
