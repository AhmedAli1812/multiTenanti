using Microsoft.AspNetCore.SignalR;
using HMS.Application.Abstractions.Services;
using HMS.Infrastructure.RealTime;
public class SignalRNotifier : IRealTimeNotifier
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRNotifier(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public async Task SendToUserAsync(Guid userId, object data)
    {
        await _hub.Clients
            .User(userId.ToString())
            .SendAsync("ReceiveNotification", data);
    }
    public async Task SendToGroupAsync(string group, object data)
    {
        await _hub.Clients
            .Group(group)
            .SendAsync("ReceiveNotification", data);
    }
}