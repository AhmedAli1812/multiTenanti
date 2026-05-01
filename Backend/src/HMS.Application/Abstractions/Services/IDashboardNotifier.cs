public interface IDashboardNotifier
{
    Task NotifyDashboardUpdated(Guid tenantId);

    // 🔥 New Events
    Task NotifyNewVisit(Guid tenantId, Guid branchId);

    Task NotifyDoctorQueue(Guid doctorId);

    Task NotifyRoomAssigned(Guid tenantId, Guid branchId);
    Task NotifyRoomStatusChanged(Guid tenantId, Guid branchId);
    Task NotifyPatientDischarged(Guid tenantId, Guid branchId, Guid visitId);
}