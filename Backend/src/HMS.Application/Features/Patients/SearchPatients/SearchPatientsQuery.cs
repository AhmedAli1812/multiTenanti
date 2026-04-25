using MediatR;

public class SearchPatientsQuery : IRequest<List<PatientSearchDto>>
{
    public string? Term { get; set; }
}