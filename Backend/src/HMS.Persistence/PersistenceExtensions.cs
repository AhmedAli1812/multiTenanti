using HMS.Identity.Application.Abstractions;
using HMS.Intake.Application.Abstractions;
using HMS.Rooms.Application.Abstractions;
using HMS.SharedKernel.Infrastructure.Persistence.Interceptors;
using HMS.SharedKernel.Infrastructure.Tenancy;
using HMS.Visits.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Persistence;

/// <summary>
/// HMS.Persistence DI registration — registers HmsDbContext and all module interface mappings.
/// Called from Program.cs: builder.Services.AddHmsPersistence(config)
/// </summary>
public static class PersistenceExtensions
{
    public static IServiceCollection AddHmsPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Tenant resolution ──────────────────────────────────────────────────
        services.AddScoped<ITenantProvider, TenantProvider>();

        // ── Domain event + audit interceptor ──────────────────────────────────
        services.AddScoped<AuditAndDomainEventInterceptor>();

        // ── HmsDbContext ───────────────────────────────────────────────────────
        services.AddDbContext<HmsDbContext>((sp, options) =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql =>
                {
                    sql.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
                    sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                });

            options.AddInterceptors(sp.GetRequiredService<AuditAndDomainEventInterceptor>());

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                options.EnableDetailedErrors().EnableSensitiveDataLogging();
        });

        // ── Map to all module slice interfaces ────────────────────────────────
        services.AddScoped<IIdentityDbContext>(sp => sp.GetRequiredService<HmsDbContext>());
        services.AddScoped<IVisitsDbContext>  (sp => sp.GetRequiredService<HmsDbContext>());
        services.AddScoped<IRoomsDbContext>   (sp => sp.GetRequiredService<HmsDbContext>());
        services.AddScoped<IIntakeDbContext>  (sp => sp.GetRequiredService<HmsDbContext>());

        // ── SharedKernel ICurrentUser for AuditInterceptor ────────────────────
        services.AddScoped<ICurrentUser, HMS.SharedKernel.Infrastructure.CurrentUser.HttpCurrentUser>();

        return services;
    }
}
