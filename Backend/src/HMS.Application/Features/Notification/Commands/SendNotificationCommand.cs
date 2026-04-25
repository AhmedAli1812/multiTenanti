using MediatR;

namespace HMS.Application.Features.Notifications.Commands.SendNotification
{
    public class SendNotificationCommand : IRequest
    {
        public Guid UserId { get; set; }

        public string Title { get; set; } = default!;
        public string Message { get; set; } = default!;
        public string Type { get; set; } = "info";

        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
    }
}