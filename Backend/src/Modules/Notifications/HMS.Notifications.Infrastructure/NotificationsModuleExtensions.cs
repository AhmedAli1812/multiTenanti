using HMS.Notifications.Application;
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace HMS.Notifications.Infrastructure;

/// <summary>Notifications module DI registration.</summary>
public static class NotificationsModuleExtensions
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        // Register the SignalR notification service
        services.AddScoped<INotificationsService, SignalRNotificationsService>();

        // Register event handlers from the Application assembly
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(
                typeof(HMS.Notifications.Application.EventHandlers
                    .VisitCreatedNotificationHandler).Assembly));

        return services;
    }
}
