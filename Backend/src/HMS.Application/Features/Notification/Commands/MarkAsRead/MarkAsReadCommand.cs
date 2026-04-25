using MediatR;

public class MarkAsReadCommand : IRequest
{
    public Guid NotificationId { get; set; }
}