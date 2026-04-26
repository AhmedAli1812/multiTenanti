namespace HMS.Application.Features.ReceptionDashboard.Dtos;

public class ReceptionDashboardDto
{
    public DashboardKpiDto Kpis { get; set; } = new();

    public List<PreviousPatientDto> PreviousPatients { get; set; } = new();

    public List<RoomStatusDto> Rooms { get; set; } = new();

    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class DashboardKpiDto
{
    public int TotalPatients { get; set; }
    public int ActiveVisits { get; set; }
    public int OccupiedRooms { get; set; }
    public int EmergencyCases { get; set; }
}

public class PreviousPatientDto
{
    public string PatientName { get; set; } = string.Empty;
    public string MedicalNumber { get; set; } = string.Empty;
    public string? DoctorName { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public DateTime? DischargeDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RoomStatusDto
{
    public string RoomName { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}