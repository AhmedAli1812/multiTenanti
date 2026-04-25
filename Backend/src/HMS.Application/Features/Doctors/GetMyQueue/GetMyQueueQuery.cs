using MediatR;
using HMS.Application.Features.Doctors.Common;

public record GetMyQueueQuery() : IRequest<List<DoctorQueueDto>>;