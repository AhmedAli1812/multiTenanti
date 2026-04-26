using MediatR;
namespace HMS.Application.Features.Patients.Delete;
public record DeletePatientCommand(Guid Id) : IRequest;