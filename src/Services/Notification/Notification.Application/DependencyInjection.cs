using Microsoft.Extensions.DependencyInjection;
using Notification.Application.Abstractions;
using Notification.Application.Services;

namespace Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<INotificationService, NotificationService>();
        return services;
    }
}
