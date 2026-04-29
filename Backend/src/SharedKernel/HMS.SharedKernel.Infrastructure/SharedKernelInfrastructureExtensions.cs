using HMS.SharedKernel.Infrastructure.Persistence;
using HMS.SharedKernel.Infrastructure.Tenancy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.SharedKernel.Infrastructure;

/// <summary>
/// SharedKernel infrastructure DI — registers only SharedKernel-level services.
/// Does NOT reference module projects (avoids circular deps).
/// Module interface mappings are done in HMS.Persistence.PersistenceExtensions.
/// </summary>
public static class SharedKernelInfrastructureExtensions
{
    public static IServiceCollection AddSharedKernelInfrastructure(
        this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<Persistence.Interceptors.AuditAndDomainEventInterceptor>();
        services.AddScoped<Persistence.Interceptors.ICurrentUser,
                           CurrentUser.HttpCurrentUser>();
        return services;
    }
}
