using MediatR;

namespace HMS.Application.Features.Doctors.CreateProfile;

public class CreateDoctorProfileCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public string Specialty { get; set; } = default!;
    public int YearsOfExperience { get; set; }
}