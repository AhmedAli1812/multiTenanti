using HMS.Visits.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Visits.Infrastructure;

/// <summary>Visits module DI registration.</summary>
public static class VisitsModuleExtensions
{
    public static IServiceCollection AddVisitsModule(this IServiceCollection services)
    {
        // IVisitsDbContext registered in HMS.Persistence.PersistenceExtensions
        // FluentValidation registered in Program.cs via AddValidatorsFromAssembly
        services.AddScoped<ICurrentUserContext>(sp =>
        {
            var legacy = sp.GetRequiredService<HMS.Application.Abstractions.CurrentUser.ICurrentUser>();
            return new CurrentUserContextAdapter(legacy);
        });
        return services;
    }
}

internal sealed class CurrentUserContextAdapter(
    HMS.Application.Abstractions.CurrentUser.ICurrentUser legacy)
    : ICurrentUserContext
{
    public Guid? UserId   => legacy.UserId == Guid.Empty ? null : legacy.UserId;
    public Guid? TenantId => legacy.TenantId == Guid.Empty ? null : legacy.TenantId;
    public bool  IsGlobal => legacy.IsGlobal;
}
