using MediatR;
using HMS.Application.Features.Patients.Common;


namespace HMS.Application.Features.Patients.GetById;

public record GetPatientByIdQuery(Guid Id) : IRequest<PatientDto>;