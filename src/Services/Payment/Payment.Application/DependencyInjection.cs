using Microsoft.Extensions.DependencyInjection;
using Payment.Application.Abstractions;
using Payment.Application.Services;

namespace Payment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IPaymentService, PaymentService>();
        return services;
    }
}
