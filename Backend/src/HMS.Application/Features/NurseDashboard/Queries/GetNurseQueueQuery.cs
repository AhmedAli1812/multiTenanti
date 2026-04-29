using HMS.Application.Features.NurseDashboard.Dtos;
using MediatR;

namespace HMS.Application.Features.NurseDashboard.Queries;

public class GetNurseQueueQuery : IRequest<List<QueuePatientDto>>
{
    public Guid TenantId { get; set; }
}
