using HMS.Application.Abstractions.Auth;
using HMS.Application.Abstractions.Tenant;
using HMS.Infrastructure.Authentication;
using HMS.Infrastructure.Tenancy;
using HMS.Application.Abstractions;
using HMS.Infrastructure.Services;  
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // 🧨 Tenant Provider (Scoped per request)
        services.AddScoped<TenantProvider>();
        services.AddScoped<ITenantProvider>(sp =>
            sp.GetRequiredService<TenantProvider>());


        // ⬇️ هنا بعدين هنضيف:
        // - DbContext
        // - Authentication
        // - Authorization
        // - Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        return services;
    }
}