using MediatR;

namespace HMS.Application.Features.Visits.Events
{
    public class VisitCreatedEvent : INotification
    {
        public Guid TenantId { get; set; }
        public Guid PatientId { get; set; }
        public Guid VisitId { get; set; }
        public Guid NurseId { get; set; }
        public Guid DoctorId { get; set; }
        public Guid RoomId { get; set; }
        public Guid BranchId { get; set; }

    }
}