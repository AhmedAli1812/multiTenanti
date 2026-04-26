using MediatR;
namespace HMS.Application.Features.Patients.Update;
public class UpdatePatientCommand : IRequest
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = default!;
    public string PhoneNumber { get; set; } = default!;
    public string Address { get; set; } = default!;
}