public interface IRealTimeNotifier
{
    Task SendToUserAsync(Guid userId, object data);
    Task SendToGroupAsync(string group, object data);
}