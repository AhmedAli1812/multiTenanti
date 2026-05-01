namespace HMS.Application.Features.NurseDashboard.Dtos;

public class NurseStatsDto
{
    public int TotalPatientsToday { get; set; }
    public int WaitingPatients { get; set; }
    public int UpcomingAppointments { get; set; }
    public int EmergencyCases { get; set; }
}

public class QueuePatientDto
{
    public Guid VisitId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string NationalId { get; set; } = string.Empty;
    public DateTime ArrivalTime { get; set; }
    public string VisitTypeName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string PriorityName { get; set; } = string.Empty;
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }
    public int QueueNumber { get; set; }
}

public class TodayAppointmentDto
{
    public Guid VisitId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string DoctorName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime ScheduledTime { get; set; }
    public int QueueNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ChiefComplaint { get; set; }
    public string? Notes { get; set; }
    public string VisitTypeName { get; set; } = string.Empty;
}
