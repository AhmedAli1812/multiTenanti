using MediatR;

namespace HMS.Application.Features.Reception.Doctors;

public class GetDoctorsQuery : IRequest<List<DoctorLookupDto>>
{
    public Guid? BranchId { get; set; }
}