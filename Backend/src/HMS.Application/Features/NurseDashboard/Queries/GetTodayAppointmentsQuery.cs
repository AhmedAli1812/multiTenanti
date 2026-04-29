using HMS.Application.Features.NurseDashboard.Dtos;
using MediatR;

namespace HMS.Application.Features.NurseDashboard.Queries;

public class GetTodayAppointmentsQuery : IRequest<List<TodayAppointmentDto>>
{
    public Guid TenantId { get; set; }
}
